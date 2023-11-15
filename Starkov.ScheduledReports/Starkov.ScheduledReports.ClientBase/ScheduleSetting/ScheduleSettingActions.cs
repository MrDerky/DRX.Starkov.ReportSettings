using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports.Client
{
  partial class ScheduleSettingActions
  {
    public virtual void TestRelative(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var dialog = Dialogs.CreateInputDialog("Тест относительных дат");
      var result = dialog.AddDate("Результат", false);
      var expression = dialog.AddString("Выражение", false);
      var expressionUI = dialog.AddString("Выражение", false);
      expression.IsEnabled = false;
      
      var relativeDate = dialog.AddSelect("Относительная дата", false, RelativeDates.Null);
      var number = dialog.AddInteger("Количество", false);
      var addRelative = dialog.AddHyperlink("Добавить");
      var clear = dialog.AddHyperlink("Очистить");
      
      addRelative.SetOnExecute(
        ()=>
        {
          if (relativeDate.Value == null)
            return;
          
          expression.Value += Functions.RelativeDate.GetExpressionFromRelativeDate(relativeDate.Value, number.Value.GetValueOrDefault()) + ";";
          result.Value = Functions.RelativeDate.GetDateFromExpression(expression.Value);
          
          expressionUI.Value = Functions.RelativeDate.GetDateFromExpression(expression.Value).ToString();
          relativeDate.Value = null;
          number.Value = null;
        });
      
      clear.SetOnExecute(
        ()=>
        {
          expression.Value = string.Empty;
          result.Value = Functions.RelativeDate.GetDateFromExpression(expression.Value);
        });
      
      dialog.Show();
    }

    public virtual bool CanTestRelative(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }


    public virtual void DisableSchedule(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      PublicFunctions.Module.CloseScheduleLog(_obj);
      
      _obj.Status = Status.Closed;
      _obj.Save();
      
      Functions.ScheduleSetting.SetPropertyStates(_obj);
      _obj.State.Controls.ScheduleReportState.Refresh();
    }

    public virtual bool CanDisableSchedule(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Status.Active;
    }

    public virtual void EnableSchedule(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.DateEnd.HasValue && _obj.DateEnd.Value <= Calendar.Now)
      {
        e.AddError("Завершение расписания должно быть больше текущего времени");
        return;
      }
      
      var nextDate = Functions.ScheduleSetting.Remote.GetNextPeriod(_obj);
      if (nextDate <= Calendar.Now)
      {
        e.AddError("Следующий запуск не может быть меньше текущего времени.");
        return;
      }
      
      if (_obj.DateEnd.HasValue && nextDate >= _obj.DateEnd.Value)
      {
        e.AddError(string.Format("Следующий запуск {0} не может быть позже даты завершения расписания.", nextDate.Value.ToString()));
        return;
      }
      
      try
      {
        PublicFunctions.Module.EnableSchedule(_obj, null);
        var message = _obj.IsAsyncExecute.GetValueOrDefault()
          ? string.Format("Запланирована отправка отчета по расписанию.{0}Время следующего запуска {1}", Environment.NewLine, nextDate)
          : string.Format("Запланирована отправка отчета по расписанию.{0}Время запуска зависит от настроек фонового процесса", Environment.NewLine);
        Dialogs.NotifyMessage(message);
      }
      catch (Exception ex)
      {
        // TODO Доработать обработку ошибок в EnableSchedule
        e.AddError(ex.Message);
        return;
      }
      
      _obj.Status = Status.Active;
      _obj.Save();
      
      Functions.ScheduleSetting.SetPropertyStates(_obj);
      _obj.State.Controls.ScheduleReportState.Refresh();
    }

    public virtual bool CanEnableSchedule(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Status.Closed;
    }

  }

}