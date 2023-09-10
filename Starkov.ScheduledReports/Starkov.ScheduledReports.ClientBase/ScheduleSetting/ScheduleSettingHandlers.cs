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

    public override void StatusValueInput(Sungero.Presentation.EnumerationValueInputEventArgs e)
    {
      Functions.ScheduleSetting.SetPropertyStates(_obj);
    }

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      //Functions.ScheduleSetting.SetPropertyStates(_obj);
      
//      var nextDate = Functions.ScheduleSetting.Remote.GetNextPeriod(_obj);
//      if (nextDate.HasValue)
//        e.AddInformation("Время следующего запуска " + nextDate.Value);
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
    }
    
  }
}