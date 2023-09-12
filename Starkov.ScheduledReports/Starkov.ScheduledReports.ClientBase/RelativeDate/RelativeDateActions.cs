using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.RelativeDate;

namespace Starkov.ScheduledReports.Client
{
  partial class RelativeDateActions
  {
    public virtual void Calculate(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.State.IsChanged)
        _obj.Save();
      
      var result = Functions.RelativeDate.CalculateDate(_obj);
      
      Dialogs.NotifyMessage(result.ToString());
    }

    public virtual bool CanCalculate(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}