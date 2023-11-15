using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.SettingBase;
using Sungero.Domain.Shared;

namespace Starkov.ScheduledReports.Client
{
  partial class SettingBaseParametersActions
  {

    public virtual bool CanEditParameterValue(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

    public virtual void EditParameterValue(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      // TODO Возможно следует перенести код
      if (!string.IsNullOrEmpty(_obj.EntityGuid))
      {
        #region Типы сущностей системы
        
        var entities = PublicFunctions.Module.Remote.GetEntitiesByGuid(Guid.Parse(_obj.EntityGuid));

        var dialog = Dialogs.CreateInputDialog("Выбор записи");
        var itemMetadata = Sungero.Metadata.Services.MetadataSearcher.FindModuleItemMetadata(Guid.Parse(_obj.EntityGuid));
        var dialogMethod = typeof(Sungero.Core.ExtensionInputDialog)
          .GetMethod("AddSelect")
          .MakeGenericMethod(itemMetadata.InterfaceType);
        
        object[] args = new object[] { dialog, _obj.DisplayName, true, null };
        var selectResult = dialogMethod.Invoke(null, args);
        
        if (dialog.Show() == DialogButtons.Ok)
        {
          var entity = selectResult.GetType().GetProperty("Value")?.GetValue(selectResult) as Sungero.Domain.Shared.IEntity;
          
          if (entity != null)
          {
            _obj.ViewValue = entity.DisplayValue;
            _obj.EntityId = entity.Id;
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
            
            var boolInput = dialog.AddBoolean(title, false);
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
            if (_obj.EntityId.HasValue)
            {
              isRelative.Value = true;
              relative.Value = PublicFunctions.RelativeDate.Remote.GetRelativeDate(_obj.EntityId.GetValueOrDefault(), false);
              increment.Value = Functions.SettingBase.GetIncrementForRelativeDateFromViewValue(_obj.ViewValue);
            }
            else
            {
              DateTime dateValue;
              if (!string.IsNullOrEmpty(_obj.ViewValue) && Calendar.TryParseDateTime(_obj.ViewValue, out dateValue))
                date.Value = dateValue;
            }
            
            if (dialog.Show() == DialogButtons.Ok)
            {
              if (isRelative.Value.GetValueOrDefault() && relative.Value != null)
              {
                _obj.ViewValue = Functions.SettingBase.BuildViewValueForRelativeDate(relative.Value, increment.Value);
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

    public virtual bool CanClearParameterValue(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return !string.IsNullOrEmpty(_obj.ViewValue);
    }

    public virtual void ClearParameterValue(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      _obj.ViewValue = null;
      _obj.EntityId = null;
    }
  }

  partial class SettingBaseActions
  {


    public virtual void StartReportWithParameters(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        var report = PublicFunctions.Module.GetModuleReportByGuid(Guid.Parse(_obj.ModuleGuid), Guid.Parse(_obj.ReportGuid));
        if (report == null)
          return;
        
        Functions.SettingBase.FillReportParams(_obj, report);
        report.Open();
      }
      catch (Exception ex)
      {
        e.AddError(SettingBases.Resources.FillRequiredParametersError);
      }
    }

    public virtual bool CanStartReportWithParameters(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Functions.SettingBase.IsFillReportParamsAny(_obj);
    }

    public virtual void StartReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        var report = PublicFunctions.Module.GetModuleReportByGuid(Guid.Parse(_obj.ModuleGuid), Guid.Parse(_obj.ReportGuid));
        if (report == null)
          return;
        
        report.Open();
        
        Functions.SettingBase.WriteParamsToReport(_obj, report);
      }
      catch (Exception ex)
      {
        e.AddError(SettingBases.Resources.FillRequiredParametersError);
      }
    }

    public virtual bool CanStartReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !string.IsNullOrEmpty(_obj.ReportGuid);
    }

  }


}