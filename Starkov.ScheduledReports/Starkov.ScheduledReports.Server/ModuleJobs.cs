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
      var logInfo = string.Format("SendSheduleReports Job");
      Logger.DebugFormat("{0}. Start.", logInfo);
      
      var jobId = Constants.Module.SendSheduleReportsJobId;
      var lastJobExecuteTime = Functions.Module.GetLastJobExecuteTime(jobId);
      var nextJobExecuteTime = Functions.Module.GetNextJobExecuteTime(jobId);
      
      Logger.DebugFormat("{0}. lastJobExecuteTime={1}, nextJobExecuteTime={2}", logInfo, lastJobExecuteTime, nextJobExecuteTime);
      
      // Если есть асинхронные варианты, но с ошибками - подхватить их фоновым процессом
      var scheduleLogs = Functions.Module.GetScheduleLogs()
        .Where(s => s.StartDate.HasValue)
        .Where(s => (s.IsAsyncExecute != true || s.StartDate < Calendar.Now) && s.Status == ScheduledReports.ScheduleLog.Status.Waiting || s.Status == ScheduledReports.ScheduleLog.Status.Error)
        .Where(s =>  !nextJobExecuteTime.HasValue || s.StartDate.Value <= nextJobExecuteTime)
        .OrderByDescending(s => s.Id);
      
      foreach (var scheduleBySetting in scheduleLogs.GroupBy(s => s.ScheduleSettingId))
      {
        var schedule = scheduleBySetting.First();
        Logger.DebugFormat("{0}. scheduleLog={1}", logInfo, schedule.Id);
        var setting = PublicFunctions.ScheduleSetting.Remote.GetScheduleSetting(schedule.ScheduleSettingId);
        if (setting == null)
        {
          Logger.DebugFormat("{0}. Не удалось получить действующую запись справочника SheduleSetting.", logInfo);
          if (Locks.TryLock(schedule))
          {
            schedule.Status = ScheduledReports.ScheduleLog.Status.Closed;
            schedule.Save();
            
            if (Locks.GetLockInfo(schedule).IsLockedByMe)
              Locks.Unlock(schedule);
          }
          continue;
        }
        
        if (!Functions.Module.ScheduleLogInterationExecute(schedule.Id, logInfo))
        {
          Logger.DebugFormat("{0}. scheduleLog={1}. Ошибка при обработке.", logInfo, schedule.Id);
          
          // HACK Обход платформенного бага при генерации отчетов
          if (!string.IsNullOrEmpty(schedule.Comment) && schedule.Comment.Contains("Object reference not set to an instance of an object."))
          {
            Logger.DebugFormat("{0}. scheduleLog={1}. Передача обработки в асинхронный обработчик.", logInfo, schedule.Id);
            Functions.Module.ExecuteSheduleReportAsync(setting.Id);
          }
          
          return;
        }
      }
      
      Logger.DebugFormat("{0}. Done.", logInfo);
    }

  }
}