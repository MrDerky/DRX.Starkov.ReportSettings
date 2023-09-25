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
      base.CreatingFrom(e);
      
      e.Without(_info.Properties.Author);
      e.Without(_info.Properties.Status);
    }
  }

  partial class ScheduleSettingServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      
      _obj.Author = Users.Current;
      _obj.Status = Status.Closed;
      _obj.IsAsyncExecute = Functions.Module.GetNextJobExecuteTime(Constants.Module.SendSheduleReportsJobId) == null;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      if (_obj.ReportSetting == null)
      {
        e.AddError(Starkov.ScheduledReports.ScheduleSettings.Resources.NeedSelectReportError);
        return;
      }
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