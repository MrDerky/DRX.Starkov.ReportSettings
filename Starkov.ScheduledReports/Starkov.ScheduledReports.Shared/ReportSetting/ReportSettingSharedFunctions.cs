using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ReportSetting;
using Sungero.Metadata;
using Sungero.Domain.Shared;

namespace Starkov.ScheduledReports.Shared
{
  partial class ReportSettingFunctions
  {
    /// <summary>
    /// Загрузить параметры отчета.
    /// </summary>
    [Public]
    public void SaveReportParams()
    {
      var reportGuid = Guid.Parse(_obj.ReportGuid);
      var report = Starkov.ScheduledReports.PublicFunctions.Module.GetReportMetaData(reportGuid);
      
      _obj.Parameters.Clear();
      foreach (var parameter in report.Parameters)
      {
        if (parameter.NameResourceKey == "ReportSessionId")
          continue;
        
        var reportParam = _obj.Parameters.AddNew();
        reportParam.ParameterName = parameter.NameResourceKey;
        reportParam.InternalDataTypeName = parameter.InternalDataTypeName;
        if (parameter.EntityMetadata != null)
        {
          reportParam.DisplayName = Sungero.Domain.Shared.TypeExtension.GetTypeByGuid(parameter.EntityType).GetEntityMetadata().GetDisplayName();
          reportParam.EntityGuid = parameter.EntityType.ToString();
        }
      }
    }
    
    /// <summary>
    /// Загрузить параметры отчета.
    /// </summary>
    [Public]
    public void SaveReportParams(Sungero.Reporting.Shared.ReportBase report)
    {
      foreach (var parameter in report.Parameters)
      {
        var reportParam = _obj.Parameters.FirstOrDefault(p => p.ParameterName == parameter.Key);
        if (reportParam != null)
        {
          var entityParameter = parameter.Value as Sungero.Reporting.Shared.EntityParameter;
          if (entityParameter != null)
          {
            reportParam.EntityId = entityParameter.Entity.Id;
            reportParam.ViewValue = entityParameter.Entity.DisplayValue;
          }
          else
            reportParam.ViewValue = parameter.Value.ToString().Contains(reportParam.InternalDataTypeName) ? string.Empty : parameter.Value.ToString();
        }
      }
    }
    
  }
}