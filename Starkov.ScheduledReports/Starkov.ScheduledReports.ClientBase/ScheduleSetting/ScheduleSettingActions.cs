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
      // TODO Возмно следует перенести код
      if (!string.IsNullOrEmpty(_obj.EntityGuid))
      {
        #region Типы сущностей системы
        
        var entities = PublicFunctions.Module.Remote.GetEntitiesByGuid(Guid.Parse(_obj.EntityGuid));
        if (entities.Any())
        {
          var selected = entities.ShowSelect();
          if (selected != null)
          {
            _obj.ViewValue = selected.DisplayValue;
            _obj.EntityId = selected.Id;
          }
        }
        
        #endregion
      }
      else
      {
        // TODO Доработать Простые типы
        #region Простые типы
        
        var type = System.Type.GetType(_obj.InternalDataTypeName);
        var dialog = Dialogs.CreateInputDialog("Введите значение");
        
        var title = "Значение";
        
        switch (_obj.InternalDataTypeName)
        {
          case "System.Boolean":
            #region Логический тип
            
            var boolInput = dialog.AddBoolean(title, true);
            if (dialog.Show() == DialogButtons.Ok)
              _obj.ViewValue = boolInput.Value.ToString();
            break;
            #endregion
          case "System.Int32":
            #region Целое число
            
            var intInput = dialog.AddInteger(title, false);
            if (dialog.Show() == DialogButtons.Ok)
              _obj.ViewValue = intInput.Value.ToString();
            break;
            #endregion
          case "System.Double":
            #region Дробь
            
            var doubleInput = dialog.AddDouble(title, false);
            if (dialog.Show() == DialogButtons.Ok)
              _obj.ViewValue = doubleInput.Value.ToString();
            break;
            #endregion
          case "System.DateTime":
            #region Правка даты
            
            var isRelative = dialog.AddBoolean("Относительная дата");
            var date = dialog.AddDate("Дата", false);
            
            var relative = dialog.AddSelect("Период", false, Starkov.ScheduledReports.RelativeDates.Null).Where(r => r.Status != RelativeDate.Status.Closed);
            var increment = dialog.AddInteger("Количество", false);
            
            increment.IsVisible = false;
            relative.IsVisible = false;
            
            isRelative.SetOnValueChanged((x) =>
                                         {
                                           date.IsEnabled = !x.NewValue.GetValueOrDefault();
                                           relative.IsVisible = x.NewValue.GetValueOrDefault();
                                           increment.IsVisible = x.NewValue.GetValueOrDefault() && relative.Value != null && relative.Value.IsIncremental.GetValueOrDefault();
                                         });
            
            relative.SetOnValueChanged((r) =>
                                       {
                                         var isIncremental = r.NewValue != null && r.NewValue.IsIncremental.GetValueOrDefault();
                                         increment.IsVisible = isIncremental;
                                         if (!isIncremental)
                                           increment.Value = null;
                                       });
            
            dialog.SetOnRefresh((d) =>
                                {
                                  try
                                  {
                                    if (isRelative.Value.GetValueOrDefault() && relative.Value != null)
                                      date.Value = PublicFunctions.RelativeDate.CalculateDate(relative.Value, null, increment.Value);
                                  }
                                  catch (Exception ex)
                                  {
                                    d.AddError("Не корректное значение");
                                  }
                                });
            
            // Заполнить значения из карточки.
            if (_obj.IsRelativeDate.GetValueOrDefault())
            {
              isRelative.Value = true;
              relative.Value = PublicFunctions.Module.Remote.GetRelativeDate(_obj.EntityId.GetValueOrDefault());
              increment.Value = Functions.ScheduleSetting.GetIncrementForRelativeDateFromViewValue(_obj.ViewValue);
            }
            else
            {
              DateTime dateValue;
              if (Calendar.TryParseDateTime(_obj.ViewValue, out dateValue))
                date.Value = dateValue;
            }
            
            if (dialog.Show() == DialogButtons.Ok)
            {
              _obj.IsRelativeDate = isRelative.Value.GetValueOrDefault();
              
              if (isRelative.Value.GetValueOrDefault() && relative.Value != null)
              {
                _obj.ViewValue = Functions.ScheduleSetting.BuildViewValueForRelativeDate(relative.Value, increment.Value);
                _obj.EntityId = relative.Value.Id;
              }
              else
                _obj.ViewValue = date.Value.GetValueOrDefault().ToString();
            }
            
            break;
            #endregion
          default:
            #region Строка
            
            var inputString = dialog.AddString(title, false);
            if (dialog.Show() == DialogButtons.Ok)
              _obj.ViewValue = inputString.Value;
            break;
            #endregion
        }
        
        #endregion
      }
    }
  }

  partial class ScheduleSettingActions
  {
    public virtual void DisableSchedule(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      _obj.Status = Status.Closed;
      _obj.Save();
      
      PublicFunctions.Module.CloseScheduleLog(_obj);
      
      Functions.ScheduleSetting.SetPropertyStates(_obj);
      _obj.State.Controls.ScheduleReportState.Refresh();
    }

    public virtual bool CanDisableSchedule(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Status.Active;
    }

    public virtual void EnableSchedule(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.DateEnd.HasValue && _obj.DateEnd.Value <= Calendar.Now)
      {
        e.AddError("Завершение расписания должно быть больше текущего времени");
        return;
      }
      
      var nextDate = Functions.ScheduleSetting.Remote.GetNextPeriod(_obj);
      if (nextDate <= Calendar.Now)
      {
        e.AddError("Следующий запуск не может быть меньше текущего времени.");
        return;
      }
      
      try
      {
        PublicFunctions.Module.EnableSchedule(_obj);
        var message = string.Format("Запланирована отправка отчета по расписанию.{0}Время следующего запуска {1}", Environment.NewLine, nextDate);
        Dialogs.NotifyMessage(message);
      }
      catch (Exception ex)
      {
        // TODO Доработать обработку ошибок в EnableSchedule
        e.AddError(ex.Message);
        return;
      }
      
      _obj.Status = Status.Active;
      _obj.Save();
      
      Functions.ScheduleSetting.SetPropertyStates(_obj);
      _obj.State.Controls.ScheduleReportState.Refresh();
    }

    public virtual bool CanEnableSchedule(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Status.Closed && (!_obj.DateEnd.HasValue || _obj.DateEnd.HasValue && _obj.DateEnd.Value > Calendar.Now);
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
      return !string.IsNullOrEmpty(_obj.ReportGuid);
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
      return !string.IsNullOrEmpty(_obj.ReportGuid);
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