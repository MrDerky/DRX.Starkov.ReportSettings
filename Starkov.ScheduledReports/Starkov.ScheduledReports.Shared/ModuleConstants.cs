using System;
using Sungero.Core;

namespace Starkov.ScheduledReports.Constants
{
  public static class Module
  {
    [Public]
    public static readonly Guid RelativeDatesManagerRole = Guid.Parse("F4EA8EF4-A03E-489F-BBF5-9691405EFFBC");
    
    [Public]
    public static readonly Guid ScheduleSettingManagerRole = Guid.Parse("349B25C0-EA2E-464A-A10A-BBD6FBF45761");
    
    // TODO доделать список или удалить если будет не нужен
    /// <summary>
    /// Известные типы объектов.
    /// </summary>
    [Public]
    public static class KnownType
    {
      public const string String = "System.String";
      
      public const string Int = "System.Int32";
      
      public const string Boolean = "System.Boolean";
      
      public const string DateTime = "System.DateTime";
    }

    /*
    [DataContract]
    [KnownType(typeof (EntityParameter))]
    [KnownType(typeof (ReportExportFormat))]
    [KnownType(typeof (List<EntityParameter>))]
    [KnownType(typeof (List<double?>))]
    [KnownType(typeof (List<Decimal?>))]
    [KnownType(typeof (List<float?>))]
    [KnownType(typeof (List<sbyte?>))]
    [KnownType(typeof (List<TimeSpan?>))]
    [KnownType(typeof (List<byte?>))]

     */
  }
}