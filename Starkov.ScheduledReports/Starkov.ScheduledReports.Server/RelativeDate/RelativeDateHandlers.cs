using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.RelativeDate;

namespace Starkov.ScheduledReports
{

  partial class RelativeDateServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      var testCalculate = Functions.RelativeDate.CalculateDate(_obj);
//      if (_obj.IsIncremental == true && testCalculate == Functions.RelativeDate.CalculateDate(_obj, testCalculate))
//        e.AddError("Данный набор выражений не может принимать множитель"); // TODO локализация
      
      _obj.IsIncremental = testCalculate != Functions.RelativeDate.CalculateDate(_obj, testCalculate);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.IsIncremental = false;
    }
  }

  partial class RelativeDateCompoundExpressionExpressionPartPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> CompoundExpressionExpressionPartFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      return query
        .Where(q => q.Status != RelativeDate.Status.Closed)
        .Where(q => q.FunctionGuid.Length > 0);
    }
  }


}