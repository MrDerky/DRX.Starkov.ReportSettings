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

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Properties.PeriodNumber.IsVisible = _obj.Period != null && _obj.Period.IsIncremental.GetValueOrDefault();
      
      if (string.IsNullOrEmpty(_obj.ReportName))
        e.Instruction = string.Format(Starkov.ScheduledReports.ScheduleSettings.Resources.SheduleSettingInstruction,
                                      Environment.NewLine,
                                      _obj.Info.Actions.SetReport.LocalizedName,
                                      _obj.Info.Actions.StartReport.LocalizedName);
      
    }
    
  }
}