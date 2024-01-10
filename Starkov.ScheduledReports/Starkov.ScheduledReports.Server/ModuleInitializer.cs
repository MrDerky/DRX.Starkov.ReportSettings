using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace Starkov.ScheduledReports.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateRoles();
      GrandRights();
      CreateBaseRelativeDates();
      CreatePreviewScheduleLog();
      CreateReportSettings();
    }
    
    /// <summary>
    /// Инициализация ролей модуля.
    /// </summary>
    public virtual void CreateRoles()
    {
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Starkov.ScheduledReports.Resources.RelativeDateEditorsRoleName,
                                                                      Starkov.ScheduledReports.Resources.RelativeDateEditorsRoleDescription,
                                                                      Constants.Module.RelativeDatesManagerRole);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(Starkov.ScheduledReports.Resources.ScheduledReportsAccessRoleName,
                                                                      Starkov.ScheduledReports.Resources.ScheduledReportsAccessRoleDescription,
                                                                      Constants.Module.ScheduleSettingManagerRole);
    }
    
    /// <summary>
    /// Выдача прав на справочники модуля.
    /// </summary>
    public virtual void GrandRights()
    {
      var allUsers = Roles.AllUsers;
      if (!RelativeDates.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Read, allUsers))
        RelativeDates.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
      
      var relativeDatesManagerRole = Roles.GetAll(r => r.Sid == Constants.Module.RelativeDatesManagerRole).FirstOrDefault();
      if (!RelativeDates.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Change, relativeDatesManagerRole))
        RelativeDates.AccessRights.Grant(relativeDatesManagerRole, DefaultAccessRightsTypes.Change);
      
      var scheduleSettingManagerRole = Roles.GetAll(r => r.Sid == Constants.Module.ScheduleSettingManagerRole).FirstOrDefault();
      if (!ScheduleSettings.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Create, scheduleSettingManagerRole))
        ScheduleSettings.AccessRights.Grant(scheduleSettingManagerRole, DefaultAccessRightsTypes.Create);
      
      if (!ScheduleLogs.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Read, scheduleSettingManagerRole))
        ScheduleLogs.AccessRights.Grant(scheduleSettingManagerRole, DefaultAccessRightsTypes.Create);
      
      RelativeDates.AccessRights.Save();
      ScheduleSettings.AccessRights.Save();
      ScheduleLogs.AccessRights.Save();
    }
    
    /// <summary>
    /// Инициализация основных записей справочника "Относительная дата".
    /// </summary>
    public virtual void CreateBaseRelativeDates()
    {
      InitializationLogger.Debug("Init: Create base relative dates.");
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.Today, string.Empty, string.Empty, Starkov.ScheduledReports.Resources.TodayDescription, Constants.RelativeDate.FunctionGuids.Base.Today, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.Now, string.Empty, string.Empty, Starkov.ScheduledReports.Resources.NowDescription, Constants.RelativeDate.FunctionGuids.Base.Now, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.BeginningOfWeek, string.Empty, string.Empty, null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfWeek, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.EndOfWeek, string.Empty, string.Empty, null, Constants.RelativeDate.FunctionGuids.Base.EndOfWeek, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.BeginningOfMonth, string.Empty, string.Empty, null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfMonth, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.EndOfMonth, string.Empty, string.Empty, null, Constants.RelativeDate.FunctionGuids.Base.EndOfMonth, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.BeginningOfYear, string.Empty, string.Empty, null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfYear, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.EndOfYear, string.Empty, string.Empty, null, Constants.RelativeDate.FunctionGuids.Base.EndOfYear, false);
      
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.DateWithoutTime, string.Empty, string.Empty, Starkov.ScheduledReports.Resources.DateWithoutTimeDescription, Constants.RelativeDate.FunctionGuids.Base.Date, false);
      
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.AddDays, "Дня", "Дней", Starkov.ScheduledReports.Resources.AddDaysDescr, Constants.RelativeDate.FunctionGuids.Incremental.AddDays, true);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.AddMonths, "Месяца", "Месяцев", Starkov.ScheduledReports.Resources.AddMonthsDescr, Constants.RelativeDate.FunctionGuids.Incremental.AddMonths, true);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.AddHours, "Часа", "Часов", Starkov.ScheduledReports.Resources.AddHoursDescr, Constants.RelativeDate.FunctionGuids.Incremental.AddHours, true);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.AddMinutes, "Минуты", "Минут", Starkov.ScheduledReports.Resources.AddMinutesDescr, Constants.RelativeDate.FunctionGuids.Incremental.AddMinutes, true);
    }
    
    private void CreateBaseRelativeDate(string name, string pluralName2, string pluralName5, string description, Guid guid, bool isIncremental)
    {
      var relativeDate = RelativeDates.GetAll(r => r.FunctionGuid == guid.ToString()).FirstOrDefault();
      if (relativeDate == null)
      {
        InitializationLogger.DebugFormat("Init: Create new base relative date \"{0}\".", name);
        relativeDate = RelativeDates.Create();
        relativeDate.FunctionGuid = guid.ToString();
      }
      
      relativeDate.Name = name;
      if (!string.IsNullOrEmpty(pluralName2))
        relativeDate.PluralName2 = pluralName2;
      if (!string.IsNullOrEmpty(pluralName5))
        relativeDate.PluralName5 = pluralName5;
      
      relativeDate.Description = description;
      relativeDate.IsIncremental = isIncremental;
      relativeDate.Save();
    }
    
    /// <summary>
    /// Создание записи справочника для вывода StateView.
    /// </summary>
    private void CreatePreviewScheduleLog()
    {
      var previewLog = ScheduleLogs.GetAll().FirstOrDefault(s => s.Status == ScheduledReports.ScheduleLog.Status.Preview);
      if (previewLog != null)
        return;
      
      previewLog = ScheduleLogs.Create();
      previewLog.Status = ScheduledReports.ScheduleLog.Status.Preview;
      previewLog.AccessRights.Grant(Roles.AllUsers, DefaultAccessRightsTypes.Read);
      previewLog.Save();
      InitializationLogger.DebugFormat("Init: Created new previewLog id={0}", previewLog.Id);
    }
    
    #region Инициализация записей справочника настройки отчета.
    
    /// <summary>
    /// Создать записи с настройками отчетов.
    /// </summary>
    private void CreateReportSettings()
    {
      // Отчет "Исполнительская дисциплина по подразделениям".
      CreateAndFillReportSetting(Guid.Parse("23fc035e-72bf-4bd9-9659-a21ad2378f43"),
                                 new Dictionary<string, string>()
                                 {
                                   { "PeriodBegin", Starkov.ScheduledReports.Resources.PeriodFrom },
                                   { "PeriodEnd", Starkov.ScheduledReports.Resources.PeriodTo }
                                 });

      
      // Отчет "Сводный отчет по правилам согласования".
      CreateAndFillReportSetting(Guid.Parse("65a79eb2-8bae-4640-b817-e033c8ba9589"),
                                 new Dictionary<string, string>()
                                 {
                                   { "DocumentFlow", Starkov.ScheduledReports.Resources.DocumentFlow },
                                   { "Category", Starkov.ScheduledReports.Resources.Category }
                                 });

    }
    
    /// <summary>
    /// Создать запись с настройкой отчета и заполнить параметры.
    /// </summary>
    /// <param name="reportGuid">Идентификатор отчета.</param>
    /// <param name="parameters">Словарь с параметрами: ключ - имя параметра отчета, значение - отображаемое значение.</param>
    private void CreateAndFillReportSetting(Guid reportGuid, Dictionary<string, string> parameters)
    {
      var reportSetting = CreateReportSetting(reportGuid);
      if (reportSetting == null)
        return;

      foreach (var parameter in parameters)
        SetReportParameterDisplayName(reportSetting, parameter.Key, parameter.Value);
      
      if (reportSetting.State.IsChanged)
        reportSetting.Save();
    }
    
    /// <summary>
    /// Создать запись с настройкой отчета.
    /// </summary>
    /// <param name="reportGuid">Идентификатор отчета.</param>
    private IReportSetting CreateReportSetting(Guid reportGuid)
    {
      InitializationLogger.DebugFormat("Init: Create Report Setting For report guid={0}", reportGuid);
      
      var reportSetting = ReportSettings.GetAll(r => r.ReportGuid == reportGuid.ToString()).FirstOrDefault();
      if (reportSetting == null)
        reportSetting = ReportSettings.Create();
      
      var reportMetaData = Starkov.ScheduledReports.PublicFunctions.Module.GetReportMetaData(reportGuid);
      if (reportMetaData == null)
        return reportSetting;
      
      reportSetting.ReportGuid = reportGuid.ToString();
      reportSetting.ReportName = reportMetaData.LocalizedName;
      reportSetting.ModuleGuid = reportMetaData.ModuleMetadata.NameGuid.ToString();
      
      Starkov.ScheduledReports.PublicFunctions.SettingBase.SaveReportParams(reportSetting);
      InitializationLogger.DebugFormat("Init: Created Report Setting For report {0}", reportSetting.ReportName);
      return reportSetting;
    }
    
    private void SetReportParameterDisplayName(IReportSetting reportSetting, string parameterName, string displayName)
    {
      var parameter = reportSetting.Parameters.FirstOrDefault(p => p.ParameterName == parameterName);
      if (parameter != null && reportSetting.State.IsInserted)
        parameter.DisplayName = displayName;
    }
    
    #endregion
    
  }
}
