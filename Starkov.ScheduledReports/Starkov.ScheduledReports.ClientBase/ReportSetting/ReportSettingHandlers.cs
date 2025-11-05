using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ReportSetting;

namespace Starkov.ScheduledReports
{
  partial class ReportSettingClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      
      if (Functions.SettingBase.IsFillReportParamsAny(_obj))
        e.AddInformation("Есть параметры с заполненными значениями. При создании настройки расписания они будут подтянуты как значения по-умолчанию.");
    }

  }
}