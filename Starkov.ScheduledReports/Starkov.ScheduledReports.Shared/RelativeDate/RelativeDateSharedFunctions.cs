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
    
    //    public static string GetExpressionFromRelativeDates(List<IRelativeDate> relativeDates)
    //    {
    //      var result = string.Empty;
    //      foreach (var relativeDate in relativeDates)
    //      {
    //        result = GetExpressionFromRelativeDate(relativeDate,
    //      }
    //    }
    
    public virtual string GetExpressionFromRelativeDate(int? number)
    {
      if (number == null)
        number = 1;
      
      return string.Format("{0}*{1}", _obj.Id, number);
    }
    
    public virtual string GetUIExpressionFromRelativeDate(int? number)
    {
      if (number == 0 || !number.HasValue)
        number = 1;
      
      var operation = _obj.IsIncremental.GetValueOrDefault()
        ? number.GetValueOrDefault() < 0 ? "-" : "+"
        : "->";
      
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
      var last2Numbers= number.GetValueOrDefault() % 100;
      
      if (!string.IsNullOrEmpty(_obj.PluralName5) && (5 <= lastNumber && lastNumber <= 9 || 10 <= last2Numbers && (last2Numbers <= 19 || lastNumber == 0)))
        name = _obj.PluralName5;
      else if (!string.IsNullOrEmpty(_obj.PluralName2) && 2 <= lastNumber && lastNumber <= 4)
        name = _obj.PluralName2;
      
      return name;
    }
    
    /// <summary>
    /// Получить дату из строкового выражения.
    /// </summary>
    /// <param name="expression">Строка с выражением.</param>
    /// <returns>Дата.</returns>
    public static DateTime? GetDateFromExpression(string expression)
    {
      DateTime? result = null;
      
      var pattern = @"(\d*)\*(-*\d*)";
      var rg = new System.Text.RegularExpressions.Regex(pattern);
      
      foreach (var match in rg.Matches(expression))
      {
        int id = 0;
        int number = 1;
        int.TryParse(match?.ToString()?.Split('*')?[0], out id);
        int.TryParse(match?.ToString()?.Split('*')?[1], out number);
        
        var relativeDate = Functions.RelativeDate.Remote.GetRelativeDate(id, false);
        if (relativeDate != null)
          result = CalculateDate(relativeDate, result, number);
      }
      
      return result;
    }
    
    //    /// <summary>
    //    /// Получить дату из выражения для пользователя.
    //    /// </summary>
    //    /// <param name="expression">Строка с выражением.</param>
    //    /// <returns>Дата и отформатированное выражение.</returns>
    //    public static DateTime? GetDateFromUIExpression(string expression)
    //    {
    //      DateTime? result = null;
//
    //      var newExpression = string.Empty;
    //      var pattern = @"([+,-]|)([+d,-d]|)\[(.*?)\]";
    //      var rg = new System.Text.RegularExpressions.Regex(pattern);
//
    //      foreach (System.Text.RegularExpressions.Match match in rg.Matches(expression))
    //      {
    //        var operation = match?.Groups[1]?.ToString();
    //        var number = 1;
    //        if (!String.IsNullOrEmpty(match?.Groups[2]?.ToString()))
    //          int.TryParse(match?.Groups[2]?.ToString(), out number);
//
    //        if (operation == "-")
    //          number = 0 - number;
//
    //        var relativeDateName = match?.Groups[3]?.ToString();
    //        var relativeDate = PublicFunctions.RelativeDate.Remote.GetRelativeDate(relativeDateName, false);
//
    //        if (relativeDate == null)
    //          throw new Exception(string.Format("Не найдена относительная дата с именем «{0}»", relativeDateName));
//
    //        result = CalculateDate(relativeDate, result, number);
    //        newExpression += Functions.RelativeDate.GetUIExpressionFromRelativeDate(relativeDate, number);
    //      }
//
    //      return result;
    //    }
    
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
      
      if (!number.HasValue)
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