using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.RelativeDate;

namespace Starkov.ScheduledReports.Server
{
  partial class RelativeDateFunctions
  {

    /// <summary>
    /// Вычислить относительную дату.
    /// </summary>
    [Public, Remote]
    public static DateTime? CalculateRelativeDate(DateTime? baseDate, string relativeExpression)
    {
      var date = baseDate;
      if (date == null || string.IsNullOrEmpty(relativeExpression))
        return date;
      
      return null;
      // var expr = Convert.ToDouble(
    }
    
    /// <summary>
    /// Вычислить относительную дату.
    /// </summary>
    [Public, Remote]
    public static string CalculateExpression(string relativeExpression)
    {
      //      var result = Convert.ToDouble(new System.Data.DataTable().Compute(relativeExpression, null));
      //      var result = new System.Data.DataTable().Compute(relativeExpression, null);
      var result = RelativeDateCalculator.Calculator.Calculate(relativeExpression);
      
      
      return result.ToString();
    }
    
    /// <summary>
    /// Получить текст подсказки для относительного выражения.
    /// </summary>
    [Remote]
    public StateView GetRelativeDateState()
    {
      var stateView = StateView.Create();
      
      var block = stateView.AddBlock();
      
      try
      {
        block.AddLabel("Пример полученного результата: " + CalculateExpression(_obj.RelativeExpression));
      }
      catch (Exception ex)
      {
        block.AddLabel("Ошибка при обработке: " + ex.Message);
        block.Background = Colors.Common.Red;
      }
      
      return stateView;
    }
    
  }
}

namespace Starkov.ScheduledReports.RelativeDateCalculator
{
  public static class Calculator
  {
    private static System.Text.RegularExpressions.Regex regexBasePart = new System.Text.RegularExpressions.Regex(@"^[^+,-]*");
    
    private static System.Text.RegularExpressions.Regex regexAddPart = new System.Text.RegularExpressions.Regex(@"([+,-])([^+,-]*)");
    
    private static System.Text.RegularExpressions.Regex regexDigitPart = new System.Text.RegularExpressions.Regex(@"\d*");
    
    #region Словарь со списком алиасов и соответствующих функций для вычисления даты отсчета.

    private static Dictionary<List<string>, Func<DateTime, DateTime>> templateBaseFunctions = new Dictionary<List<string>, Func<DateTime, DateTime>>() {
      {new List<string>() {"today", "сегодня"}, (date) => Calendar.Today },
      {new List<string>() {"bw", "нн"}, (date) => Calendar.BeginningOfWeek(date) }
    };

    #endregion

    #region Словарь со списком алиасов и соответствующих функций для вычисления относительной даты.
    
    private static Dictionary<List<string>, Func<int, DateTime, DateTime>> templateAddFunctions = new Dictionary<List<string>, Func<int, DateTime, DateTime>>() {
      {new List<string>() {"d", "д"}, (digitPart, date) => date.AddDays(digitPart) },
      {new List<string>() {"w", "н"}, (digitPart, date) => date.AddDays(digitPart * 7) }
    };
    
    #endregion

    /// <summary>
    /// Вычислить дату из выражения.
    /// </summary>
    /// <param name="expression">Выражение.</param>
    /// <returns>Дата.</returns>
    public static DateTime? Calculate(string expression)
    {
      var resultDate = GetBaseDate(expression);
      
      var match = regexAddPart.Match(expression);
      var matchesCount = regexAddPart.Matches(expression).Count;

      for (int i = 1; i <= matchesCount; i++)
      {
        resultDate = RelativeDateCalculator.Calculator.AddPeriod(resultDate, match.Groups[2].Value, match.Groups[1].Value).Value;
        match = match.NextMatch();
      }
      
      return resultDate;
    }
    
    private static DateTime GetBaseDate(string expression)
    {
      var baseDate = Calendar.Now;
      var basePart = regexBasePart.Match(expression).Value;
      if (string.IsNullOrEmpty(basePart))
        return baseDate;
      
      basePart = basePart.ToLower();
      
      if (!templateBaseFunctions.Keys.Where(key => key.Contains(basePart)).Any())
        throw new Exception("Не удалось определить дату отсчета.");
      
      var templateFunction = templateBaseFunctions.FirstOrDefault(t => t.Key.Contains(basePart)).Value;
      
      return templateFunction(baseDate);
    }
    
    private static DateTime? AddPeriod(DateTime? baseDate, string periodExpression, string operation)
    {
      var resultDate = baseDate.HasValue ? baseDate.Value : Calendar.Now;
      
      if (string.IsNullOrEmpty(periodExpression))
        return resultDate;
      
      var relativeDate = RelativeDates.GetAllCached(r => r.Name == "[" + periodExpression + "]").FirstOrDefault();
      if (relativeDate != null)
        return Functions.RelativeDate.CalculateRelativeDate(baseDate, relativeDate.RelativeExpression);
      
      int digitPart = 0;
      if (int.TryParse(regexDigitPart.Match(periodExpression).Value, out digitPart) != true)
        digitPart = 1;
      
      var notDigitPart = periodExpression.Replace(digitPart.ToString(), string.Empty);
      
      if (operation == "-")
        digitPart = 0 - digitPart;
      
      if (!templateAddFunctions.Keys.Where(key => key.Contains(notDigitPart)).Any())
        throw new Exception("Не удалось вычислить выражение.");
      
      var templateFunction = templateAddFunctions.FirstOrDefault(t => t.Key.Contains(notDigitPart)).Value;
      
      return templateFunction(digitPart, resultDate);
    }
    
  }
}