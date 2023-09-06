using System;
using Sungero.Core;

namespace Starkov.ScheduledReports.Constants
{
  public static class RelativeDate
  {
    
    public static class FunctionGuids
    {
      public static class Base
      {
        /// <summary>
        /// Текущая дата без времени.
        /// </summary>
        [Public]
        public static readonly Guid Today = Guid.Parse("3FD2B769-CB23-4380-A908-D8D00A3B8F81");
        
        /// <summary>
        /// Сейчас.
        /// </summary>
        [Public]
        public static readonly Guid Now = Guid.Parse("54A4B7AA-B0AC-4DE9-AA78-A03C70A51FF4");
        
        /// <summary>
        /// Начало недели.
        /// </summary>
        [Public]
        public static readonly Guid BeginningOfWeek = Guid.Parse("C3D5CE3A-7E53-463A-B921-24E8B27E9D09");
        
        /// <summary>
        /// Конец недели.
        /// </summary>
        [Public]
        public static readonly Guid EndOfWeek = Guid.Parse("4B0267C8-E463-4E57-97A9-6FBD4D03C7DD");
        
        /// <summary>
        /// Начало месяца.
        /// </summary>
        [Public]
        public static readonly Guid BeginningOfMonth = Guid.Parse("8D064018-3F0B-402E-A593-B06FD70D8E04");
        
        /// <summary>
        /// Конец месяца.
        /// </summary>
        [Public]
        public static readonly Guid EndOfMonth = Guid.Parse("34F3BCB8-D9A9-4D5C-AE69-1A649F1B3D12");
        
        /// <summary>
        /// Начало года.
        /// </summary>
        [Public]
        public static readonly Guid BeginningOfYear = Guid.Parse("E0569893-FA34-42B9-AAFC-7821549C1980");
        
        /// <summary>
        /// Конец года.
        /// </summary>
        [Public]
        public static readonly Guid EndOfYear = Guid.Parse("C7BFD192-79CE-4601-8BB4-B2220D6628BF");
        
        /// <summary>
        /// Только дата.
        /// </summary>
        [Public]
        public static readonly Guid Date = Guid.Parse("7DC04436-CAFF-48CF-934A-5AD901870C1A");
      }
      
      public static class Incremental
      {
        /// <summary>
        /// Добавление дней.
        /// </summary>
        [Public]
        public static readonly Guid AddDays = Guid.Parse("EAE6D5CC-A5EC-47E6-A36A-F0FD6F8FB300");
        
        /// <summary>
        /// Добавление месяцев.
        /// </summary>
        [Public]
        public static readonly Guid AddMonths = Guid.Parse("A5E9EF2D-AF39-4982-8FB2-9B10D64D41A8");
        
        /// <summary>
        /// Добавление часов.
        /// </summary>
        [Public]
        public static readonly Guid AddHours = Guid.Parse("2CB762A8-03BA-4ACF-8DDD-6F5FF5AB1808");
        
        /// <summary>
        /// Добавление минут.
        /// </summary>
        [Public]
        public static readonly Guid AddMinutes = Guid.Parse("06EA3600-2D94-46A4-9F39-D2DC324ED222");

      }
    }
  }
}