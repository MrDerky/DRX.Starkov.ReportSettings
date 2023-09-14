using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports
{
  partial class ScheduleSettingClientHandlers
  {

    public virtual void ShowParamsValueInput(Sungero.Presentation.BooleanValueInputEventArgs e)
    {
      _obj.State.Properties.ReportParams.IsVisible = e.NewValue.GetValueOrDefault();
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Properties.PeriodNumber.IsVisible = _obj.Period != null && _obj.Period.IsIncremental.GetValueOrDefault();
      
      if (string.IsNullOrEmpty(_obj.ReportName))
        e.Instruction = string.Format(Starkov.ScheduledReports.ScheduleSettings.Resources.SheduleSettingInstruction,
                                      Environment.NewLine,
                                      _obj.Info.Actions.SetReport.LocalizedName,
                                      _obj.Info.Actions.StartReport.LocalizedName);
      
      Functions.ScheduleSetting.SetPropertyStates(_obj);
      _obj.State.Properties.ReportParams.IsVisible = _obj.ShowParams.GetValueOrDefault();
      
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