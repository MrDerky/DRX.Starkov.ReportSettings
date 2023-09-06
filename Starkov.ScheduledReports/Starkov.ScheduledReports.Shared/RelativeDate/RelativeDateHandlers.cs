using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.RelativeDate;

namespace Starkov.ScheduledReports
{
  partial class RelativeDateCompoundExpressionSharedCollectionHandlers
  {

    public virtual void CompoundExpressionDeleted(Sungero.Domain.Shared.CollectionPropertyDeletedEventArgs e)
    {
      var order = 1;
      foreach (var expression in _obj.CompoundExpression.OrderBy(c => c.OrderCalculation))
        expression.OrderCalculation = order++;
    }

    public virtual void CompoundExpressionAdded(Sungero.Domain.Shared.CollectionPropertyAddedEventArgs e)
    {
      var maxOrder = _obj.CompoundExpression.Max(c => c.OrderCalculation);
      _added.OrderCalculation = maxOrder > 0 ? ++maxOrder : 1;
    }
  }

  partial class RelativeDateSharedHandlers
  {

  }
}