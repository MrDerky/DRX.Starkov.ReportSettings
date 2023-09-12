using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports.Client
{
  partial class ScheduleSettingFunctions
  {

    /// <summary>
    /// Выбрать отчет.
    /// </summary>       
    public void SelectReport()
    {
      var reports = new List<ScheduledReports.Structures.Module.IReportInfo>();
      var modulesInfo = PublicFunctions.Module.GetReportsModuleNames();
      if (!modulesInfo.Any())
      {
        Dialogs.ShowMessage("Модулей с отчетами не найдено", MessageType.Error);
        return;
      }
      
      var dialog = Dialogs.CreateInputDialog("Выбор отчета");
      var module = dialog.AddSelect("Модуль", true).From(modulesInfo.Select(m => m.Value).ToArray());
      var report = dialog.AddSelect("Отчет", true);
      
      module.SetOnValueChanged((m) =>
                               {
                                 reports = PublicFunctions.Module.GetModuleReportsStructure(modulesInfo.FirstOrDefault(i => i.Value == m.NewValue).Key);
                                 report.From(reports.Select(r => r.LocalizedName).ToArray());
                               });
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        _obj.ModuleName = module.Value; // TODO Пока вопрос нужно ли хранить модуль
        _obj.ModuleGuid = modulesInfo.FirstOrDefault(m => m.Value == module.Value).Key.ToString();
        _obj.ReportName = report.Value;
        _obj.ReportGuid = reports.FirstOrDefault(r => r.LocalizedName == report.Value).NameGuid.ToString();
        
        Functions.ScheduleSetting.SetPropertyStates(_obj);
      }
    }

  }
}