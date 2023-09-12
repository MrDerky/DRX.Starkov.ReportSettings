using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ScheduledReports.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Отправка отчетов по расписанию.
    /// </summary>
    public virtual void SendSheduleReports()
    {
      var logInfo = string.Format("SendSheduleReports");
      Logger.DebugFormat("{0}. Start.", logInfo);
      
      var jobId = Constants.Module.SendSheduleReportsJobId;
      var lastJobExecuteTime = Functions.Module.GetLastJobExecuteTime(jobId);
      var nextJobExecuteTime = Functions.Module.GetNextJobExecuteTime(jobId);
      
      Logger.DebugFormat("{0}. lastJobExecuteTime={1}, nextJobExecuteTime={2}", logInfo, lastJobExecuteTime, nextJobExecuteTime);
      
      var scheduleLogs = Functions.Module.GetScheduleLogs()
        .Where(s => s.Status == ScheduledReports.ScheduleLog.Status.Waiting)
        .Where(s => s.IsAsyncExecute != true)
        .Where(s => s.StartDate.HasValue &&
               !lastJobExecuteTime.HasValue || lastJobExecuteTime < s.StartDate.Value &&
               !nextJobExecuteTime.HasValue || s.StartDate.Value <= nextJobExecuteTime);
      
      foreach (var schedule in scheduleLogs)
      {
        Logger.DebugFormat("{0}. scheduleLog={1}", logInfo, schedule.Id);
        var setting = PublicFunctions.Module.Remote.GetScheduleSetting(schedule.ScheduleSettingId);
        if (setting == null)
        {
          Logger.DebugFormat("{0}. Не удалось получить действующую запись справочника SheduleSetting.", logInfo);
          continue;
        }
        
        if (!Functions.Module.ScheduleLogExecute(setting, schedule, logInfo))
          Logger.DebugFormat("{0}. scheduleLog={1}. Ошибка при обработке.", logInfo, schedule.Id);
      }
      
      Logger.DebugFormat("{0}. Done.", logInfo);
    }

  }
}