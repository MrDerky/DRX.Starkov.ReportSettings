using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleLog;

namespace Starkov.ScheduledReports
{
  partial class ScheduleLogServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      Logger.DebugFormat("ScheduleLog BeforeSave. Id={0}, Users.Current.Id={1}, Users.Current.Login.Id={2}, Users.Current.Name={3}", _obj.Id, Users.Current.Id, Users.Current.Login.Id, Users.Current.Name);
      
      if (_obj.Status == Status.Closed)
        _obj.Comment = string.Format(Starkov.ScheduledReports.ScheduleLogs.Resources.CancelByUser, Users.Current.Name);
    }
  }


}