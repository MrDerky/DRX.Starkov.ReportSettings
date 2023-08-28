using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ScheduledReports.Structures.Module
{

  /// <summary>
  /// Информация по отчету.
  /// </summary>
  [Public]
  partial class ReportInfo
  {
    /// <summary>
    /// Ид.
    /// </summary>
    public Guid NameGuid { get; set; }
    
    /// <summary>
    /// Локализованое имя.
    /// </summary>
    public string LocalizedName { get; set; }
    
    /// <summary>
    /// Имя.
    /// </summary>
    public string Name { get; set; }
  }

}