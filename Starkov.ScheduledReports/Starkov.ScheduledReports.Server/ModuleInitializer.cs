using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace Starkov.ScheduledReports.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateBaseRelativeDates();
    }
    
    /// <summary>
    /// Инициализация основных записей справочника "Относительная дата".
    /// </summary>
    public virtual void CreateBaseRelativeDates()
    {
      InitializationLogger.Debug("Init: Create base relative dates.");
      CreateBaseRelativeDate("Сегодня", "Текущая дата без времени", Constants.RelativeDate.FunctionGuids.Base.Today);
      CreateBaseRelativeDate("Сейчас", "Текущая дата и время", Constants.RelativeDate.FunctionGuids.Base.Now);
      CreateBaseRelativeDate("Начало недели", null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfWeek);
      CreateBaseRelativeDate("Конец недели", null, Constants.RelativeDate.FunctionGuids.Base.EndOfWeek);
      CreateBaseRelativeDate("Начало месяца", null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfMonth);
      CreateBaseRelativeDate("Конец месяца", null, Constants.RelativeDate.FunctionGuids.Base.EndOfMonth);
      CreateBaseRelativeDate("Начало года", null, Constants.RelativeDate.FunctionGuids.Base.BeginningOfYear);
      CreateBaseRelativeDate("Конец года", null, Constants.RelativeDate.FunctionGuids.Base.EndOfYear);
      CreateBaseRelativeDate("Сброс времени", "Вернуть дату и время 0:00:00", Constants.RelativeDate.FunctionGuids.Base.Date);
      
      CreateBaseRelativeDate("Через день", "Добавить 1 день", Constants.RelativeDate.FunctionGuids.Incremental.AddDays);
      CreateBaseRelativeDate("Через месяц", "Добавить 1 месяц", Constants.RelativeDate.FunctionGuids.Incremental.AddMonths);
      CreateBaseRelativeDate("Через час", "Добавить 1 час", Constants.RelativeDate.FunctionGuids.Incremental.AddHours);
      CreateBaseRelativeDate("Через минуту", "Добавить 1 минуту", Constants.RelativeDate.FunctionGuids.Incremental.AddMinutes);
    }
    
    private void CreateBaseRelativeDate(string name, string description, Guid guid)
    {
      var relativeDate = RelativeDates.GetAll(r => r.FunctionGuid == guid.ToString()).FirstOrDefault();
      if (relativeDate == null)
      {
        InitializationLogger.DebugFormat("Init: Create new base relative date \"{0}\".", name);
        relativeDate = RelativeDates.Create();
        relativeDate.FunctionGuid = guid.ToString();
      }
      
      relativeDate.Name = name;
      relativeDate.Description = description;
      relativeDate.Save();
    }
  }
}
