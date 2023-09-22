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
    
    #region StateView расписаний
    
    /// <summary>
    /// Получить состояние из журнала расписаний.
    /// </summary>
    public StateView GetScheduleState(ISettingSchedule setting)
    {
      var stateView = StateView.Create();
      stateView.IsPrintable = true;
      
      var scheduleLogs = Functions.Module.GetScheduleLogs(setting)
        .OrderByDescending(s => s.StartDate);
      
      if (!scheduleLogs.Any())
        return stateView;
      
      var nextJobExecuteTime = Functions.Module.GetNextJobExecuteTime(Constants.Module.SendSheduleReportsJobId);
      
      var block = stateView.AddBlock();
      // TODO Подумать как лучше реализовать возможность просмотра всех записей (Листание или отчет)
      if (Users.Current.IncludedIn(Roles.Administrators))
        block.AddHyperlink("Показать все записи", Hyperlinks.Get(ScheduleLogs.Info));
      else
        block.AddLabel("Информация о созданных расписаниях.");
      
      var childBlockRowsLimit = setting != null ? 100 : 10;
      foreach (var scheduleBySetting in scheduleLogs.GroupBy(s => s.ScheduleSettingId))
      {
        foreach (var log in scheduleBySetting.Take(childBlockRowsLimit))
        {
          if (setting != null || log.Id == scheduleBySetting.FirstOrDefault().Id)
          {
            block = stateView.AddBlock();
            FillBlock(block, setting, log, nextJobExecuteTime, false);
          }
          else
            FillBlock(block.AddChildBlock(), setting, log, nextJobExecuteTime, true);
        }
      }
      
      return stateView;
    }
    
    /// <summary>
    /// Добавить блок.
    /// </summary>
    /// <param name="block">Блок.</param>
    /// <param name="setting">Запись настроек расписания.</param>
    /// <param name="log">Запись журнала расписания.</param>
    /// <param name="nextJobExecuteTime">Следующий запуск фонового процесса.</param>
    private void FillBlock(Sungero.Core.StateBlock block, ISettingSchedule setting, IScheduleLog log, DateTime? nextJobExecuteTime, bool IsChildBlock)
    {
      #region Стили
      var iconSize = StateBlockIconSize.Large;
      var errorBlockStyle = StateBlockLabelStyle.Create();
      errorBlockStyle.Color = Colors.Common.Red;
      errorBlockStyle.FontWeight = FontWeight.Bold;
      
      var statusStyle = StateBlockLabelStyle.Create();
      statusStyle.FontWeight = FontWeight.Bold;
      
      if (log.Status == ScheduledReports.ScheduleLog.Status.Complete)
      {
        block.AssignIcon(ScheduleLogs.Resources.Complete, iconSize);
      }
      else if (log.Status == ScheduledReports.ScheduleLog.Status.Waiting)
      {
        statusStyle.Color = Colors.Common.Green;
        block.AssignIcon(ScheduleLogs.Resources.Waiting, iconSize);
      }
      else if (log.Status == ScheduledReports.ScheduleLog.Status.Error)
      {
        statusStyle = errorBlockStyle;
        block.AssignIcon(ScheduleLogs.Resources.Error, iconSize);
      }
      else if(log.Status == ScheduledReports.ScheduleLog.Status.Closed)
      {
        statusStyle.Color = Colors.Common.LightGray;
      }
      #endregion
      
      //TODO локализация
      //TODO рефакторинг разбить на подфункции со своими блоками в зависимости от статуса
      
      // Статус
      block.AddLabel(log.Info.Properties.Status.GetLocalizedValue(log.Status.Value), statusStyle);
      
      // Ссылка на настройку расписания
      if (!IsChildBlock && setting == null && log.ScheduleSettingId.HasValue)
        block.AddHyperlink(log.Name, Hyperlinks.Get(ScheduledReports.ScheduleSettings.Info, log.ScheduleSettingId.Value));
      
      // Плановый запуск
      var content = block.AddContent();
      content.AddLabel("Плановый запуск: " + GetStringDate(log.StartDate));
      
      // Последний запуск
      if (log.LastStart.HasValue && log.Status != ScheduledReports.ScheduleLog.Status.Closed)
      {
        content.AddLineBreak();
        content.AddLabel("Запуск " + GetStringDate(log.LastStart));
      }
      
      block.AddLineBreak();
      if (log.DocumentId.HasValue)
        // Ссылка на документ с отчетом
        block.AddHyperlink("Просмотр", Hyperlinks.Get(Sungero.Docflow.OfficialDocuments.Info, log.DocumentId.Value));
      else if (log.IsAsyncExecute != true)
      {
        // Информация о фоновом обработчике
        if (nextJobExecuteTime.HasValue)
          block.AddLabel(string.Format("Старт фонового процесса: {0}", nextJobExecuteTime.ToUserTime()));
        else
          block.AddLabel("Не удалось получить время старта фонового процесса", errorBlockStyle);
      }
      
      // Комментарий
      if (log.Status == ScheduledReports.ScheduleLog.Status.Error && !string.IsNullOrEmpty(log.Comment))
      {
        // Сообщение об ошибке
        block.AddLineBreak();
        block.AddLabel("Ошибка: " + log.Comment);
      }
      else if(log.Status == ScheduledReports.ScheduleLog.Status.Closed && !string.IsNullOrEmpty(log.Comment))
      {
        //          block.AddLineBreak();
        content.AddLineBreak();
        //          content = block.AddContent();
        content.AddLabel(log.Comment);
      }
    }
    
    private string GetStringDate(DateTime? date)
    {
      if (!date.HasValue)
        return string.Empty;
      
      if (Calendar.Today == date.Value.Date)
        return date.Value.ToUserTime().ToShortTimeString();
      
      return date.Value.ToUserTime().ToString("g");
    }
    
    #endregion
    
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
      var task = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, performers.Where(p => p.IsSystem != true).ToArray());
      if (!string.IsNullOrEmpty(body))
      {
        var text = task.Texts.AddNew();
        text.Body = body;
      }
      
      if (attachment != null)
        task.Attachments.Add(attachment);
      
      task.Start();
    }
    
    #endregion
    
    #region Отправка отчетов по расписанию
    
    /// <summary>
    /// Создать запись расписания и асинхронный обработчик для выполнения.
    /// </summary>
    /// <param name="setting">Настройка расписания.</param>
    [Public]
    public void EnableSchedule(Starkov.ScheduledReports.ISettingSchedule setting)
    {
      CreateScheduleLog(setting, null);
      if (setting.IsAsyncExecute == true)
        ExecuteSheduleReportAsync(setting.Id);
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
    /// Обработать запись журнала расписания и отправить отчет.
    /// </summary>
    /// <param name="setting">Настройка расписания.</param>
    /// <param name="scheduleLog">Запись журнала расписания.</param>
    /// <param name="logInfo">Информация для логирования.</param>
    /// <returns>Признак успешного выполнения.</returns>
    public bool ScheduleLogExecute(ISettingSchedule setting, IScheduleLog scheduleLog, string logInfo)
    {
      try
      {
        if (!Locks.GetLockInfo(scheduleLog).IsLockedByMe && !Locks.TryLock(scheduleLog))
        {
          Logger.DebugFormat("{0} Запись справочника scheduleLog={1} заблокирована пользователем {2}.", logInfo, scheduleLog.Id, Locks.GetLockInfo(scheduleLog).OwnerName);
          return false;
        }
        
        scheduleLog.LastStart = Calendar.Now;
        
        StartSheduleReport(setting, scheduleLog);
        EnableSchedule(setting);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("{0} Ошибка при отправке отчета.", ex, logInfo);
        
        SendNotice(Roles.Administrators, "Ошибка при отправке отчета по расписанию",
                   string.Join(Environment.NewLine, ex.Message, ex.StackTrace),
                   setting);
        
        scheduleLog.Comment = ex.Message.Length > 250 ? ex.Message.Substring(250) : ex.Message;
        scheduleLog.Status = ScheduledReports.ScheduleLog.Status.Error;
        scheduleLog.Save();
        
        return false;
      }
      finally
      {
        Logger.ErrorFormat("{0} Разблокировка scheduleLog={1}, setting={2}.", logInfo, scheduleLog.Id, setting.Id);
        if (Locks.GetLockInfo(scheduleLog).IsLockedByMe)
          Locks.Unlock(scheduleLog);
        
        if (Locks.GetLockInfo(setting).IsLockedByMe)
          Locks.Unlock(setting);
      }
      
      return true;
    }
    
    /// <summary>
    /// Отправить отчет по расписанию.
    /// </summary>
    /// <param name="setting">Настройки расписания.</param>
    /// <param name="scheduleLog">Журнал расписания.</param>
    private void StartSheduleReport(Starkov.ScheduledReports.ISettingSchedule setting, IScheduleLog scheduleLog)
    {
      if (setting == null || setting.ReportSetting == null)
        return;
      
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
      
      var report = GetModuleReportByGuid(Guid.Parse(setting.ReportSetting.ModuleGuid), Guid.Parse(setting.ReportSetting.ReportGuid));
      if (report == null)
        return;
      
      Logger.Debug("StartSheduleReport. FillReportParams");
      FillReportParams(report, setting);

      var document = Sungero.Docflow.SimpleDocuments.Create();
      if (!Locks.GetLockInfo(document).IsLocked)
        Locks.Lock(document);
      
      document.Name = setting.Name;
      
      Logger.Debug("StartSheduleReport. document.AccessRights.Grant");
      foreach (var recipient in observers)
        document.AccessRights.Grant(recipient, DefaultAccessRightsTypes.Read);
      
      using(var stream = new System.IO.MemoryStream())
      {
        using (var reportStream = report.Export())
          reportStream.CopyTo(stream);
        
        Logger.Debug("StartSheduleReport. report export to doc");
        document.CreateVersionFrom(stream, report.ExportFormat.GetFileExtension());
        
        Logger.Debug("StartSheduleReport. document save");
        document.Save();
        
        if (!Locks.GetLockInfo(document).IsLockedByMe)
          Locks.Unlock(document);
      }
      
      Logger.Debug("StartSheduleReport. task start");
      SendNotice(observers.ToArray(), setting.Name, null, document);
      
      scheduleLog.DocumentId = document.Id;
      scheduleLog.Status = ScheduledReports.ScheduleLog.Status.Complete;
      scheduleLog.Save();
      
      Logger.DebugFormat("StartSheduleReport. scheduleLog {0} save status = {1}", scheduleLog.Id, scheduleLog.Status);
    }
    
    /// <summary>
    /// Заполнить параметры отчета из настроек.
    /// </summary>
    /// <param name="report">Отчет.</param>
    /// <param name="setting">Настройки отчета.</param>
    [Public]
    public void FillReportParams(Sungero.Reporting.Shared.ReportBase report, Starkov.ScheduledReports.ISettingSchedule setting)
    {
      var reportParams = setting.ReportParams.Where(p => !string.IsNullOrEmpty(p.ViewValue));
      Logger.DebugFormat("FillReportParams. setting={0}, reportParam={1}", setting.Id, string.Join(", ", reportParams.Select(p => string.Format("{0}: ViewValue={1}, Id={2}", p.ParameterName, p.ViewValue, p.Id))));
      foreach (var parameter in reportParams)
        report.SetParameterValue(parameter.ParameterName, Functions.ScheduleSetting.GetObjectFromReportParam(parameter));
    }

    #endregion
    
    #region Операции с Журналом расписаний

    /// <summary>
    /// Получить записи журнала расписаний.
    /// </summary>
    /// <returns>Список записей журнала.</returns>
    [Public, Remote(IsPure = true)]
    public IQueryable<IScheduleLog> GetScheduleLogs()
    {
      return GetScheduleLogs(null);
    }
    
    /// <summary>
    /// Получить записи журнала расписаний.
    /// </summary>
    /// <param name="setting">Настройка расписания.</param>
    /// <returns>Список записей журнала.</returns>
    [Public, Remote(IsPure = true)]
    public IQueryable<IScheduleLog> GetScheduleLogs(Starkov.ScheduledReports.ISettingSchedule setting)
    {
      var scheduleLogs = ScheduleLogs.GetAll()
        .Where(s => s.Status != ScheduledReports.ScheduleLog.Status.Preview);

      if (setting != null)
        scheduleLogs = scheduleLogs.Where(s => s.ScheduleSettingId == setting.Id);
      
      return scheduleLogs;
    }
    
    /// <summary>
    /// Получить запись журнала расписаний для предпросмотра.
    /// </summary>
    /// <returns>Запись журнала со статусом Preview.</returns>
    [Public, Remote(IsPure = true)]
    public IScheduleLog GetPreviewScheduleLog()
    {
      var scheduleLog = ScheduleLogs.GetAll()
        .FirstOrDefault(s => s.Status == ScheduledReports.ScheduleLog.Status.Preview);
      
      return scheduleLog;
    }
    
    /// <summary>
    /// Создать запись Журнала расписаний
    /// </summary>
    private void CreateScheduleLog(Starkov.ScheduledReports.ISettingSchedule setting, DateTime? startDate)
    {
      if (setting == null)
        return;
      
      if (startDate == null)
        startDate = Functions.ScheduleSetting.GetNextPeriod(setting, startDate);
      
      if (startDate == null)
      {
        Logger.ErrorFormat("CreateScheduleLog. setting={0}. Не удалось вычислить дату следующего выполнения.", setting.Id);
        throw new Exception("Не удалось вычислить дату следующего выполнения.");
      }
      
      if (setting.DateEnd.HasValue && setting.DateEnd.Value < startDate.Value)
        return;
      
      var scheduleLog = ScheduleLogs.Create();
      scheduleLog.ScheduleSettingId = setting.Id;
      scheduleLog.Name = setting.Name;
      scheduleLog.StartDate = startDate;
      scheduleLog.Status = ScheduledReports.ScheduleLog.Status.Waiting;
      scheduleLog.IsAsyncExecute = setting.IsAsyncExecute;
      
      if (Users.Current.Id != setting.Author.Id)
        scheduleLog.AccessRights.Grant(setting.Author, DefaultAccessRightsTypes.FullAccess);
      
      foreach (var observer in setting.Observers.Select(o => o.Recipient))
        scheduleLog.AccessRights.Grant(observer, DefaultAccessRightsTypes.Read);
      
      scheduleLog.Save();
      
      if (Locks.GetLockInfo(scheduleLog).IsLockedByMe)
        Locks.Unlock(scheduleLog);
    }
    
    /// <summary>
    /// Отключить расписание.
    /// </summary>
    /// <param name="setting">Настройка расписания.</param>
    [Public]
    public void CloseScheduleLog(Starkov.ScheduledReports.ISettingSchedule setting)
    {
      var scheduleLogs = ScheduleLogs.GetAll(s => s.ScheduleSettingId == setting.Id)
        .Where(s => s.Status == ScheduledReports.ScheduleLog.Status.Waiting || s.Status == ScheduledReports.ScheduleLog.Status.Error)
        .OrderByDescending(s => s.StartDate);
      
      foreach (var scheduleLog in scheduleLogs)
      {
        if (scheduleLog.Status == ScheduledReports.ScheduleLog.Status.Error && scheduleLog.Id != scheduleLogs.FirstOrDefault().Id)
          continue;
        
        scheduleLog.Status = ScheduledReports.ScheduleLog.Status.Closed;
        scheduleLog.Save();
      }
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
    
    #region Получение данных о фоновом процессе
    
    /// <summary>
    /// Получить время последнего запуска фонового процесса.
    /// </summary>
    /// <param name="jobId">Идентификатор фонового процесса.</param>
    /// <returns>Дата последней синхронизации.</returns>
    public virtual DateTime? GetLastJobExecuteTime(Guid jobId)
    {
      var command = string.Format(Queries.Module.GetLastJobExecute, jobId.ToString());
      return GetJobExecuteTime(command);
    }
    
    /// <summary>
    /// Получить время следующего запуска фонового процесса.
    /// </summary>
    /// <param name="jobId">Идентификатор фонового процесса.</param>
    /// <returns>Дата последней синхронизации.</returns>
    public virtual DateTime? GetNextJobExecuteTime(Guid jobId)
    {
      var command = string.Format(Queries.Module.GetNextJobExecute, jobId.ToString());
      return GetJobExecuteTime(command);
    }
    
    private DateTime? GetJobExecuteTime(string command)
    {
      try
      {
        var executionResult = Sungero.Docflow.PublicFunctions.Module.ExecuteScalarSQLCommand(command);
        DateTime date;
        if (!(executionResult is DBNull) && executionResult != null && Calendar.TryParseDateTime(executionResult.ToString(), out date))
          return date;
        else
          return null;
      }
      catch (Exception ex)
      {
        Logger.Error("Error while getting next notification date", ex);
        return null;
      }
    }
    
    #endregion
  }
}