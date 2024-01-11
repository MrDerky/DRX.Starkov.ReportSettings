using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports
{
  partial class ScheduleSettingClientHandlers
  {

    public virtual void PeriodExpressionValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
      if (e.NewValue == e.OldValue || string.IsNullOrEmpty(e.NewValue))
        return;
      
      KeyValuePair<DateTime?, string> dateAndExpression;
      try
      {
        dateAndExpression = PublicFunctions.RelativeDate.GetDateFromUIExpression(e.NewValue);
        if (!dateAndExpression.Key.HasValue)
          //              resultUI.Value = dateAndExpression.Key.Value.ToString();
//        else
          throw new Exception("Не удалось вычислить выражение");
      }
      catch (Exception ex)
      {
        e.AddWarning(ex.Message, e.Property);
        return;
      }
      
      if (!string.IsNullOrEmpty(dateAndExpression.Value) && e.NewValue != dateAndExpression.Value)
        e.NewValue = dateAndExpression.Value;
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      var scheduleSettingManagerRole = Roles.GetAll(r => r.Sid == Constants.Module.ScheduleSettingManagerRole).FirstOrDefault();
      if (scheduleSettingManagerRole == null || !Users.Current.IncludedIn(scheduleSettingManagerRole))
      {
        e.HideAction(_obj.Info.Actions.EnableSchedule);
        e.HideAction(_obj.Info.Actions.DisableSchedule);
      }
      
      //TODO Временный костыль, подумать на переход на e.Params
      if (_obj.Status == Status.Active && !Functions.Module.Remote.GetScheduleLogs(_obj)
          .Any(l => l.Status == ScheduleLog.Status.Waiting || l.Status == ScheduleLog.Status.Error))
      {
        _obj.Status = Status.Closed;
      }
      
      Functions.ScheduleSetting.SetPropertyStates(_obj);
    }

  }
}