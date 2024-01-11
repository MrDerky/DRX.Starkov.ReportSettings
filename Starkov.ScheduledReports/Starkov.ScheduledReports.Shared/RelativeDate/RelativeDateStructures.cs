using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ScheduledReports.Structures.RelativeDate
{

  /// <summary>
  /// Структура для хранения параметров относительной даты.
  /// </summary>
  [Public]
  partial class RelatedDateInfo
  {
    /// <summary>
    /// Признак, что дата относительная.
    /// </summary>
    public bool IsRelated { get; set; }
    
    /// <summary>
    /// Выражение относительной даты.
    /// </summary>
    public string Expression { get; set; }
    
    /// <summary>
    /// Абсолютная дата.
    /// </summary>
    public DateTime? Date { get; set; }
  }

}