using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleLog;

namespace Starkov.ScheduledReports.Server
{
  partial class ScheduleLogFunctions
  {

    /// <summary>
    /// Получить StateView для просмотра журналов расписаний.
    /// </summary>       
    [Remote]
    public StateView GetScheduleLogState()
    {
      return Functions.Module.GetScheduleState(null);
    }

  }
}