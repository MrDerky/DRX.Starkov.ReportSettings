using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ScheduledReports.Server
{
  public class ModuleAsyncHandlers
  {

    /// <summary>
    /// Отправка отчета по расписанию
    /// </summary>
    /// <param name="args"></param>
    public virtual void SendSheduleReport(Starkov.ScheduledReports.Server.AsyncHandlerInvokeArgs.SendSheduleReportInvokeArgs args)
    {
      var logInfo = string.Format("SendSheduleReport. SheduleSettingId = {0}.", args.SheduleSettingId);
      Logger.DebugFormat("{0} Start.", logInfo);
      
      var setting = PublicFunctions.Module.Remote.GetScheduleSetting(args.SheduleSettingId);
      if (setting == null)
      {
        Logger.DebugFormat("{0} Не удалось получить действующую запись справочника SheduleSetting.", logInfo);
        args.Retry = false;
        return;
      }
      
      var scheduleLog = ScheduleLogs.GetAll(s => s.ScheduleSettingId == setting.Id)
        .Where(s => s.Status == ScheduledReports.ScheduleLog.Status.Waiting || s.Status == ScheduledReports.ScheduleLog.Status.Error)
        .OrderByDescending(s => s.StartDate)
        .FirstOrDefault();
      
      if (scheduleLog == null)
      {
        Logger.DebugFormat("{0} Не найдено записей справочника ScheduleLog со статусом Waiting.", logInfo);
        if (!Locks.TryLock(setting))
        {
          Logger.DebugFormat("{0} Запись справочника ScheduleSetting заблокирована пользователем {1}.", logInfo, Locks.GetLockInfo(setting).OwnerName);
          args.Retry = true;
          return;
        }
        
        setting.Status = ScheduledReports.ScheduleSetting.Status.Closed;
        setting.Save();
        
        if (Locks.GetLockInfo(setting).IsLockedByMe)
          Locks.Unlock(setting);
        
        args.Retry = false;
        return;
      }
      
      if (Calendar.Now < scheduleLog.StartDate.Value)
      {
        args.NextRetryTime = scheduleLog.StartDate.Value;
        args.Retry = true;
        Logger.DebugFormat("{0} Запуск отложен до {1}.", logInfo, scheduleLog.StartDate.Value);
        return;
      }
      
      if (!Functions.Module.ScheduleLogExecute(setting, scheduleLog, logInfo))
      {
        Logger.DebugFormat("{0}. scheduleLog={1}. Ошибка при обработке.", logInfo, scheduleLog.Id);
        
        // HACK Обход платформенного бага при генерации отчетов
        if (scheduleLog.Comment.Contains("System.NullReferenceException"))
          args.NextRetryTime = Calendar.Now.AddSeconds(10);
        
        args.Retry = args.RetryIteration < 100;
        return;
      }
      
      Logger.DebugFormat("{0} Done.", logInfo);
    }

  }
}