using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.SettingBase;

namespace Starkov.ScheduledReports.Client
{
  partial class SettingBaseFunctions
  {
    /// <summary>
    /// Показать диалог ввода относительной даты.
    /// </summary>
    public static bool ShowRelativeDateDialog(Structures.RelativeDate.IRelatedDateInfo relatedDateInfo)
    {
      var result = false;
      
      var dialog = Dialogs.CreateInputDialog("Ввод даты");
      
      var isRelative = dialog.AddBoolean("Относительная дата", relatedDateInfo.IsRelated);
      var date = dialog.AddDate("Дата", false, relatedDateInfo.Date);
      var resultUI = dialog.AddString("Результат", false);
      var expressionUI = dialog.AddMultilineString("Выражение", false, relatedDateInfo.Expression);
      var relativeDate = dialog.AddSelect("Относительная дата", false, RelativeDates.Null);
      var number = dialog.AddInteger("Количество", false);
      var addRelative = dialog.AddHyperlink("Добавить");
      
      resultUI.IsEnabled = false;
      if (relatedDateInfo.IsRelated)
        date.IsVisible = false;
      else
      {
        resultUI.IsVisible = false;
        expressionUI.IsVisible = false;
        relativeDate.IsVisible = false;
        number.IsVisible = false;
        addRelative.IsVisible = false;
      }
      
      var expressionError = string.Empty;
      
      isRelative.SetOnValueChanged(
        (x) =>
        {
          date.IsVisible = date.IsRequired = !x.NewValue.GetValueOrDefault();
          relativeDate.IsVisible = addRelative.IsVisible = expressionUI.IsVisible = expressionUI.IsRequired = resultUI.IsVisible = x.NewValue.GetValueOrDefault();
          number.IsVisible = x.NewValue.GetValueOrDefault() && relativeDate.Value != null && relativeDate.Value.IsIncremental.GetValueOrDefault();
        });

      relativeDate.SetOnValueChanged(
        (r)=>
        {
          number.IsVisible = r.NewValue != null && r.NewValue.IsIncremental.GetValueOrDefault();
        });
      
      expressionUI.SetOnValueChanged(
        (expr)=>
        {
          if (expr.NewValue == expr.OldValue || string.IsNullOrEmpty(expr.NewValue))
            return;
          
          expressionError = string.Empty;
          
          KeyValuePair<DateTime?, string> dateAndExpression;
          try
          {
            dateAndExpression = PublicFunctions.RelativeDate.GetDateFromUIExpression(expr.NewValue);
            if (dateAndExpression.Key.HasValue)
              resultUI.Value = dateAndExpression.Key.Value.ToString();
            else
              throw new Exception("Не удалось вычислить выражение");
          }
          catch (Exception ex)
          {
            expressionError = ex.Message;
          }
          
          if (!string.IsNullOrEmpty(dateAndExpression.Value) && expressionUI.Value != dateAndExpression.Value)
            expressionUI.Value = dateAndExpression.Value;
        });
      
      addRelative.SetOnExecute(
        ()=>
        {
          if (relativeDate.Value == null)
            return;
          
          var isLineBegin = string.IsNullOrEmpty(expressionUI.Value?.Trim());
          expressionUI.Value += Functions.RelativeDate.GetUIExpressionFromRelativeDate(relativeDate.Value, number.Value.GetValueOrDefault(), isLineBegin);
          relativeDate.Value = null;
          number.Value = null;
        });
      
      dialog.SetOnRefresh(
        (_)=>
        {
          if (isRelative.Value == true && !string.IsNullOrEmpty(expressionError))
            _.AddError(expressionError);
        });
      
      if (dialog.Show() == DialogButtons.Ok)
      {
        relatedDateInfo.IsRelated = isRelative.Value.GetValueOrDefault();
        
        relatedDateInfo.Expression = relatedDateInfo.IsRelated
          ? expressionUI.Value
          : date.Value.ToString();
        
        result = true;
      }
      
      return result;
    }
    
    #region Работа с параметрами в интерфейсе
    
    /// <summary>
    /// Редактировать выбранный параметр.
    /// </summary>
    /// <param name="parameter">Строка коллекции параметров.</param>
    public static void EditParameter(ISettingBaseParameters parameter)
    {
      var parameterInfo = PublicFunctions.SettingBase.GetReportParameterInfo(parameter);
      var isChanged = false;
      
      if (parameterInfo.IsEntity)
        isChanged = EditEntityParameter(parameterInfo);
      else
        isChanged = EditSimpleParameter(parameterInfo);
      
      if (isChanged)
        Functions.SettingBase.WriteReportParameterInfo(parameter, parameterInfo);
    }
    
