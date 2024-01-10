using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.RelativeDate;

namespace Starkov.ScheduledReports.Client
{
  partial class RelativeDateFunctions
  {

    /// <summary>
    /// Показать диалог ввода относительной даты.
    /// </summary>
    [Public]
    public static void ShowRelativeDateDialog()
    {
      var dialog = Dialogs.CreateInputDialog("Тест относительных дат");
      
      var resultUI = dialog.AddString("Результат", false);
      resultUI.IsEnabled = false;
      var expression = dialog.AddMultilineString("Выражение", false);
      //expression.IsVisible = false;
      var expressionUI = dialog.AddMultilineString("Выражение", false);
      //      expressionUI.IsEnabled = false;
      
      var relativeDate = dialog.AddSelect("Относительная дата", false, RelativeDates.Null);
      var number = dialog.AddInteger("Количество", false);
      //      var hour = dialog.AddString("часов", false);
      //      var minutes = dialog.AddString("минут", false);
      number.IsVisible = false;
      var addRelative = dialog.AddHyperlink("Добавить");
      var clear = dialog.AddHyperlink("Очистить");
      
      //      time.SetOnValueChanged(
      //        (t)=>
      //        {
      //          var pattern = @"(\d\d)\:(-*\d\d)";
      //          var rg = new System.Text.RegularExpressions.Regex(pattern);
      //          var matches = rg.Matches(t.NewValue);
      //          var h = matches.Count > 0 && matches[0].Groups[0] > 0 ? matches[0].Groups[0].Value : 0;
      //        });
      
      var errorText = string.Empty;
      
      relativeDate.SetOnValueChanged(
        (r)=>
        {
          number.IsVisible = r.NewValue != null && r.NewValue.IsIncremental.GetValueOrDefault();
        });
      
      expressionUI.SetOnValueChanged(
        (expr)=>
        {
          errorText = string.Empty;
          
          KeyValuePair<DateTime?, string> result;
          try
          {
            result = GetDateFromUIExpression(expr.NewValue);
            if (result.Key.HasValue)
              resultUI.Value = result.Key.Value.ToString();
            else
              throw new Exception("Не удалось вычислить выражение");
          }
          catch (Exception ex)
          {
            errorText = ex.Message;
          }
          
          if (!string.IsNullOrEmpty(result.Value) && expressionUI.Value != result.Value)
            expressionUI.Value = result.Value;
        });
      
      addRelative.SetOnExecute(
        ()=>
        {
          if (relativeDate.Value == null)
            return;
          
          //expression.Value += Functions.RelativeDate.GetExpressionFromRelativeDate(relativeDate.Value, number.Value.GetValueOrDefault()) + ";";
          ////result.Value = Functions.RelativeDate.GetDateFromExpression(expression.Value);
          //resultUI.Value = Functions.RelativeDate.GetDateFromUIExpression(expression.Value).ToString();
          expressionUI.Value += Functions.RelativeDate.GetUIExpressionFromRelativeDate(relativeDate.Value, number.Value.GetValueOrDefault());
          relativeDate.Value = null;
          number.Value = null;
        });
      
      clear.SetOnExecute(
        ()=>
        {
          expression.Value = string.Empty;
          expressionUI.Value = string.Empty;
          //result.Value = Functions.RelativeDate.GetDateFromExpression(expression.Value);
          resultUI.Value = string.Empty;
        });
      
      dialog.SetOnRefresh(
        (_)=>
        {
          if (!string.IsNullOrEmpty(errorText))
            _.AddError(errorText);
        });
      
      dialog.Show();
    }
    
    /// <summary>
    /// Получить дату из выражения для пользователя.
    /// </summary>
    /// <param name="expression">Строка с выражением.</param>
    /// <returns>Дата и отформатированное выражение.</returns>
    public static System.Collections.Generic.KeyValuePair<DateTime?, string> GetDateFromUIExpression(string expression)
    {
      var newExpression = string.Empty;
      var pattern = @"([+,-]|)(\d*|)(\[(.*?)\]|[^+->].[^+-]*)";
      var rg = new System.Text.RegularExpressions.Regex(pattern);

      DateTime? resultDate = null;
      foreach (System.Text.RegularExpressions.Match match in rg.Matches(expression))
      {
        var operation = match?.Groups[1]?.ToString();
        var number = 1;
        if (!String.IsNullOrEmpty(match?.Groups[2]?.ToString()))
          int.TryParse(match?.Groups[2]?.ToString(), out number);
        
        if (operation == "-")
          number = 0 - number;
        
        var relativeDateName = !String.IsNullOrEmpty(match?.Groups[4]?.ToString())
          ? match?.Groups[4]?.ToString()
          : match?.Groups[3]?.ToString();
        
        var relativeDate = PublicFunctions.RelativeDate.Remote.GetRelativeDate(relativeDateName, false);
        
        if (relativeDate == null)
          throw new Exception(string.Format("Не найдена относительная дата с именем «{0}»", relativeDateName));
        
        resultDate = Functions.RelativeDate.CalculateDate(relativeDate, resultDate, number);
        newExpression += Functions.RelativeDate.GetUIExpressionFromRelativeDate(relativeDate, number);
      }
      
      return new System.Collections.Generic.KeyValuePair<DateTime?, string> (resultDate, newExpression);
    }

  }
}