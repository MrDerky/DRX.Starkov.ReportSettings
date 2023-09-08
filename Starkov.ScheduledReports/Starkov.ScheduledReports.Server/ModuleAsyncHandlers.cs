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
      
      if (!Locks.TryLock(setting))
      {
        Logger.DebugFormat("{0}. Запись справочника SheduleSetting заблокирована пользователем {1}.", logInfo, Locks.GetLockInfo(setting).OwnerName);
        args.Retry = true;
        return;
      }
      
      try
      {
        PublicFunctions.Module.StartSheduleReport(setting);
      }
      catch (Exception ex)
      {
        args.Retry = args.RetryIteration < 100;
        Logger.ErrorFormat("{0} Ошибка при отправке отчета.", ex, logInfo);
        return;
      }
      finally
      {
        if (Locks.GetLockInfo(setting).IsLockedByMe)
          Locks.Unlock(setting);
      }
      
      Logger.DebugFormat("{0} Done.", logInfo);
    }

  }
}