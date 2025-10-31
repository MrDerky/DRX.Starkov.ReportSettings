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
    /// Получить запись "Относительная дата" по ИД.
    /// </summary>
    /// <param name="id">ИД.</param>
    /// <param name="isActiveOnly">Только активная запись.</param>
    /// <returns>Относительная дата.</returns>
    [Public, Remote(IsPure = true)]
    public static IRelativeDate GetRelativeDate(long id, bool isActiveOnly)
    {
      return RelativeDates.GetAllCached(r => r.Id == id)
        .FirstOrDefault(r => !isActiveOnly || r.Status != Status.Closed);
    }

    /// <summary>
    /// Получить запись "Относительная дата" по Имени.
    /// </summary>
    /// <param name="name">Имя.</param>
    /// <param name="isActiveOnly">Только активная запись.</param>
    /// <returns>Относительная дата.</returns>
    [Public, Remote(IsPure = true)]
    public static IRelativeDate GetRelativeDate(string name, bool isActiveOnly)
    {
      name = name.ToLower().Trim().Replace(" ", "");
      
      return RelativeDates.GetAllCached(r => r.Name.ToLower().Trim().Replace(" ", "") == name ||
                                        r.PluralName2.ToLower().Trim().Replace(" ", "") == name ||
                                        r.PluralName5.ToLower().Trim().Replace(" ", "") == name)
        .FirstOrDefault(r => !isActiveOnly || r.Status != Status.Closed);
    }
  }
}