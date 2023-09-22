﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.SettingSchedule;

namespace Starkov.ScheduledReports
{
  partial class SettingScheduleClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      if (_obj.ReportSetting == null)
        e.Instruction = string.Format(Starkov.ScheduledReports.SettingSchedules.Resources.SheduleSettingInstruction,
                                      Environment.NewLine,
                                      _obj.Info.Actions.SetReport.LocalizedName,
                                      _obj.Info.Actions.StartReport.LocalizedName);
      
      Functions.SettingSchedule.SetPropertyStates(_obj);
      
      var scheduleSettingManagerRole = Roles.GetAll(r => r.Sid == Constants.Module.ScheduleSettingManagerRole).FirstOrDefault();
      if (scheduleSettingManagerRole == null || !Users.Current.IncludedIn(scheduleSettingManagerRole))
      {
        e.HideAction(_obj.Info.Actions.EnableSchedule);
        e.HideAction(_obj.Info.Actions.DisableSchedule);
      }
      
      if (_obj.Status == Status.Active && !Functions.Module.Remote.GetScheduleLogs(_obj)
          .Any(l => l.Status == ScheduleLog.Status.Waiting || l.Status == ScheduleLog.Status.Error))
      {
        _obj.Status = Status.Closed;
      }
    }

  }
}