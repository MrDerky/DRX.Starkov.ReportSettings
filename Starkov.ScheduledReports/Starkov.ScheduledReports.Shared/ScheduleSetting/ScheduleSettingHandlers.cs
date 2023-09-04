using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports
{

  partial class ScheduleSettingSharedHandlers
  {

    public virtual void PeriodChanged(Starkov.ScheduledReports.Shared.ScheduleSettingPeriodChangedEventArgs e)
    {
      if (e.OldValue != e.NewValue)
        _obj.NextDate = Functions.ScheduleSetting.Remote.GetNextPeriod(_obj);
    }

    public virtual void ReportNameChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      Functions.ScheduleSetting.FillName(_obj);
    }

    public virtual void ReportGuidChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      if (Equals(e.OldValue, e.NewValue))
        return;
      
      PublicFunctions.ScheduleSetting.SaveReportParams(_obj);
    }

  }
}