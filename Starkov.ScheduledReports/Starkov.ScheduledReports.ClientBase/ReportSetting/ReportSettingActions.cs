﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ReportSetting;

namespace Starkov.ScheduledReports.Client
{
  partial class ReportSettingActions
  {
    public virtual void SetReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.ReportSetting.SelectReport(_obj);
    }

    public virtual bool CanSetReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }
  }

}