using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.RelativeDate;

namespace Starkov.ScheduledReports
{
  partial class RelativeDateCompoundExpressionExpressionPartPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> CompoundExpressionExpressionPartFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query.Where(q => q.FunctionGuid.Length > 0);
    }
  }


}