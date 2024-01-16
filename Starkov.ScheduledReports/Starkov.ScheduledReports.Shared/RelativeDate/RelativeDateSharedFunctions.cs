using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.RelativeDate;

namespace Starkov.ScheduledReports.Shared
{
  partial class RelativeDateFunctions
  {
    /// <summary>
    /// Признак что запись была инициализирована.
    /// </summary>
    [Public]
    public virtual bool IsInitialized()
    {
      return !string.IsNullOrEmpty(_obj.FunctionGuid);
    }
    
    /// <summary>
    /// Получить дату из строкового выражения.
    /// </summary>
    /// <param name="expression">Строка с выражением.</param>
    /// <returns>Дата и отформатированное выражение.</returns>
    [Public]
    public static System.Collections.Generic.KeyValuePair<DateTime?, string> GetDateFromUIExpression(string expression)
    {
      return GetDateFromUIExpression(expression, null);
    }
    
    /// <summary>
    /// Получить дату из строкового выражения.
    /// </summary>
    /// <param name="expression">Строка с выражением.</param>
    /// <param name="baseDate">Дата отсчета.</param>
    /// <returns>Дата и отформатированное выражение.</returns>
    [Public]
    public static System.Collections.Generic.KeyValuePair<DateTime?, string> GetDateFromUIExpression(string expression, DateTime? baseDate)
    {
      expression = expression.Trim().Replace("+ ", "+").Replace("- ", "-").Replace("> ", ">");
      var newExpression = string.Empty;
      var pattern = @"([+->][^\d]|^)(\d*|)(\[(.*?)\]|[^+->].[^+-]*|[0-2][0-9]:[0-5][0-9])";

      var resultDate = baseDate;
      var isLineBegin = true;
      foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(expression, pattern))
      {
        var operation = match?.Groups[1]?.ToString();
        var number = 1;
        if (!String.IsNullOrEmpty(match?.Groups[2]?.ToString()))
          int.TryParse(match?.Groups[2]?.ToString(), out number);
        else if (!String.IsNullOrEmpty(match?.Groups[1]?.ToString()))
          int.TryParse(match?.Groups[1]?.ToString(), out number);
        
        if (operation == "-")
          number = 0 - number;
        
        var relativeDateName = !String.IsNullOrEmpty(match?.Groups[4]?.ToString())
          ? match?.Groups[4]?.ToString().Trim()
          : match?.Groups[3]?.ToString().Trim();
        
        if (IsTime(relativeDateName))
        {
          var time = relativeDateName;
          if (time.Trim().Length < 5)
            time = time = "0" + time;

          int hour = 0;
          int minutes = 0;

          int.TryParse(time.Substring(0, 2), out hour);
          int.TryParse(time.Substring(3, 2), out minutes);
          
          var timeSpan = new TimeSpan(hour, minutes, 0);
          resultDate = SetTime(resultDate, timeSpan);
          newExpression += string.Format("->[{0}]", time);
        }
        else
        {
          var relativeDate = PublicFunctions.RelativeDate.Remote.GetRelativeDate(relativeDateName, false);
          
          if (relativeDate == null)
            throw new Exception(string.Format("Не найдена относительная дата «{0}»", relativeDateName));

          resultDate = Functions.RelativeDate.CalculateDate(relativeDate, resultDate, number);
          newExpression += Functions.RelativeDate.GetUIExpressionFromRelativeDate(relativeDate, number, isLineBegin);
        }
        
        isLineBegin = false;
      }
      
      return new System.Collections.Generic.KeyValuePair<DateTime?, string> (resultDate, newExpression);
    }
    
    /// <summary>
    /// Проверка что в строке передано время в формате чч:мм.
    /// </summary>
    /// <param name="text">Строка.</param>
    /// <returns>true при соответствии формату.</returns>
    private static bool IsTime(string text)
    {
      var rgTime = new System.Text.RegularExpressions.Regex(@"^([0-1]?[0-9]|2[0-3]):([0-5][0-9])$");
      return rgTime.IsMatch(text);
    }
    
    /// <summary>
    /// Установить время.
    /// </summary>
    /// <param name="date">Дата для вычислений.</param>
    /// <param name="timespan">Время в формате TimeSpan.</param>
    private static DateTime SetTime(DateTime? date, System.TimeSpan timespan)
    {
      var resultDate = date.HasValue ? date.Value : Calendar.Now;
      return resultDate.Date.Add(timespan);
    }
    
    /// <summary>
    /// Генерация строкового выражения для относительной даты.
    /// </summary>
    /// <param name="number">Множитель.</param>
    /// <param name="isLineBegin">Признак начала строки.</param>
    /// <returns>Строковое выражение.</returns>
    public virtual string GetUIExpressionFromRelativeDate(int? number, bool isLineBegin)
    {
      if (number == 0 || !number.HasValue)
        number = 1;
      
      var operation = _obj.IsIncremental.GetValueOrDefault()
        ? number.GetValueOrDefault() < 0 ? "-" : "+"
        : isLineBegin ? string.Empty : "->";
      
      if (isLineBegin && operation != "-")
        operation = string.Empty;
      
      if (number == 1 || number == -1 || !_obj.IsIncremental.GetValueOrDefault())
        number = null;
      else if (number.GetValueOrDefault() < 0)
        number = Math.Abs(number.Value);
      
      return string.Format("{0}{1}[{2}]", operation, number, GetFormatedNameByNumber(number));
    }
    
