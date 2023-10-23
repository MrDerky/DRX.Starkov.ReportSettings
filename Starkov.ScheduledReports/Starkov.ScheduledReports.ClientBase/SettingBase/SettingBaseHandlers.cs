using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.SettingBase;

namespace Starkov.ScheduledReports
{
  partial class SettingBaseClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ReportGuid))
        e.Instruction = Starkov.ScheduledReports.SettingBases.Resources.SheduleSettingInstruction;
    }

  }
}