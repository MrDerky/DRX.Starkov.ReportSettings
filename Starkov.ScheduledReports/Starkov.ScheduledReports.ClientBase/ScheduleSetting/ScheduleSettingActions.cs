using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;
using Sungero.Domain.Shared;

namespace Starkov.ScheduledReports.Client
{
  partial class ScheduleSettingReportParamsActions
  {

    public virtual bool CanEditParameter(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

    public virtual void EditParameter(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      // TODO Перенести код
      if (!string.IsNullOrEmpty(_obj.EntityType))
      {
        // Типы сущностей системы
        var entities = PublicFunctions.Module.Remote.GetEntitiesByGuid(Guid.Parse(_obj.EntityType));
        if (entities.Any())
        {
          var selected = entities.ShowSelect();
          if (selected != null)
          {
            _obj.ValueDisplay = selected.DisplayValue;
            _obj.ValueId = selected.Id;
          }
        }
      }
      else
      {
        // TODO Доработать типы
        // Простые типы
        var type = System.Type.GetType(_obj.InternalDataTypeName);
        var dialog = Dialogs.CreateInputDialog("Введите значение");
        
        var title = "Значение";
        
        switch (_obj.InternalDataTypeName)
        {
          case "System.Boolean":
            var input = dialog.AddBoolean(title);
            if (dialog.Show() == DialogButtons.Ok)
              _obj.ValueDisplay = input.Value.ToString();
            break;
          case "System.DateTime": // TODO нужен рефакторинг
            // Относительные даты пример: https://www.elma-bpm.ru/KB/help/Platform/content/User_Filter_relatively_dates_index.html
            var isRelative = dialog.AddBoolean("Относительная дата");
            var date = dialog.AddDate(title, false);
            var relative = dialog.AddSelect(title, false).From(new string[] {"начало месяца", "дней"});
            relative.IsVisible = false;
            isRelative.SetOnValueChanged((x)=>
                                         {
                                           relative.IsVisible = x.NewValue.GetValueOrDefault();
                                           date.IsVisible = !x.NewValue.GetValueOrDefault();
                                         });
            if (dialog.Show() == DialogButtons.Ok)
            {
              _obj.IsRelativeDate = isRelative.Value.GetValueOrDefault();
              _obj.ValueDisplay = isRelative.Value.GetValueOrDefault() ? relative.Value : date.Value.GetValueOrDefault().ToShortDateString();
            }
            break;
          default:
            var inputString = dialog.AddString(title, true);
            if (dialog.Show() == DialogButtons.Ok)
              _obj.ValueDisplay = inputString.Value;
            break;
        }
        
        //        if (_obj.InternalDataTypeName == "System.Boolean")
        //        {
        //          var input = dialog.AddBoolean(title);
        //          if (dialog.Show() == DialogButtons.Ok)
        //          {
        //            _obj.ValueDisplay = input.Value.ToString();
        //          }
        //        }
        //        else if (_obj.InternalDataTypeName == "System.DateTime")
        //        {
//
        //        }
        //        else
        //        {
        //          var input = dialog.AddString(title, true);
//
        //          if (dialog.Show() == DialogButtons.Ok)
        //          {
        //            _obj.ValueDisplay = input.Value;
        //          }
        //        }
      }
      
    }
  }

  partial class ScheduleSettingActions
  {
    public virtual void StartReportWithParameters(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      PublicFunctions.Module.StartSheduleReport(_obj);
    }

    public virtual bool CanStartReportWithParameters(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void StartReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var report = PublicFunctions.Module.GetModuleReportByGuid(Guid.Parse(_obj.ModuleGuid), Guid.Parse(_obj.ReportGuid));
      if (report != null)
        report.Open();
      
      //      var rep  = Sungero.Docflow.Reports.GetApprovalRuleCardReport();
      //      rep.Open();
      
//      PublicFunctions.ScheduleSetting.FillReportParams(_obj, report);
      PublicFunctions.ScheduleSetting.FillReportParamsClear(_obj, report);
    }

    public virtual bool CanStartReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }


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
      }
    }

    public virtual bool CanSetReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}