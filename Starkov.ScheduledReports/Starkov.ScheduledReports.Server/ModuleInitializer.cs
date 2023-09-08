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
    }
    
    /// <summary>
    /// Инициализация ролей модуля.
    /// </summary>
    public virtual void CreateRoles()
    {// TODO добавить локализацию ролей
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole("Редакторы относительных дат",
                                                                      "Пользователи с правами на редактирование списка относительных дат",
                                                                      Constants.Module.RelativeDatesManagerRole);
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole("Пользователи с доступом к отчетам по расписанию",
                                                                      "Пользователи с правами на создание настроек для отправки отчетов по расписанию",
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
      
      RelativeDates.AccessRights.Save();
      
      var scheduleSettingManagerRole = Roles.GetAll(r => r.Sid == Constants.Module.ScheduleSettingManagerRole).FirstOrDefault();
      if (!ScheduleSettings.AccessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Create, scheduleSettingManagerRole))
        ScheduleSettings.AccessRights.Grant(scheduleSettingManagerRole, DefaultAccessRightsTypes.Create);
      
      ScheduleSettings.AccessRights.Save();
    }
    
    /// <summary>
    /// Инициализация основных записей справочника "Относительная дата".
    /// </summary>
    public virtual void CreateBaseRelativeDates()
    {
      //TODO локализация
      InitializationLogger.Debug("Init: Create base relative dates.");
      CreateBaseRelativeDate("Сегодня", "Текущая дата без времени", Constants.RelativeDate.FunctionGuids.Base.Today, false);
      CreateBaseRelativeDate("Сейчас", "Текущая дата и время", Constants.RelativeDate.FunctionGuids.Base.Now, false);
      CreateBaseRelativeDate("Начало недели", null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfWeek, false);
      CreateBaseRelativeDate("Конец недели", null, Constants.RelativeDate.FunctionGuids.Base.EndOfWeek, false);
      CreateBaseRelativeDate("Начало месяца", null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfMonth, false);
      CreateBaseRelativeDate("Конец месяца", null, Constants.RelativeDate.FunctionGuids.Base.EndOfMonth, false);
      CreateBaseRelativeDate("Начало года", null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfYear, false);
      CreateBaseRelativeDate("Конец года", null, Constants.RelativeDate.FunctionGuids.Base.EndOfYear, false);
      CreateBaseRelativeDate("Сброс времени", "Вернуть дату и время 0:00:00", Constants.RelativeDate.FunctionGuids.Base.Date, false);
      
      CreateBaseRelativeDate("День", "Добавить n дней", Constants.RelativeDate.FunctionGuids.Incremental.AddDays, true);
      CreateBaseRelativeDate("Месяц", "Добавить n месяцев", Constants.RelativeDate.FunctionGuids.Incremental.AddMonths, true);
      CreateBaseRelativeDate("Час", "Добавить n часов", Constants.RelativeDate.FunctionGuids.Incremental.AddHours, true);
      CreateBaseRelativeDate("Минута", "Добавить n минут", Constants.RelativeDate.FunctionGuids.Incremental.AddMinutes, true);
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
  }
}
