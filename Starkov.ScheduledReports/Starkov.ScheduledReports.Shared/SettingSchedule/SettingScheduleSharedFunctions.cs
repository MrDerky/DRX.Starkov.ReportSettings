using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.SettingSchedule;

namespace Starkov.ScheduledReports.Shared
{
  partial class SettingScheduleFunctions
  {
    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public override void FillName()
    {
      _obj.Name = _obj.ReportName;
    }
    
    /// <summary>
    /// Установить доступность свойств.
    /// </summary>
    public void SetPropertyStates()
    {
      var properties = _obj.State.Properties;
      var canChangeSchedule = _obj.Status != Status.Active;
      
      properties.Name.IsEnabled = canChangeSchedule;
      properties.DateBegin.IsEnabled = canChangeSchedule;
      properties.Period.IsEnabled = canChangeSchedule;
      properties.PeriodNumber.IsEnabled = canChangeSchedule;
      properties.IsAsyncExecute.IsEnabled = canChangeSchedule;
      properties.Observers.IsEnabled = canChangeSchedule;
    }
  }
}