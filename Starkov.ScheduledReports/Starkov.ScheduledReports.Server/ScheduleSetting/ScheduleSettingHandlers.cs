using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports
{
  partial class ScheduleSettingPeriodPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> PeriodFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(q => q.Status != RelativeDate.Status.Closed)
        .Where(q => q.IsIncremental.GetValueOrDefault());
    }
  }

}