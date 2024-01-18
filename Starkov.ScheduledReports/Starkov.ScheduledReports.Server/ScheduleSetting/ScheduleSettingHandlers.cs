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
  }

}