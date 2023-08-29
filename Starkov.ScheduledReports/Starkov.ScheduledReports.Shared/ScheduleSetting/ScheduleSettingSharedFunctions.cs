using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports.Shared
{
  partial class ScheduleSettingFunctions
  {

    /// <summary>
    /// Заполнить имя.
    /// </summary>       
    public virtual void FillName()
    {
      _obj.Name = _obj.ReportName;
    }

  }
}