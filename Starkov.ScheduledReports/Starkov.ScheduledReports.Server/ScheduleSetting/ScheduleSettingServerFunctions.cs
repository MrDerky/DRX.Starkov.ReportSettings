using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;
using Sungero.Domain.Shared;

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
      
      _obj.ReportParams.Clear();
      foreach (var parameter in report.Parameters)
      {
        //        if (parameter.NameResourceKey == "ReportSessionId")
        //          continue;
        
        var reportParam = _obj.ReportParams.AddNew();
        reportParam.Parameter = parameter.NameResourceKey;
        reportParam.InternalDataTypeName = parameter.InternalDataTypeName;
        if (parameter.EntityMetadata != null)
          reportParam.EntityType = parameter.EntityType.ToString();
      }
    }
    
    /// <summary>
    /// Заполнить параметры отчета.
    /// </summary>
    [Public]
    public void FillReportParams(Sungero.Reporting.Shared.ReportBase report)
    {
      foreach (var parameter in report.Parameters)
      {
        var reportParam = _obj.ReportParams.FirstOrDefault(p => p.Parameter == parameter.Key);
        if (reportParam != null)
        {
          var entityParameter = parameter.Value as Sungero.Reporting.Shared.EntityParameter;
          if (entityParameter != null)
          {
            reportParam.ValueId = entityParameter.Entity.Id; //EntityIdentifier.Id;
            reportParam.ValueDisplay = entityParameter.Entity.DisplayValue;
          }
          else
            reportParam.ValueDisplay = parameter.Value.ToString();
        }
      }
    }
    
    
    //    public Sungero.Domain.Shared.IEntity GetEntity(object
    public object GetObjectFromParameter(string type, string parameter)
    {
      //TODO сделать реализацию получения объекта по типу
      return null;
    }
  }
}