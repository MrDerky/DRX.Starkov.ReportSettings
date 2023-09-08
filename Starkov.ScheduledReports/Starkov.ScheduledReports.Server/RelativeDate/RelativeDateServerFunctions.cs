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

    
    //    /// <summary>
    //    /// Вычислить относительную дату.
    //    /// </summary>
    //    [Public, Remote]
    //    public static DateTime? CalculateRelativeDate(DateTime? baseDate, string relativeExpression)
    //    {
    //      var date = baseDate;
    //      if (date == null || string.IsNullOrEmpty(relativeExpression))
    //        return date;
//
    //      return null;
    //      // var expr = Convert.ToDouble(
    //    }
    
    //    /// <summary>
    //    /// Вычислить относительную дату.
    //    /// </summary>
    //    [Public, Remote]
    //    public static string CalculateExpression(string relativeExpression)
    //    {
    //      //      var result = Convert.ToDouble(new System.Data.DataTable().Compute(relativeExpression, null));
    //      //      var result = new System.Data.DataTable().Compute(relativeExpression, null);
    ////      var result = RelativeDateCalculator.Calculator.Calculate(relativeExpression);
//
//
    //      return result.ToString();
    //    }
    
    //    /// <summary>
    //    /// Получить текст подсказки для относительного выражения.
    //    /// </summary>
    //    [Remote]
    //    public StateView GetRelativeDateState()
    //    {
    //      var stateView = StateView.Create();
//
    //      var labelStyle = StateBlockLabelStyle.Create();
    //      labelStyle.FontWeight = FontWeight.Bold;
//
    //      var block = stateView.AddBlock();
//
    //      //      var validValuesForBaseExpressions = Starkov.ScheduledReports.RelativeDateCalculator.Calculator.GetValidValuesForBaseExpressions();
//
    //      var dateOperations = RelativeDateCalculator.Calculator.GetDateOperations();
    //      foreach (var operation in dateOperations)
    //      {
    //        block.AddLabel(operation.Name, labelStyle, false);
    //        block.AddLabel(" - " + operation.Description);
    //        block.AddLineBreak();
    //      }
    //      //      try
    //      //      {
    //      //        block.AddLabel("Пример полученного результата: " + CalculateExpression(_obj.RelativeExpression));
    //      //      }
    //      //      catch (Exception ex)
    //      //      {
    //      //        block.AddLabel("Ошибка при обработке: " + ex.Message);
    //      //        block.Background = Colors.Common.Red;
    //      //      }
//
    //      return stateView;
    //    }
    
  }
}

