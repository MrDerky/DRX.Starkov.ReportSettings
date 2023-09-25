using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.SettingBase;

namespace Starkov.ScheduledReports
{
  partial class SettingBaseSharedHandlers
  {

    public virtual void ReportNameChanged(Sungero.Domain.Shared.StringPropertyChangedEventArgs e)
    {
      Functions.SettingBase.FillName(_obj);
    }

  }
}