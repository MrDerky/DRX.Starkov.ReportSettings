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
      args.Retry = false;
      var logInfo = string.Format("SendSheduleReport. ScheduleLogId = {0}.", args.ScheduleLogId);
      Logger.DebugFormat("{0} Start. RetryIteration={1}", logInfo, args.RetryIteration);
      
      var scheduleLog = ScheduleLogs.GetAll(s => s.Id == args.ScheduleLogId).FirstOrDefault();
      if (scheduleLog != null)
        logInfo = string.Format("{0} SheduleSetting = {1}.", logInfo, scheduleLog.ScheduleSettingId);
      
      if (scheduleLog != null && scheduleLog.Status != ScheduledReports.ScheduleLog.Status.Waiting && scheduleLog.Status != ScheduledReports.ScheduleLog.Status.Error)
      {
        Logger.DebugFormat("{0} Запись справочника ScheduleLog в статусе {1}", logInfo, scheduleLog.Status);
        
        var setting = PublicFunctions.ScheduleSetting.Remote.GetScheduleSetting(scheduleLog.ScheduleSettingId);
        if (setting == null)
        {
          Logger.DebugFormat("{0} Не удалось получить действующую запись справочника SheduleSetting.", logInfo);
          if (Locks.TryLock(scheduleLog))
          {
            scheduleLog.Status = ScheduledReports.ScheduleLog.Status.Closed;
            scheduleLog.Save();
            
            if (Locks.GetLockInfo(scheduleLog).IsLockedByMe)
              Locks.Unlock(scheduleLog);
            
            return;
          }
          else
          {
            args.Retry = args.RetryIteration < 100;
            return;
          }
        }
        
        if (!ScheduleLogs.GetAll(s => s.ScheduleSettingId == setting.Id)
            .Any(s => s.Status == ScheduledReports.ScheduleLog.Status.Waiting || s.Status == ScheduledReports.ScheduleLog.Status.Error))
        {
          Logger.DebugFormat("{0} Не найдено записей справочника ScheduleLog со статусом Waiting или Error.", logInfo);
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
          
          return;
        }
      }
      
      if (Calendar.Now < scheduleLog.StartDate.Value)
      {
        args.NextRetryTime = scheduleLog.StartDate.Value;
        args.Retry = true;
        Logger.DebugFormat("{0} Запуск отложен до {1}.", logInfo, scheduleLog.StartDate.Value);
        return;
      }
      
      if (!Functions.Module.ScheduleLogExecute(scheduleLog, logInfo))
      {
        args.Retry = args.RetryIteration < 100;
        // HACK Обход платформенного бага при генерации отчетов
        if (!string.IsNullOrEmpty(scheduleLog.Comment) && scheduleLog.Comment.Contains("Object reference not set to an instance of an object."))
        {
          args.NextRetryTime = Calendar.Now.AddMinutes(1);
          Logger.DebugFormat("{0} scheduleLog={1}. Обработка ошибки Object reference not set to an instance of an object. Следующий запуск {2}", logInfo, scheduleLog.Id,
                             args.Retry == true ? args.NextRetryTime.ToString() : "отменен из-за превышения количества попыток");
        }
        return;
      }
      
      Logger.DebugFormat("{0} Done.", logInfo);
    }

  }
}