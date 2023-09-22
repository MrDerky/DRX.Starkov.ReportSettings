using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.SettingSchedule;

namespace Starkov.ScheduledReports
{
  partial class SettingScheduleSharedHandlers
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

    public virtual void ReportSettingChanged(Starkov.ScheduledReports.Shared.SettingScheduleReportSettingChangedEventArgs e)
    {
      if (Equals(e.OldValue, e.NewValue))
        return;
      
      if (e.NewValue != null)
      {
        _obj.ModuleGuid = e.NewValue.ModuleGuid;
        _obj.ModuleName = e.NewValue.ModuleName;
        _obj.ReportGuid = e.NewValue.ReportGuid;
        _obj.ReportName = e.NewValue.ReportName;
      }
      
      if (string.IsNullOrEmpty(_obj.Name) || e.OldValue != null && e.OldValue.ReportName == _obj.Name)
        Functions.SettingSchedule.FillName(_obj);
      
      PublicFunctions.SettingBase.SaveReportParams(_obj);
    }

  }
}