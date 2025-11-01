using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sungero.Core;
using Starkov.ScheduledReports.Structures.Module;

namespace Starkov.ScheduledReports.Isolated.JsonParser
{
  public partial class IsolatedFunctions
  {
    [Public]
    public Structures.Module.IReportParameterInfo GetReportParameterInfoStruct(string parameterInfo)
    {
      if (string.IsNullOrEmpty(parameterInfo))
      {
        var info = Structures.Module.ReportParameterInfo.Create();
        info.ParameterName = string.Empty;
        info.DisplayName = string.Empty;
        info.DisplayValue = string.Empty;
        info.EntityGuid = string.Empty;
        info.InternalDataTypeName = string.Empty;
        
        info.IsEntity = false;
        info.IsRelatedDate = false;
        info.IsCollection = false;

        info.EntityIds = new List<long>();
        
        return info;
      }
      
      return Newtonsoft.Json.JsonConvert.DeserializeObject<Structures.Module.ReportParameterInfo>(parameterInfo);
    }
    
    [Public]
    public string GetReportParameterInfoText(Structures.Module.IReportParameterInfo parameterInfo)
    {
      return Newtonsoft.Json.JsonConvert.SerializeObject(parameterInfo);
    }
  }
}