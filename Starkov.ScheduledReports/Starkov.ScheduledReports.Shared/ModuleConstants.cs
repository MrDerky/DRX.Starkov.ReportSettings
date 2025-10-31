using System;
using Sungero.Core;

namespace Starkov.ScheduledReports.Constants
{
  public static class Module
  {
    
    // Guid справочника видов документов.
    public static readonly Guid DocumentKindTypeGuid = Guid.Parse("14a59623-89a2-4ea8-b6e9-2ad4365f358c");
    
    /// <summary>
    /// Идентификатор типа документа "Простой документ".
    /// </summary>
    [Public]
    public static readonly Guid SimpleDocumentTypeGuid = Guid.Parse("09584896-81e2-4c83-8f6c-70eb8321e1d0");
    
    /// <summary>
    /// Идентификатор вида документа "Отчет".
    /// </summary>
    [Public]
    public static readonly Guid ReportDocumentKindGuid = Guid.Parse("8BFF5B18-E565-49C7-AFFA-A1AD618F692C");
    
    /// <summary>
    /// Идентификатор фонового процесса "Отправка отчетов по расписанию".
    /// </summary>
    [Public]
    public static readonly Guid SendSheduleReportsJobId = Guid.Parse("6862cdbe-45d6-47c1-9e73-10687664a758");
    
    /// <summary>
    /// Guid роли Редакторы относительных дат.
    /// </summary>
    [Public]
    public static readonly Guid RelativeDatesManagerRole = Guid.Parse("F4EA8EF4-A03E-489F-BBF5-9691405EFFBC");
    
    /// <summary>
    /// Guid роли Пользователи с доступом к отчетам по расписанию.
    /// </summary>
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