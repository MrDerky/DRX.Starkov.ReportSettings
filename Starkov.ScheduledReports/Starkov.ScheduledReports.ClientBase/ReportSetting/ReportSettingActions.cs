using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ReportSetting;

namespace Starkov.ScheduledReports.Client
{
  partial class ReportSettingActions
  {
    public override void SetReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.ReportSetting.SelectReport(_obj);
    }

    public override bool CanSetReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSetReport(e);
    }

  }

}