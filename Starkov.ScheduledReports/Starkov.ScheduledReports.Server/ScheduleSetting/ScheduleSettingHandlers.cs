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
      e.Without(_info.Properties.Status);
    }
  }

  partial class ScheduleSettingServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ReportGuid))
      {
        e.AddError(Starkov.ScheduledReports.ScheduleSettings.Resources.NeedSelectReportError);
        return;
      }
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Author = Users.Current;
      _obj.Status = Status.Closed;
      _obj.ShowParams = false;
      _obj.IsAsyncExecute = Functions.Module.GetNextJobExecuteTime(Constants.Module.SendSheduleReportsJobId) == null;
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