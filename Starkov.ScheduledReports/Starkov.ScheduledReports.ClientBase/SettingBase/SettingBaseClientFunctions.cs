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
      
      var dialog = Dialogs.CreateInputDialog("Тест относительных дат");
      
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
  }
}