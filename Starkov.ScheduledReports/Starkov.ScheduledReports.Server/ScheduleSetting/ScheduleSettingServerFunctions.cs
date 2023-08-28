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
    /// 
    /// </summary>       
    [Public]
    public void FillReportParams()
    {
      var reportGuid = _obj.ReportGuid;
      var report = Starkov.ScheduledReports.PublicFunctions.Module.GetReportMetaData(reportGuid);
      foreach (var parameter in report.Parameters)
      {
        _obj.ReportParams.AddNew().Parameter = parameter.NameResourceKey;
      }
    }

  }
}