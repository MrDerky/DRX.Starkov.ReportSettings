using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.SettingBase;

namespace Starkov.ScheduledReports
{
  partial class SettingBaseServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      if (string.IsNullOrEmpty(_obj.ReportGuid))
      {
        e.AddError(Starkov.ScheduledReports.SettingBases.Resources.NeedSelectReportError);
        return;
      }
    }
  }

}