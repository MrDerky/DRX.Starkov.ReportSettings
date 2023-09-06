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
    public virtual void Action(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var result = Functions.RelativeDate.CalculateDate(_obj);
      
      Dialogs.NotifyMessage(result.ToString());
    }

    public virtual bool CanAction(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public virtual void Test(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        var result = Functions.RelativeDate.Remote.CalculateExpression(_obj.RelativeExpression);
        Dialogs.NotifyMessage(result);
      }
      catch (Exception ex)
      {
        e.AddError(ex.Message);
      }
      
    }

    public virtual bool CanTest(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}