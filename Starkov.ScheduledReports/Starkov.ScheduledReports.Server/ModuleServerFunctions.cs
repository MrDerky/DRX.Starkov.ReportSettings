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
    /// Получить имена модулей системы, в которых есть отчеты.
    /// </summary>
    /// <returns>Словарь (guid, имя)</returns>
    [Public]
    public System.Collections.Generic.Dictionary<Guid, string> GetReportsModuleNames()
    {
      var moduleNames = new Dictionary<Guid, string>();
      
      foreach (var module in Sungero.Metadata.Services.MetadataService.Instance.ModuleList.Modules)
      {
        if (GetModuleReports(module.NameGuid) != null)
          moduleNames.Add(module.NameGuid, module.GetDisplayName());
      }

      return moduleNames;
    }
    
    /// <summary>
    /// Получить список отчетов модуля в виде структуры.
    /// </summary>
    /// <param name="moduleGuid">Идентификатор модуля.</param>
    /// <returns>Список отчетов модуля.</returns>
    [Public]
    public System.Collections.Generic.List<ScheduledReports.Structures.Module.IReportInfo> GetModuleReportsStructure(Guid moduleGuid)
    {
      var reports = new List<ScheduledReports.Structures.Module.IReportInfo>();
      
      foreach (var report in GetModuleReports(moduleGuid))
      {
        var newStructure = ScheduledReports.Structures.Module.ReportInfo.Create();
        newStructure.NameGuid = report.Info.ReportTypeId;
        newStructure.Name = report.Info.Name;
        newStructure.LocalizedName = GetReportLocalizedName(report.Info.ReportTypeId);
        reports.Add(newStructure);
      }
      
      return reports;
    }
    
    /// <summary>
    /// Получить список отчетов модуля.
    /// </summary>
    /// <param name="moduleGuid">Идентификатор модуля.</param>
    /// <returns>Список отчетов модуля.</returns>
    private System.Collections.Generic.IEnumerable<Sungero.Reporting.IReport> GetModuleReports(Guid moduleGuid)
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
    
    private System.Type GetReportClassForModule(Guid moduleGuid)
    {
      var moduleNamespace = Sungero.Metadata.Services.MetadataSearcher.FindModuleMetadata(moduleGuid).DefaultInterfaceNamespace;
      var className = string.Format("{0}.{1}, Sungero.Domain.Interfaces, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", moduleNamespace, "Reports");
      return Type.GetType(className);
    }
    
    /// <summary>
    /// Получить локализованное название отчета.
    /// </summary>
    /// <param name="reportGuid">Идентификатор отчета.</param>
    /// <returns>Строка с именем.</returns>
    private string GetReportLocalizedName(Guid reportGuid)
    {
      var reportLocalizedName = string.Empty;
      var reportMetaData = Sungero.Metadata.Services.MetadataSearcher.FindModuleItemMetadata(reportGuid);
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
    
    //    [Public]
    //    public ScheduledReports.Structures.Module.IReportInfo GetReportInfo(Guid reportGuid)
    //    {
    //      var reportInfo = ScheduledReports.Structures.Module.ReportInfo.Create();
//
    //      var reportMetaData = GetReportMetaData(reportGuid);
    //      if (reportMetaData == null)
    //        return reportInfo;
//
    //      reportInfo.NameGuid = reportMetaData.NameGuid;
    //      reportInfo.Name = reportMetaData.Name;
    //      reportInfo.LocalizedName = GetReportLocalizedName(reportMetaData.NameGuid);
//
    //      foreach (var parameter in reportMetaData.Parameters)
    //      {
//
    //        reportInfo.Parameters.Add(parameter.NameResourceKey);
    //      }
//
    //      return reportInfo;
    //    }
    
  }
}