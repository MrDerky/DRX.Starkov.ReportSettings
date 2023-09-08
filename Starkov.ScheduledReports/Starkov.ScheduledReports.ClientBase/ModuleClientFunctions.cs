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
      
      
    /// <summary>
    /// 
    /// </summary>
    public virtual void Function2()
    {
      
//       Type tDocflow = Type.GetType("Sungero.Docflow.Reports, Sungero.Domain.Interfaces, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"); //className);
//        Type t = Type.GetType(tDocflow);
//        if (t != null)
//        {
//          var methodGetAll = t.GetMethods().Where(m => m.Name == "GetAll" && m.GetParameters().Length == 0).FirstOrDefault();
//          if (methodGetAll != null)
//          {
//            var reports = (IEnumerable<IReport>)methodGetAll.Invoke(null, null);
//            foreach (var report in reports)
//            {
//              var reportMetaData = Sungero.Metadata.Services.MetadataSearcher.FindModuleItemMetadata(report.Info.ReportTypeId);
//              if (reportMetaData != null)
//                reportList.Add(((ReportMetadata)reportMetaData).LocalizedName);
//            }
//
//          }
//        }
    }
    


  }
}