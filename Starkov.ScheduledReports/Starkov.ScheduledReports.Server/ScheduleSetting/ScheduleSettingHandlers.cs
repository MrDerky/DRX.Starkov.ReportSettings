using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports
{
  partial class ScheduleSettingCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      e.Without(_info.Properties.Author);
    }
  }

  partial class ScheduleSettingServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Author = Users.Current;
    }
  }

  partial class ScheduleSettingPeriodPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> PeriodFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(q => q.Status != RelativeDate.Status.Closed)
        .Where(q => q.IsIncremental.GetValueOrDefault());
    }
  }

}