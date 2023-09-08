using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Metadata;
using Sungero.Reporting;
using Sungero.Domain.SessionExtensions;
using Sungero.Reporting.Shared;
using Sungero.Metadata.Attributes;

namespace Starkov.ScheduledReports.Server
{
  public class ModuleFunctions
  {
    
    /// <summary>
    /// Получить запись "Относительная дата" по ИД.
    /// </summary>
    /// <param name="id">ИД</param>
    /// <returns>Относительная дата</returns>
    [Public, Remote]
    public static IRelativeDate GetRelativeDate(int id)
    {
      return RelativeDates.GetAll(r => r.Id == id).FirstOrDefault(r => r.Status != ScheduledReports.RelativeDate.Status.Closed);
    }
    
    #region Отправка отчетов по расписанию
    
    [Public]
    public void StartSheduleReport(Starkov.ScheduledReports.IScheduleSetting setting)
    {
      var report = GetModuleReportByGuid(Guid.Parse(setting.ModuleGuid), Guid.Parse(setting.ReportGuid));
      if (report == null)
        return;
      
      var performer = Sungero.Company.Employees.Null;
      FillReportParams(report, setting);
      
      var document = setting.Document;
      if (document == null)
      {
        document = Sungero.Docflow.SimpleDocuments.Create();
        document.Name = setting.Name;
        setting.Document = document;
      }
      
      report.ExportTo(document);
      document.Save();
      
      var obsrvers = setting.Observers.Select(o => o.Recipient).ToArray();
      if (Sungero.Company.Employees.Is(setting.Author) && setting.Author.Status != Sungero.Company.Employee.Status.Closed && !obsrvers.Contains(setting.Author))
        obsrvers.Append(setting.Author);
      
      if (obsrvers.Any())
      {
        var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(setting.Name, obsrvers);
        task.Attachments.Add(document);
        task.Start();
      }
      
      if (setting.State.IsChanged)
        setting.Save();
    }
    
    /// <summary>
    /// Заполнить параметры отчета из настроек.
    /// </summary>
    /// <param name="report">Отчет.</param>
    /// <param name="setting">Настройки отчета.</param>
    [Public]
    public void FillReportParams(Sungero.Reporting.Shared.ReportBase report, Starkov.ScheduledReports.IScheduleSetting setting)
    {
      foreach (var parameter in setting.ReportParams.Where(p => !string.IsNullOrEmpty(p.ViewValue)))
        report.SetParameterValue(parameter.Parameter, Functions.ScheduleSetting.GetObjectFromReportParam(parameter));
    }

    #endregion
    
    #region Работа с метаданными
    
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
      
      var moduleReports = Sungero.Metadata.Services.MetadataSearcher.FindModuleMetadata(moduleGuid).Items.OfType<Sungero.Metadata.ReportMetadata>();
      
      foreach (var report in moduleReports) //GetModuleReports(moduleGuid))
      {
        var newStructure = ScheduledReports.Structures.Module.ReportInfo.Create();
        newStructure.NameGuid = report.GetInfo().NameGuid; //Info.ReportTypeId;
        newStructure.Name = report.GetInfo().Name; //.Info.Name;
        newStructure.LocalizedName = GetReportLocalizedName(report.GetInfo().NameGuid); //report.Info.ReportTypeId);
        reports.Add(newStructure);
      }
      
      return reports;
    }
    
    [Public]
    public Sungero.Reporting.Shared.ReportBase GetModuleReportByGuid(Guid moduleGuid, Guid reportGuid)
    {
      Sungero.Reporting.Shared.ReportBase report = null;
      //      if (reportGuid == null)
      //        return report;
//
      //      using (var session = new Sungero.Domain.Session())
      //      {
      //        report = session.GetAll(Sungero.Reporting.Shared.ReportBase)
      //          .Where(r => (IReport)r != null && ((IReport)r).Info.ReportTypeId == reportGuid)
      //          .FirstOrDefault();
      //      }

      var reportClass = GetReportClassForModule(moduleGuid);
      if (reportClass == null)
        return report;
      
      var metodName = string.Format("Get{0}", GetReportMetaData(reportGuid)?.Name);
      
      var getMethod = reportClass.GetMethods().Where(m => m.Name == metodName).FirstOrDefault();
      if (getMethod != null)
        report = (Sungero.Reporting.Shared.ReportBase)getMethod.Invoke(null, null);
      
      return report;
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
      var moduleNamespace = Sungero.Metadata.Services.MetadataSearcher.FindModuleMetadata(moduleGuid)?.DefaultInterfaceNamespace;
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
    
    #endregion
    
    #region Получение сущностей по Guid
    
    /// <summary>
    /// Получить экземпляр сущности по Guid типа объекта и ИД.
    /// </summary>
    /// <param name="entityGuid">Guid сущности.</param>
    /// <param name="id">ИД объекта.</param>
    /// <returns>Экземпляр сущности.</returns>
    [Public, Remote]
    public Sungero.Domain.Shared.IEntity GetEntitiesByGuid(Guid entityGuid, int? id)
    {
      return GetEntitiesByGuid(entityGuid).FirstOrDefault(e => e.Id == id);
    }
    
    /// <summary>
    /// Получить список сущностей по Guid типа объекта и списку ИД.
    /// </summary>
    /// <param name="entityGuid">Guid сущности.</param>
    /// <param name="entitiesIds">Список ИД.</param>
    /// <returns>Список сущностей.</returns>
    [Public, Remote]
    public List<Sungero.Domain.Shared.IEntity> GetEntitiesByGuid(Guid entityGuid, List<int> entitiesIds)
    {
      return GetEntitiesByGuid(entityGuid).Where(e => entitiesIds.Contains(e.Id)).ToList();
    }
    
    /// <summary>
    /// Получить все сущности определенного типа по Guid.
    /// </summary>
    /// <param name="entityGuid">Guid сущности.</param>
    /// <returns>Список сущностей.</returns>
    [Public, Remote]
    public IQueryable<Sungero.Domain.Shared.IEntity> GetEntitiesByGuid(Guid entityGuid)
    {
      IQueryable<IEntity> entities = null;// new List<IEntity>();
      var entityType = Sungero.Domain.Shared.TypeExtension.GetTypeByGuid(entityGuid);
      if (entityType == null)
        return entities;
      
      using (var session = new Sungero.Domain.Session())
      {
        entities = session.GetAll(entityType);
      }
      
      return entities;
    }
    
    #endregion

  }
}