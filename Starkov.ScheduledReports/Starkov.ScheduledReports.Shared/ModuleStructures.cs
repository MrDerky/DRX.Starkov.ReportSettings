using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Starkov.ScheduledReports.Structures.Module
{

  /// <summary>
  /// Информация о параметре отчета.
  /// </summary>
  [Public(Isolated=true)]
  partial class ReportParameterInfo
  {
    public string ParameterName { get; set; }
    public string DisplayName { get; set; }
    public string DisplayValue { get; set; }
    public string InternalDataTypeName { get; set; }
    public bool IsRelatedDate { get; set; }
    public bool IsEntity { get; set; }
    public string EntityGuid { get; set; }
    public bool IsCollection { get; set; }
    public List<long> EntityIds { get; set; }
  }

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