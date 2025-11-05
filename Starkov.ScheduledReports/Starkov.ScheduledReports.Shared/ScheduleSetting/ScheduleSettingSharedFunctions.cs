using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports.Shared
{
  partial class ScheduleSettingFunctions
  {
    
    /// <summary>
    /// Установить доступность свойств.
    /// </summary>
    public void SetPropertyStates()
    {
      var properties = _obj.State.Properties;
      var canChangeSchedule = _obj.Status != Status.Active;
      
      properties.Name.IsEnabled = canChangeSchedule;
      properties.DateBegin.IsEnabled = canChangeSchedule;
      properties.PeriodExpression.IsEnabled = canChangeSchedule;
      properties.IsAsyncExecute.IsEnabled = canChangeSchedule;
      properties.Observers.IsEnabled = canChangeSchedule;
    }

    /// <summary>
    /// Загрузить параметры отчета.
    /// </summary>
    public override void SaveReportParams()
    {
      base.SaveReportParams();
      _obj.Parameters.Clear();
      foreach (var parameter in _obj.ReportSetting.Parameters.Where(p => !string.IsNullOrEmpty(p.DisplayName)))
      {
        var reportParam = _obj.Parameters.AddNew();
        reportParam.ParameterName = parameter.ParameterName;
        reportParam.DisplayName = parameter.DisplayName;
        reportParam.ParameterInfo = parameter.ParameterInfo;
      }
    }
  }
}