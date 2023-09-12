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

    public virtual void PeriodNumberChanged(Sungero.Domain.Shared.IntegerPropertyChangedEventArgs e)
    {
      if (e.OldValue == e.NewValue )
        return;
      
      var number = e.NewValue;
      
      if (e.NewValue < 1) //TODO вынести в настройку
        number =_obj.PeriodNumber = 1;
      else if (e.NewValue > 100)
        number = _obj.PeriodNumber = 100;
    }

    public virtual void PeriodChanged(Starkov.ScheduledReports.Shared.ScheduleSettingPeriodChangedEventArgs e)
    {
      if (e.OldValue == e.NewValue)
        return;
      
      var isIncremental = e.NewValue != null && e.NewValue.IsIncremental.GetValueOrDefault();
      _obj.State.Properties.PeriodNumber.IsVisible = isIncremental;
    }

    public virtual void ReportNameChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      if (string.IsNullOrEmpty(_obj.Name) || e.OldValue == _obj.Name)
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