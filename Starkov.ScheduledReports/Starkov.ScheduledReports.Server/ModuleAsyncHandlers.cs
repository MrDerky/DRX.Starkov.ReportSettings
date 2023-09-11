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
      var logInfo = string.Format("SendSheduleReport. SheduleReportId = {0}.", args.SheduleSettingId);
      Logger.DebugFormat("{0} Start.", logInfo);
      
      var setting = PublicFunctions.Module.Remote.GetScheduleSetting(args.SheduleSettingId);
      if (setting == null)
      {
        Logger.DebugFormat("{0}. Не удалось получить действующую запись справочника SheduleSetting.", logInfo);
        args.Retry = false;
        return;
      }
      
      var schedluleLog = ScheduleLogs.GetAll(s => s.ScheduleSettingId == setting.Id)
        .Where(s => s.Status == ScheduledReports.ScheduleLog.Status.Waiting || s.Status == ScheduledReports.ScheduleLog.Status.Error)
        .OrderByDescending(s => s.StartDate)
        .FirstOrDefault();
      
      if (schedluleLog == null)
      {
        Logger.DebugFormat("{0}. Не найдено записей справочника ScheduleLog со статусом Waiting.", logInfo);
        if (!Locks.TryLock(setting))
        {
          Logger.DebugFormat("{0}. Запись справочника ScheduleSetting заблокирована пользователем {1}.", logInfo, Locks.GetLockInfo(setting).OwnerName);
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
      
      if (Calendar.Now < schedluleLog.StartDate.Value)
      {
        args.NextRetryTime = schedluleLog.StartDate.Value;
        args.Retry = true;
        Logger.DebugFormat("{0}. Запуск отложен до {1}.", logInfo, schedluleLog.StartDate.Value);
        return;
      }
      
      if (!Locks.TryLock(schedluleLog))
      {
        Logger.DebugFormat("{0}. Запись справочника SchedluleLog заблокирована пользователем {1}.", logInfo, Locks.GetLockInfo(schedluleLog).OwnerName);
        args.Retry = true;
        return;
      }
      
      try
      {
        schedluleLog.Comment = string.Format("Запуск {0}", Calendar.Now);
        
        PublicFunctions.Module.StartSheduleReport(setting, schedluleLog);
        PublicFunctions.Module.EnableSchedule(setting);
      }
      catch (Exception ex)
      {
        args.Retry = args.RetryIteration < 100;
        Logger.ErrorFormat("{0} Ошибка при отправке отчета.", ex, logInfo);
        
        var message = ex.Message.Length > 250 ? ex.Message.Substring(250) : ex.Message;
        schedluleLog.Comment += ". " + message;
        schedluleLog.Status = ScheduledReports.ScheduleLog.Status.Error;
        schedluleLog.Save();
        
        Functions.Module.SendNotice(Roles.Administrators, "Ошибка при отправке отчета по расписанию", ex.StackTrace, setting);
        
        return;
      }
      finally
      {
        if (Locks.GetLockInfo(schedluleLog).IsLockedByMe)
          Locks.Unlock(schedluleLog);
      }
      
      Logger.DebugFormat("{0} Done.", logInfo);
    }

  }
}