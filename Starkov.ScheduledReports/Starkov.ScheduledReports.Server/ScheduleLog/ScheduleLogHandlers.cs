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
      if (_obj.Status == Status.Closed)
        _obj.Comment = string.Format(Starkov.ScheduledReports.ScheduleLogs.Resources.CancelByUser, Users.Current.Name);
    }
  }


}