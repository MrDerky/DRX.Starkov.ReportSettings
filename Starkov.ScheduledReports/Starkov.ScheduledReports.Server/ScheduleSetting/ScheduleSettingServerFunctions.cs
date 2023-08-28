using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports.Server
{
  partial class ScheduleSettingFunctions
  {

    /// <summary>
    /// Заполнить параметры отчета.
    /// </summary>
    [Public]
    public void FillReportParams()
    {
      var reportGuid = Guid.Parse(_obj.ReportGuid);
      var report = Starkov.ScheduledReports.PublicFunctions.Module.GetReportMetaData(reportGuid);
      
      foreach (var parameter in report.Parameters)
      {
        var reportParam = _obj.ReportParams.AddNew();
        reportParam.Parameter = parameter.NameResourceKey;
        reportParam.Type = parameter.InternalDataTypeName;
      }
    }

  }
}