﻿using System;
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
    /// <param name="relative">Экземпляр справочника "Относительная дата".</param>
    /// <param name="date">Дата для вычислений.</param>
    /// <param name="number">Число для вычислений.</param>
    private static DateTime CalculateDate(IRelativeDate relative, DateTime? date, int? number)
    {
      var resultDate = date.HasValue ? date.Value : Calendar.Now;
      if (relative == null)
        return resultDate;
      
      if (!string.IsNullOrEmpty(relative.FunctionGuid))
        return CalculateDateByFunctionGuid(Guid.Parse(relative.FunctionGuid), date, number.GetValueOrDefault(1));
      
      foreach (var expression in relative.CompoundExpression.OrderBy(c => c.Id))
        resultDate = CalculateDate(expression.ExpressionPart, resultDate, expression.Number);
      
      return resultDate;
    }

    private static DateTime CalculateDateByFunctionGuid(Guid functionGuid, DateTime? date, int number)
    {
      var resultDate = date.HasValue ? date.Value : Calendar.Now;
      
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
      
      return resultDate;
    }
  }
}