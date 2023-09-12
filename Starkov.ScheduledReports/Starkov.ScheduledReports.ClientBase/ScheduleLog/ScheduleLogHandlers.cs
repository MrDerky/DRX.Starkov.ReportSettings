using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleLog;

namespace Starkov.ScheduledReports
{
  partial class ScheduleLogClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      _obj.State.Pages.Preview.Activate();
    }

  }
}