    /// <summary>
    /// Получить имя относительной даты во множественном числе.
    /// </summary>
    /// <returns>Имя относительной даты во множественном числе.</returns>
    private string GetFormatedNameByNumber(int? number)
    {
      var name = _obj.Name;
      var lastNumber = number.GetValueOrDefault() % 10;
      
      if (!string.IsNullOrEmpty(_obj.PluralName5) && (5 <= lastNumber && lastNumber <= 9 || 10 <= number && lastNumber == 0))
        name = _obj.PluralName5;
      else if (!string.IsNullOrEmpty(_obj.PluralName2) && 2 <= lastNumber && lastNumber <= 4)
        name = _obj.PluralName2;
      
      return name;
    }
    
    /// <summary>
    /// Вычислить дату.
    /// </summary>
    [Public]
    public virtual DateTime CalculateDate()
    {
      return CalculateDate(_obj, Calendar.Now, null);
    }
    
    /// <summary>
    /// Вычислить дату.
    /// </summary>
    /// <param name="date">Дата для вычислений.</param>
    [Public]
    public virtual DateTime CalculateDate(DateTime? date)
    {
      return CalculateDate(_obj, date, null);
    }
    
    /// <summary>
    /// Вычислить дату.
    /// </summary>
    /// <param name="date">Дата для вычислений.</param>
    /// <param name="number">Число для вычислений.</param>
    [Public]
    public virtual DateTime CalculateDate(DateTime? date, int? number)
    {
      if (_obj.IsIncremental != true && number.HasValue)
        number = null;
      
      return CalculateDate(_obj, date, number);
    }
    
    /// <summary>
    /// Вычислить дату.
    /// </summary>
    /// <param name="relative">Экземпляр справочника "Относительная дата".</param>
    /// <param name="date">Дата для вычислений.</param>
    /// <param name="number">Число для вычислений.</param>
    private static DateTime CalculateDate(IRelativeDate relative, DateTime? date, int? number)
    {
      var resultDate = date.HasValue ? date.Value : Calendar.Now;
      if (relative == null)
        return resultDate;
      
      if (!number.HasValue || number == 0)
        number = 1;
      
      if (Functions.RelativeDate.IsInitialized(relative))
        return CalculateDateByFunctionGuid(Guid.Parse(relative.FunctionGuid), date, number.Value);
      
      // Защитой от переполнения стека служит фильтрация для ExpressionPart в коллекции CompoundExpression
      foreach (var expression in relative.CompoundExpression.Where(c => c.ExpressionPart != null).OrderBy(c => c.OrderCalculation))
        resultDate = CalculateDate(expression.ExpressionPart, resultDate, expression.Number.GetValueOrDefault(1) * number);
      
      return resultDate;
    }

    private static DateTime CalculateDateByFunctionGuid(Guid functionGuid, DateTime? date, int number)
    {
      var resultDate = date.HasValue ? date.Value : Calendar.Now;
      
      try
      {
        if (functionGuid == Constants.RelativeDate.FunctionGuids.Base.Today)
          resultDate = Calendar.Today;
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Base.Now)
          resultDate = Calendar.Now;
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Base.BeginningOfWeek)
          resultDate = Calendar.BeginningOfWeek(resultDate);
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Base.EndOfWeek)
          resultDate = Calendar.EndOfWeek(resultDate);
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Base.BeginningOfMonth)
          resultDate = Calendar.BeginningOfMonth(resultDate);
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Base.EndOfMonth)
          resultDate = Calendar.EndOfMonth(resultDate);
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Base.BeginningOfYear)
          resultDate = Calendar.BeginningOfYear(resultDate);
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Base.EndOfYear)
          resultDate = Calendar.EndOfYear(resultDate);
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Base.Date)
          resultDate = resultDate.Date;
        
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Incremental.AddDays)
          resultDate = resultDate.AddDays(number);
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Incremental.AddMonths)
          resultDate = resultDate.AddMonths(number);
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Incremental.AddHours)
          resultDate = resultDate.AddHours(number);
        else if (functionGuid == Constants.RelativeDate.FunctionGuids.Incremental.AddMinutes)
          resultDate = resultDate.AddMinutes(number);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("CalculateDateByFunctionGuid. functionGuid={0}, date={1}, number={2}", ex, functionGuid, date, number);
        throw new Exception(Starkov.ScheduledReports.RelativeDates.Resources.RelativeCalculatedError);
      }
      
      return resultDate;
    }

  }
}