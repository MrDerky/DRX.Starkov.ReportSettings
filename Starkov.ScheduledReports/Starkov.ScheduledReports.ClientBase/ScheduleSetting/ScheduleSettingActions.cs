using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports.Client
{
  partial class ScheduleSettingReportParamsActions
  {

    public virtual bool CanSetParameter(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

    public virtual void SetParameter(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
//      var type = Type.GetType(_obj.EntityType);
var entityMetadata = Sungero.Metadata.Services.MetadataSearcher.FindEntityMetadata(Guid.Parse(_obj.EntityType)).GetType();
      var entityInfo = entityMetadata as Sungero.Domain.Shared.IEntityInfo;
      var entity = entityMetadata as Sungero.Domain.Shared.IEntity;
      if (entityInfo != null)
      {
        var dialog = Dialogs.CreateSelectTypeDialog("Выбор", entityInfo);
        dialog.Show();
      }
      
    }
  }

  partial class ScheduleSettingActions
  {

    public virtual void SetReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var reports = new List<ScheduledReports.Structures.Module.IReportInfo>();
      var modulesInfo = PublicFunctions.Module.GetReportsModuleNames();
      if (!modulesInfo.Any())
      {
        Dialogs.ShowMessage("Модулей с отчетами не найдено", MessageType.Error);
        return;
      }
      
      var dialog = Dialogs.CreateInputDialog("Выбор отчета");
      var modules = dialog.AddSelect("Модуль", true).From(modulesInfo.Select(m => m.Value).ToArray());
      var report = dialog.AddSelect("Отчет", true);
      
      modules.SetOnValueChanged((m) =>
                                {
                                  reports = PublicFunctions.Module.GetModuleReportsStructure(modulesInfo.FirstOrDefault(i => i.Value == m.NewValue).Key);
                                  report.From(reports.Select(r => r.LocalizedName).ToArray());
                                });
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        _obj.ReportName = report.Value;
        _obj.ReportGuid = reports.FirstOrDefault(r => r.LocalizedName == report.Value).NameGuid.ToString();
      }
    }

    public virtual bool CanSetReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}