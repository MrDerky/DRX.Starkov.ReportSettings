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

    public virtual void ReportGuidChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      if (Equals(e.OldValue, e.NewValue))
        return;
      
      PublicFunctions.ScheduleSetting.FillReportParams(_obj);
    }

  }
}