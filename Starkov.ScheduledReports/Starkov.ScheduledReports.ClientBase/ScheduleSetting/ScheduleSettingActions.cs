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
      if (!string.IsNullOrEmpty(_obj.EntityGuid))
      {
        // Типы сущностей системы
        var entities = PublicFunctions.Module.Remote.GetEntitiesByGuid(Guid.Parse(_obj.EntityGuid));
        if (entities.Any())
        {
          var selected = entities.ShowSelect();
          if (selected != null)
          {
            _obj.ValueText = selected.DisplayValue;
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
            #region Логический тип
            
            var input = dialog.AddBoolean(title);
            if (dialog.Show() == DialogButtons.Ok)
              _obj.ValueText = input.Value.ToString();
            break;
            #endregion
          case "System.DateTime": // TODO нужен рефакторинг
            #region Правка даты
            
            //            var dialog = Dialogs.CreateInputDialog("Изменить дату");
            var isRelative = dialog.AddBoolean("Относительная дата");
            var date = dialog.AddDate("Дата", false);
            
            var relative = dialog.AddSelect("Период", false, Starkov.ScheduledReports.RelativeDates.Null).Where(r => r.Status != RelativeDate.Status.Closed);
            var increment = dialog.AddInteger("Количество", false);
            //var isCustomInput = dialog.AddBoolean("Ввести вручную");
            
            //            isCustomInput.IsVisible = false;
            increment.IsVisible = false;
            relative.IsVisible = false;
            
            isRelative.SetOnValueChanged((x) =>
                                         {
                                           date.IsVisible = !x.NewValue.GetValueOrDefault();
                                           relative.IsVisible = x.NewValue.GetValueOrDefault();
                                           increment.IsVisible = x.NewValue.GetValueOrDefault() && relative.Value != null && relative.Value.IsIncremental.GetValueOrDefault();
                                           //                                           isCustomInput.IsVisible = x.NewValue.GetValueOrDefault();
                                         });
            
            relative.SetOnValueChanged((r) =>
                                       {
                                         //relativeText.Value = r.NewValue.RelativeExpression;
                                         increment.IsVisible = r.NewValue != null && r.NewValue.IsIncremental.GetValueOrDefault();
                                       });
            
            increment.SetOnValueChanged((n) =>
                                        {
//                                          if (n.NewValue < -100)
//                                            increment = -100;
//                                          else if (n.NewValue > 100)
//                                            increment = 100; //TODO вынести в настройки
                                        });
            
            //            isCustomInput.SetOnValueChanged((x) =>
            //                                            {
            //                                              relative.IsVisible = !x.NewValue.GetValueOrDefault();
            //                                              relativeText.IsVisible = x.NewValue.GetValueOrDefault();
            //                                            });
            
            // TODO Рефакторить это безобразие
            if (dialog.Show() == DialogButtons.Ok)
            {
              _obj.IsRelativeDate = isRelative.Value.GetValueOrDefault();
              
              if (isRelative.Value.GetValueOrDefault())
              {
                _obj.ValueText = string.Join(" ", increment.Value, relative.Value.Name);
                _obj.ValueId = relative.Value.Id;
              }
              else
                _obj.ValueText = date.Value.GetValueOrDefault().ToString();
            }
            
            break;
            #endregion
          default:
            #region Строка
            
            var inputString = dialog.AddString(title, true);
            if (dialog.Show() == DialogButtons.Ok)
              _obj.ValueText = inputString.Value;
            break;
            #endregion
        }
      }
      
    }
  }

  partial class ScheduleSettingActions
  {
    public virtual void TrySendReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      PublicFunctions.Module.StartSheduleReport(_obj);
    }

    public virtual bool CanTrySendReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }


    public virtual void StartReportWithParameters(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        var report = PublicFunctions.Module.GetModuleReportByGuid(Guid.Parse(_obj.ModuleGuid), Guid.Parse(_obj.ReportGuid));
        if (report == null)
          return;
        
        PublicFunctions.Module.FillReportParams(report, _obj);
        report.Open();
      }
      catch (Exception ex)
      {
        e.AddError("Не удалось выполнить отчет. Проверьте параметры.");
      }
    }

    public virtual bool CanStartReportWithParameters(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void StartReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        var report = PublicFunctions.Module.GetModuleReportByGuid(Guid.Parse(_obj.ModuleGuid), Guid.Parse(_obj.ReportGuid));
        if (report == null)
          return;
        
        report.Open();
        
        PublicFunctions.ScheduleSetting.SaveReportParams(_obj, report);
      }
      catch (Exception ex)
      {
        e.AddError("Не удалось выполнить отчет. Проверьте параметры.");
      }
      
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
      
      //var entityReport = dialog.AddSelect("Отчеты сущ", false).From(PublicFunctions.Module.Remote.GetEntityReports().ToArray());
      
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