    #region Сущность
    
    /// <summary>
    /// Редактировать параметр на основании сущности.
    /// </summary>
    /// <param name="parameter">Структура с данными параметра.</param>
    public static bool EditEntityParameter(ScheduledReports.Structures.Module.IReportParameterInfo parameterInfo)
    {
      var isChanged = false;
      
      var entities = PublicFunctions.Module.Remote.GetEntitiesByGuid(Guid.Parse(parameterInfo.EntityGuid));
      if (parameterInfo.IsCollection)
      {
        var selectedEntitiesIds = entities.ShowSelectMany();
        if (selectedEntitiesIds.Any())
        {
          parameterInfo.EntityIds = selectedEntitiesIds.Select(_ => _.Id).ToList();
          parameterInfo.DisplayValue =  string.Join("; ", selectedEntitiesIds.Select(_ => _.DisplayValue));
          isChanged = true;
        }
      }
      else
      {
        var dialog = Dialogs.CreateInputDialog("Выбор записи");
        var itemMetadata = Sungero.Metadata.Services.MetadataSearcher.FindModuleItemMetadata(Guid.Parse(parameterInfo.EntityGuid));
        var dialogMethod = typeof(Sungero.Core.ExtensionInputDialog)
          .GetMethod("AddSelect")
          .MakeGenericMethod(itemMetadata.InterfaceType);
        
        object[] args = new object[] { dialog, parameterInfo.DisplayName, true, null };
        var selectResult = dialogMethod.Invoke(null, args);
        
        if (dialog.Show() == DialogButtons.Ok)
        {
          var entity = selectResult.GetType().GetProperty("Value")?.GetValue(selectResult) as Sungero.Domain.Shared.IEntity;
          if (entity != null)
          {
            parameterInfo.EntityIds = new List<long>() { entity.Id };
            parameterInfo.DisplayValue = entity.DisplayValue;
            isChanged = true;
          }
        }
      }
      
      return isChanged;
    }
    
    #endregion
    
    #region Простые типы
    
    /// <summary>
    /// Редактировать параметр простого типа.
    /// </summary>
    /// <param name="parameterInfo">Структура с данными параметра.</param>
    public static bool EditSimpleParameter(ScheduledReports.Structures.Module.IReportParameterInfo parameterInfo)
    {
      var originalValue = parameterInfo.DisplayValue;
      
      // TODO Доработать Перечисления
      // TODO Доработать Простые типы

      var type = System.Type.GetType(parameterInfo.InternalDataTypeName);
      var dialog = Dialogs.CreateInputDialog("Введите значение");

      var title = "Значение";

      switch (parameterInfo.InternalDataTypeName)
      {
        case "System.Boolean":
          #region Логический тип

          var boolInput = dialog.AddBoolean(title, false);
          if (dialog.Show() == DialogButtons.Ok)
            parameterInfo.DisplayValue = boolInput.Value.ToString();
          break;
          #endregion
        case "System.Int32":
          #region Целое число

          var intInput = dialog.AddInteger(title, false);
          if (dialog.Show() == DialogButtons.Ok)
            parameterInfo.DisplayValue = intInput.Value.ToString();
          break;
          #endregion
        case "System.Double":
          #region Дробь

          var doubleInput = dialog.AddDouble(title, false);
          if (dialog.Show() == DialogButtons.Ok)
            parameterInfo.DisplayValue = doubleInput.Value.ToString();
          break;
          #endregion
        case "System.DateTime":
          #region Правка даты

          var relatedInfo = Structures.RelativeDate.RelatedDateInfo.Create();
          relatedInfo.IsRelated = parameterInfo.IsRelatedDate;
          if (!string.IsNullOrEmpty(parameterInfo.DisplayValue))
          {
            if (relatedInfo.IsRelated)
              relatedInfo.Expression = parameterInfo.DisplayValue;
            else
            {
              DateTime date;
              if (Calendar.TryParseDateTime(parameterInfo.DisplayValue, out date))
                relatedInfo.Date = date;
            }
          }

          if (Functions.SettingBase.ShowRelativeDateDialog(relatedInfo))
          {
            parameterInfo.DisplayValue = relatedInfo.Expression;
            parameterInfo.IsRelatedDate = relatedInfo.IsRelated;
          }

          break;
          #endregion
        default:
          #region Строка

          var inputString = dialog.AddString(title, false);
          if (dialog.Show() == DialogButtons.Ok)
            parameterInfo.DisplayValue = inputString.Value;
          break;
          #endregion
      }
      
      return originalValue != parameterInfo.DisplayValue;
    }
    
    #endregion
    
    
    #endregion
    
  }
}