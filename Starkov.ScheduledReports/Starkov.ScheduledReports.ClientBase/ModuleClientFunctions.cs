using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Shared;
using Sungero.Metadata;
using Sungero.Reporting;

namespace Starkov.ScheduledReports.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Показать карточку со Stateview журнала.
    /// </summary>
    public virtual void ShowScheduleLogs()
    {
      var previewLog = Functions.Module.Remote.GetPreviewScheduleLog();
      
      if (previewLog != null)
        previewLog.Show();
    }
    
    public virtual void CreateScheduleSetting()
    {
      var scheduleSetting = PublicFunctions.Module.Remote.CreateScheduleSetting();
      scheduleSetting.Show();
    }

    /// <summary>
    /// Открыть роль Редакторы относительных дат
    /// </summary>
    public virtual void RelativeDatesManagerRoleShow()
    {
      var relativeDatesManagerRole = Roles.GetAll(r => r.Sid == Constants.Module.RelativeDatesManagerRole).FirstOrDefault();
      if (relativeDatesManagerRole != null)
        relativeDatesManagerRole.Show();
    }
    
    /// <summary>
    /// Открыть роль Пользователи с доступом к отчетам по расписанию
    /// </summary>
    public virtual void ScheduleSettingManagerRoleShow()
    {
      var scheduleSettingManagerRole = Roles.GetAll(r => r.Sid == Constants.Module.ScheduleSettingManagerRole).FirstOrDefault();
      if (scheduleSettingManagerRole != null)
        scheduleSettingManagerRole.Show();
    }
      
  }
}