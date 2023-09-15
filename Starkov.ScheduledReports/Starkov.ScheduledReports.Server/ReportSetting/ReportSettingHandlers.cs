using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ReportSetting;

namespace Starkov.ScheduledReports
{
  partial class ReportSettingCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      e.Without(_info.Properties.Status);
    }
  }

  partial class ReportSettingServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Status = Status.Closed;
    }
  }

}