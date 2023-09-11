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
    /// Создать запись настройки расписания для отчета.
    /// </summary>
    [Public, Remote]
    public static IScheduleSetting CreateScheduleSetting()
    {
      return ScheduleSettings.Create();
    }
    
    /// <summary>
    /// Получить запись настройки расписания для отчета по ИД.
    /// </summary>
    [Public, Remote]
    public static IScheduleSetting GetScheduleSetting(int id)
    {
      return ScheduleSettings.GetAll(s => s.Id == id).FirstOrDefault(s => s.Status != ScheduledReports.ScheduleSetting.Status.Closed);
    }
    
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
    
    #region Отправка уведомлений
    
    /// <summary>
    /// Отправить уведомление.
    /// </summary>
    /// <param name="role">Роль.</param>
    /// <param name="subject">Тема.</param>
    /// <param name="body">Текст.</param>
    /// <param name="attachment">Приложение.</param>
    public void SendNotice(IRole role, string subject, string body, IEntity attachment)
    {
      var performers = role.RecipientLinks.Select(r => r.Member).OfType<IRecipient>().ToArray();
      SendNotice(performers, subject, body, attachment);
    }
    
    /// <summary>
    /// Отправить уведомление.
    /// </summary>
    /// <param name="performers">Получатели.</param>
    /// <param name="subject">Тема.</param>
    /// <param name="body">Текст.</param>
    /// <param name="attachment">Приложение.</param>
    public void SendNotice(IRecipient[] performers, string subject, string body, IEntity attachment)
    {
      var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, performers.ToArray());
      if (!string.IsNullOrEmpty(body))
      {
        var text = task.Texts.AddNew();
        text.Body = "body";
      }
      
      if (attachment != null)
        task.Attachments.Add(attachment);
      
      task.Start();
    }
    
    #endregion
    
    #region Отправка отчетов по расписанию
    
    /// <summary>
    /// Отключить расписание.
    /// </summary>
    /// <param name="setting">Настройка расписания.</param>
    [Public]
    public void CloseScheduleLog(Starkov.ScheduledReports.IScheduleSetting setting)
    {
      AccessRights.AllowRead(() =>
                             {
                               var scheduleLogs = ScheduleLogs.GetAll(s => s.ScheduleSettingId == setting.Id)
                                 .Where(s => s.Status == ScheduledReports.ScheduleLog.Status.Waiting || s.Status == ScheduledReports.ScheduleLog.Status.Error)
                                 .OrderByDescending(s => s.StartDate);
                               
                               foreach (var scheduleLog in scheduleLogs)
                               {
                                 scheduleLog.Status = ScheduledReports.ScheduleLog.Status.Closed;
                                 scheduleLog.Comment = string.Format("Отменил: {0}", Users.Current.Name);
                                 scheduleLog.Save();
                               }
                             });
    }
    
    /// <summary>
    /// Получить записи журнала расписаний.
    /// </summary>
    /// <param name="setting">Настройка расписания.</param>
    /// <returns></returns>
    [Public]
    public IQueryable<IScheduleLog> GetScheduleLogs(Starkov.ScheduledReports.IScheduleSetting setting)
    {
      IQueryable<IScheduleLog> scheduleLogs = null;
      
      AccessRights.AllowRead(() =>
                             {
                               scheduleLogs = ScheduleLogs.GetAll(s => s.ScheduleSettingId == setting.Id);
                             });
      
      return scheduleLogs;
    }
    
    /// <summary>
    /// Создать запись расписания и асинхронный обработчик для выполнения.
    /// </summary>
    /// <param name="setting">Настройка расписания.</param>
    [Public]
    public void EnableSchedule(Starkov.ScheduledReports.IScheduleSetting setting)
    {
      Logger.Debug("StartSheduleReport. CreateScheduleLog");
      CreateScheduleLog(setting, null);
      ExecuteSheduleReportAsync(setting.Id);
    }
    
    /// <summary>
    /// Создать запись Журнала расписаний
    /// </summary>
    [Public]
    public IScheduleLog CreateScheduleLog(Starkov.ScheduledReports.IScheduleSetting setting, DateTime? startDate)
    {
      var scheduleLog = ScheduleLogs.Null;
      if (setting == null)
        return scheduleLog;
      
      if (setting.DateEnd.HasValue && setting.DateEnd.Value <= Calendar.Now)
        return scheduleLog;
      
      if (startDate == null)
        startDate = Functions.ScheduleSetting.GetNextPeriod(setting, startDate);
      
      if (startDate == null)
      {
        Logger.ErrorFormat("CreateScheduleLog. setting={0}. Не удалось вычислить дату следующего выполнения.", setting.Id);
        throw new Exception("Не удалось вычислить дату следующего выполнения.");
      }
      
      AccessRights.AllowRead(() =>
                             {
                               scheduleLog = ScheduleLogs.Create();
                               scheduleLog.ScheduleSettingId = setting.Id;
                               scheduleLog.StartDate = startDate;
                               scheduleLog.Status = ScheduledReports.ScheduleLog.Status.Waiting;
                               scheduleLog.Save();
                             });
      return scheduleLog;
    }
    
    /// <summary>
    /// Создать асинхронный обработчик для отправки отчета.
    /// </summary>
    /// <param name="scheduleSettingId"></param>
    [Public]
    public void ExecuteSheduleReportAsync (int scheduleSettingId)
    {
      var asyncHandler = Starkov.ScheduledReports.AsyncHandlers.SendSheduleReport.Create();
      asyncHandler.SheduleSettingId = scheduleSettingId;
      asyncHandler.ExecuteAsync();
    }
    
    /// <summary>
    /// Отправить отчет по расписанию.
    /// </summary>
    /// <param name="setting">Настройки расписания.</param>
    /// <param name="scheduleLog">Журнал расписания.</param>
    [Public]
    public void StartSheduleReport(Starkov.ScheduledReports.IScheduleSetting setting, IScheduleLog scheduleLog)
    {
      var observers = setting.Observers.Select(o => o.Recipient).Distinct().AsEnumerable();
      if (Sungero.Company.Employees.Is(setting.Author) && !observers.Contains(setting.Author))
        observers = observers.Append(setting.Author);
      
      observers = observers.Where(o => o.Status != Sungero.Company.Employee.Status.Closed)
        .OfType<IRecipient>();
      
      if (!observers.Any())
      {
        SendNotice(Roles.Administrators, string.Format("ScheduleSetting={0}. Не удалось вычислить получателей", setting.Id), null, setting);
        return;
      }
      
      Logger.Debug("StartSheduleReport. Get report");
      var report = GetModuleReportByGuid(Guid.Parse(setting.ModuleGuid), Guid.Parse(setting.ReportGuid));
      if (report == null)
        return;
      
      Logger.Debug("StartSheduleReport. FillReportParams");
      FillReportParams(report, setting);

      var document = Sungero.Docflow.SimpleDocuments.Create();
      document.Name = setting.Name;
      
      foreach (var recipient in observers)
        document.AccessRights.Grant(recipient, DefaultAccessRightsTypes.Read);
      
      Logger.Debug("StartSheduleReport. document save");
      document.Save();
      
      Logger.Debug("StartSheduleReport. repirt export to doc");
      report.ExportTo(document);
      
      Logger.Debug("StartSheduleReport. task start");
      SendNotice(observers.ToArray(), string.Format(Starkov.ScheduledReports.Resources.ReportSubject, setting.Name), null, document);
      
      scheduleLog.DocumentId = document.Id;
      scheduleLog.Status = ScheduledReports.ScheduleLog.Status.Complete;
      scheduleLog.Save();
      Logger.DebugFormat("StartSheduleReport. scheduleLog {0} save status = {1}", scheduleLog.Id, scheduleLog.Status);
      
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
      var reportParams = setting.ReportParams.Where(p => !string.IsNullOrEmpty(p.ViewValue));
      Logger.DebugFormat("FillReportParams. setting={0}, reportParam={1}", setting.Id, string.Join(", ", reportParams.Select(p => string.Format("{0}: ViewValue={1}, Id={2}", p.Parameter, p.ViewValue, p.Id))));
      foreach (var parameter in reportParams)
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
      
      foreach (var report in moduleReports)
      {
        var newStructure = ScheduledReports.Structures.Module.ReportInfo.Create();
        newStructure.NameGuid = report.GetInfo().NameGuid;
        newStructure.Name = report.GetInfo().Name;
        newStructure.LocalizedName = GetReportLocalizedName(report.GetInfo().NameGuid);
        reports.Add(newStructure);
      }
      
      return reports;
    }
    
    /// <summary>
    /// Получить отчет модуля по Guid.
    /// </summary>
    /// <param name="moduleGuid">Guid модуля.</param>
    /// <param name="reportGuid">Guid отчета.</param>
    /// <returns>Отчет.</returns>
    [Public]
    public Sungero.Reporting.Shared.ReportBase GetModuleReportByGuid(Guid moduleGuid, Guid reportGuid)
    {
      Sungero.Reporting.Shared.ReportBase report = null;

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
    
    /// <summary>
    /// Получить метаданные отчета по Guid.
    /// </summary>
    /// <param name="reportGuid">Guid Отчета.</param>
    /// <returns>метаданные отчета.</returns>
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
    /// Получить все сущности определенного типа по Guid.
    /// </summary>
    /// <param name="entityGuid">Guid сущности.</param>
    /// <returns>Список сущностей.</returns>
    [Public, Remote]
    public IQueryable<Sungero.Domain.Shared.IEntity> GetEntitiesByGuid(Guid entityGuid)
    {
      IQueryable<IEntity> entities = null;
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