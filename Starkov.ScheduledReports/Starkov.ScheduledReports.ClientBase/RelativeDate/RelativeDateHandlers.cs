using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.RelativeDate;

namespace Starkov.ScheduledReports
{
  partial class RelativeDateClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      var canChange = string.IsNullOrEmpty(_obj.FunctionGuid);
      _obj.State.Properties.CompoundExpression.IsVisible = canChange;
      _obj.State.IsEnabled = canChange;
    }
  }

}