//namespace Starkov.ScheduledReports.RelativeDateCalculator
//{
//
//  public static class Calculator
//  {
//
//    /// <summary>
//    /// Класс, описывающий операцию с датой.
//    /// </summary>
//    public class DateOperation
//    {
//      /// <summary>
//      /// Обозначение.
//      /// </summary>
//      public string Name { get; }
//
//      /// <summary>
//      /// Описание
//      /// </summary>
//      public string Description { get; }
//
//      /// <summary>
//      /// Признак что операция предназначена для вычисления даты отсчета.
//      /// </summary>
//      public bool BasePartOperation { get; }
//
//      private System.Func<DateTime, DateTime> BaseFunction { get; }
//
//      private System.Func<int, DateTime, DateTime> AddFunction { get; }
//
//      public DateOperation(string name, string description, Func<DateTime, DateTime> function)
//      {
//        Name = name;
//        Description = description;
//        BaseFunction = function;
//        BasePartOperation = true;
//      }
//
//      public DateOperation(string name, string description, Func<int, DateTime, DateTime> function)
//      {
//        Name = name;
//        Description = description;
//        AddFunction = function;
//        BasePartOperation = false;
//      }
//
//      /// <summary>
//      /// Вычислить дату отсчета.
//      /// </summary>
//      /// <param name="date">Дата для расчета.</param>
//      /// <returns>Дата отсчета.</returns>
//      public DateTime Execute(DateTime date)
//      {
//        return BaseFunction(date);
//      }
//
//      /// <summary>
//      /// Вычислить относительную дату.
//      /// </summary>
//      /// <param name="digit">Значение.</param>
//      /// <param name="date">Дата отсчета.</param>
//      /// <returns>Относительная дата.</returns>
//      public DateTime Execute(int digit, DateTime date)
//      {
//        return AddFunction(digit, date);
//      }
//    }
//
//    private static System.Text.RegularExpressions.Regex regexBasePart = new System.Text.RegularExpressions.Regex(@"^[^+,-]*");
//
//    private static System.Text.RegularExpressions.Regex regexAddPart = new System.Text.RegularExpressions.Regex(@"([+,-])([^+,-]*)");
//
//    private static System.Text.RegularExpressions.Regex regexDigitPart = new System.Text.RegularExpressions.Regex(@"\d*");
//
//    private static System.Text.RegularExpressions.Regex regexBracketPart = new System.Text.RegularExpressions.Regex(@"([^+,-]*)(\([^)]+\))");
//
//    /// <summary>
//    /// Получить список доступных операций с датами.
//    /// </summary>
//    public static List<RelativeDateCalculator.Calculator.DateOperation> GetDateOperations()
//    {
//      return new List<DateOperation>() {
//
//        #region Операции вычисления основной даты.
//
//        new DateOperation("сейчас", "Текущая дата и время (подставляется автоматически, если ничего не указано)", (date) => Calendar.Now),
//        //new DateOperation("сегодня", "Текущая дата без времени", (date) => Calendar.Today),
//        new DateOperation("нн", "Начало недели", (date) => Calendar.BeginningOfWeek(date)),
//        new DateOperation("кн", "Конец недели", (date) => Calendar.EndOfWeek(date)),
//        new DateOperation("нм", "Начало месяца", (date) => Calendar.BeginningOfMonth(date)),
//        new DateOperation("км", "Конец месяца", (date) => Calendar.EndOfMonth(date)),
//        new DateOperation("св", "Получение даты без времени", (date) => date.Date),
//
//        #endregion
//
//        #region Операции вычисления добавочной даты.
//
//        new DateOperation("д", "Дней", (digitPart, date) => date.AddDays(digitPart)),
//        new DateOperation("н", "Недель", (digitPart, date) => date.AddDays(digitPart * 7)),
//        new DateOperation("м", "Месяцев", (digitPart, date) => date.AddMonths(digitPart)),
//        new DateOperation("чч", "Часов", (digitPart, date) => date.AddHours(digitPart)),
//        new DateOperation("мм", "Минут", (digitPart, date) => date.AddMinutes(digitPart))
//
//          #endregion
//
//      };
//    }
//
//    // TODO Отрефакторить и оттестировать
//    /// <summary>
//    /// Вычислить дату из выражения.
//    /// </summary>
//    /// <param name="expression">Выражение.</param>
//    /// <returns>Дата.</returns>
//    public static DateTime Calculate(string expression)
//    {
//      // Проверка на выражение в скобках.
//      var match = regexBracketPart.Match(expression);
//      if (match.Groups.Count == 3)
//      {
//        var basePart = match.Groups[1].Value;
//        var addPart = match.Groups[2].Value.Substring(1, match.Groups[2].Value.Length - 2);
//        return GetBaseDate(basePart, Calculate(addPart));
//      }
//      else
//        return CalculatePart(expression);
//    }
//
//    private static DateTime CalculatePart(string expression)
//    {
//      var dateOperation = GetDateOperations();
//      var resultDate = GetBaseDate(expression, null);
//
//      var match = regexAddPart.Match(expression);
//      var matchesCount = regexAddPart.Matches(expression).Count;
//
//      for (int i = 1; i <= matchesCount; i++)
//      {
//        resultDate = RelativeDateCalculator.Calculator.AddPeriod(resultDate, match.Groups[2].Value, match.Groups[1].Value).Value;
//        match = match.NextMatch();
//      }
//
//      return resultDate;
//    }
//
//    private static DateTime GetBaseDate(string expression, DateTime? baseDate)
//    {
//      if (!baseDate.HasValue)
//        baseDate = Calendar.Now;
//
//      var basePart = regexBasePart.Match(expression).Value;
//      if (string.IsNullOrEmpty(basePart))
//        return baseDate.Value;
//
//      basePart = basePart.ToLower();
//
//      var dateOperation = GetDateOperations().FirstOrDefault(o => o.Name == basePart);
//      if (dateOperation == null)
//        //      if (!templateBaseFunctions.Keys.Where(key => key.ContainsKey(basePart)).Any())
//        throw new Exception("Не удалось определить дату отсчета.");
//
//      //      var templateFunction = dateOperation.Function templateBaseFunctions.FirstOrDefault(t => t.Key.ContainsKey(basePart)).Value;
//
//      return dateOperation.Execute(baseDate.Value);
//    }
//
//    private static DateTime? AddPeriod(DateTime? baseDate, string periodExpression, string operation)
//    {
//      var resultDate = baseDate.HasValue ? baseDate.Value : Calendar.Now;
//
//      if (string.IsNullOrEmpty(periodExpression))
//        return resultDate;
//
//      var relativeDate = RelativeDates.GetAllCached(r => r.Name == "[" + periodExpression + "]").FirstOrDefault();
//      if (relativeDate != null)
//        return Functions.RelativeDate.CalculateRelativeDate(baseDate, relativeDate.RelativeExpression);
//
//      int digitPart = 0;
//      if (int.TryParse(regexDigitPart.Match(periodExpression).Value, out digitPart) != true)
//        digitPart = 1;
//
//      var notDigitPart = periodExpression.Replace(digitPart.ToString(), string.Empty);
//
//      if (operation == "-")
//        digitPart = 0 - digitPart;
//
//      var dateOperation = GetDateOperations().FirstOrDefault(o => o.Name == notDigitPart);
//      if (dateOperation == null)
//        //      if (!templateAddFunctions.Keys.Where(key => key.Contains(notDigitPart)).Any())
//        throw new Exception("Не удалось вычислить выражение.");
//
//      //      var templateFunction = templateAddFunctions.FirstOrDefault(t => t.Key.Contains(notDigitPart)).Value;
//
//      return dateOperation.Execute(digitPart, resultDate); //templateFunction(digitPart, resultDate);
//    }
//
//  }
//}