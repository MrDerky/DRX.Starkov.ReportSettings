using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Metadata;
using Sungero.Reporting;

namespace Starkov.ScheduledReports.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// 
    /// </summary>
    public void TestFunction()
    {
      var moduleGuids = GetAllModulesGuids();
      
      var moduleNamespaces = new List<string>();
      var reportList = new List<string>();
      foreach (var guid in moduleGuids)
      {
        var reports = GetModuleReports(guid);
        foreach (var report in reports)
        {
          var reportName = GetReportLocalizedName(report);
          if (!string.IsNullOrEmpty(reportName))
            reportList.Add(reportName);
        }
      }
    }
    
    public List<Guid> GetAllModulesGuids()
    {
      return Sungero.Metadata.Services.MetadataService.Instance.ModuleList.Modules
        .Select(m => m.NameGuid) // TODO уточнить какой гуид нужен
        .ToList();
    }
    
    public System.Collections.Generic.IEnumerable<Sungero.Reporting.IReport> GetModuleReports(Guid moduleGuid)
    {
      IEnumerable<IReport> reports = null;
      var reportClass = GetReportClassForModule(moduleGuid);
      if (reportClass == null)
        return reports;
      
      var methodGetAll = reportClass.GetMethods().Where(m => m.Name == "GetAll" && m.GetParameters().Length == 0).FirstOrDefault();
      if (methodGetAll != null)
        reports = (IEnumerable<IReport>)methodGetAll.Invoke(null, null);
      
      return reports;
    }
    
    public System.Type GetReportClassForModule(Guid moduleGuid)
    {
      var moduleNamespace = Sungero.Metadata.Services.MetadataSearcher.FindModuleMetadata(moduleGuid).DefaultInterfaceNamespace;
      var className = string.Format("{0}.{1}, Sungero.Domain.Interfaces, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", moduleNamespace, "Reports");
      return Type.GetType(className);
    }
    
    public string GetReportLocalizedName(Sungero.Reporting.IReport report)
    {
      var reportLocalizedName = string.Empty;
      if (report == null)
        return reportLocalizedName;
      
      var reportMetaData = Sungero.Metadata.Services.MetadataSearcher.FindModuleItemMetadata(report.Info.ReportTypeId);
      if (reportMetaData != null)
        reportLocalizedName = ((Sungero.Metadata.ReportMetadata)reportMetaData).LocalizedName;
      
      return reportLocalizedName;
    }
    
    [Public]
    public Sungero.Metadata.ReportMetadata GetReportMetaData(Guid reportGuid)
    {
      Sungero.Metadata.ReportMetadata reportMetaData = null;
      
      var itemMetaData = Sungero.Metadata.Services.MetadataSearcher.FindModuleItemMetadata(reportGuid);
      if (itemMetaData != null)
        reportMetaData = ((Sungero.Metadata.ReportMetadata)itemMetaData);
          
      return reportMetaData;
    }
    
    [Public]
    public Sungero.Reporting.Shared.ReportBase GetReport(Guid reportGuid)
    {
      Sungero.Reporting.Shared.ReportBase reportBase = null;
      var reportMetaData = Sungero.Metadata.Services.MetadataSearcher.FindModuleItemMetadata(reportGuid);
      if (reportMetaData != null)
        reportBase = ((Sungero.Metadata.ReportMetadata)reportMetaData)
      
      return null;
    }

  }
}