﻿using System;
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
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.Today, Starkov.ScheduledReports.Resources.TodayDescription, Constants.RelativeDate.FunctionGuids.Base.Today, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.Now, Starkov.ScheduledReports.Resources.NowDescription, Constants.RelativeDate.FunctionGuids.Base.Now, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.BeginningOfWeek, null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfWeek, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.EndOfWeek, null, Constants.RelativeDate.FunctionGuids.Base.EndOfWeek, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.BeginningOfMonth, null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfMonth, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.EndOfMonth, null, Constants.RelativeDate.FunctionGuids.Base.EndOfMonth, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.BeginningOfYear, null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfYear, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.EndOfYear, null, Constants.RelativeDate.FunctionGuids.Base.EndOfYear, false);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.DateWithoutTime, Starkov.ScheduledReports.Resources.DateWithoutTimeDescription, Constants.RelativeDate.FunctionGuids.Base.Date, false);
      
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.AddDays, Starkov.ScheduledReports.Resources.AddDaysDescr, Constants.RelativeDate.FunctionGuids.Incremental.AddDays, true);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.AddMonths, Starkov.ScheduledReports.Resources.AddMonthsDescr, Constants.RelativeDate.FunctionGuids.Incremental.AddMonths, true);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.AddHours, Starkov.ScheduledReports.Resources.AddHoursDescr, Constants.RelativeDate.FunctionGuids.Incremental.AddHours, true);
      CreateBaseRelativeDate(Starkov.ScheduledReports.Resources.AddMinutes, Starkov.ScheduledReports.Resources.AddMinutesDescr, Constants.RelativeDate.FunctionGuids.Incremental.AddMinutes, true);
    }
    
    private void CreateBaseRelativeDate(string name, string description, Guid guid, bool isIncremental)
    {
      var relativeDate = RelativeDates.GetAll(r => r.FunctionGuid == guid.ToString()).FirstOrDefault();
      if (relativeDate == null)
      {
        InitializationLogger.DebugFormat("Init: Create new base relative date \"{0}\".", name);
        relativeDate = RelativeDates.Create();
        relativeDate.FunctionGuid = guid.ToString();
      }
      
      relativeDate.Name = name;
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

  }
}
