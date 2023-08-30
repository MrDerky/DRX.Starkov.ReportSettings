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
    public static DateTime? CalculateRelativeDate(DateTime? baseDate, string relativeExpression)
    {
      var date = baseDate;
      if (date == null || string.IsNullOrEmpty(relativeExpression))
        return date;
      
      
    }

    /// <summary>
    /// Получить текст подсказки для относительного выражения.
    /// </summary>       
    [Remote]
    public StateView GetRelativeDateState()
    {
      var stateView = StateView.Create();
      
      var block = stateView.AddBlock();
      block.AddLabel = "Пример полученного результата" + CalculateRelativeDate(Calendar.Now, _obj.RelativeExpression);
      
      return stateView;
    }

  }
}