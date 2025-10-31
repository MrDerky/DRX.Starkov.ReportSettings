using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;
using Sungero.Domain.Shared;

namespace Starkov.ScheduledReports.Server
{
  partial class ScheduleSettingFunctions
  {
    /// <summary>
    /// Создать запись настройки расписания для отчета.
    /// </summary>
    [Public, Remote]
    public static Starkov.ScheduledReports.IScheduleSetting CreateScheduleSetting()
    {
      return ScheduleSettings.Create();
    }
    
    /// <summary>
    /// Получить запись настройки расписания для отчета по ИД.
    /// </summary>
    [Public, Remote(IsPure = true)]
    public static Starkov.ScheduledReports.IScheduleSetting GetScheduleSetting(long? id)
    {
      return ScheduleSettings.GetAll(s => s.Id == id).FirstOrDefault(s => s.Status != ScheduledReports.ScheduleSetting.Status.Closed);
    }

    /// <summary>
    /// Получить состояние из журнала расписаний.
    /// </summary>
    [Remote]
    public StateView GetScheduleSettingState()
    {
      return Functions.Module.GetScheduleState(_obj);
    }
    
    /// <summary>
    /// Получить следующую дату выполнения.
    /// </summary>
    [Remote, Public]
    public DateTime? GetNextPeriod()
    {
      return GetNextPeriod(_obj.PeriodExpression, null);
    }
    
    /// <summary>
    /// Получить следующую дату выполнения.
    /// </summary>
    /// <param name="baseDate">Дата, от которой идет вычисление.</param>
    [Remote, Public]
    public DateTime? GetNextPeriod(DateTime? baseDate)
    {
      return GetNextPeriod(_obj.PeriodExpression, baseDate);
    }
    
    /// <summary>
    /// Получить следующую дату выполнения.
    /// </summary>
    /// <param name="periodExpression">Выражение относительной даты.</param>
    /// <param name="baseDate">Дата, от которой идет вычисление.</param>
    /// <remarks>Если baseDate = null, берется Calendar.Now.</remarks>
    [Remote, Public]
    public DateTime? GetNextPeriod(string periodExpression, DateTime? baseDate)
    {
      if (string.IsNullOrEmpty(periodExpression))
        return null;
      
      if (_obj.DateBegin.HasValue && Calendar.Now < _obj.DateBegin.Value)
        return _obj.DateBegin.Value;
      
      if (!baseDate.HasValue)
        baseDate = Calendar.Now;
      
      return GetNextPeriod(periodExpression, baseDate, 1);
    }

    /// <summary>
    /// Получить следующую дату выполнения рекурсивно.
    /// </summary>
    /// <param name="periodExpression">Выражение относительной даты.</param>
    /// <param name="baseDate">Дата, от которой идет вычисление.</param>
    /// <param name="recursionLevel">Уровень рекурсии.</param>
    /// <remarks>Если baseDate = null, берется Calendar.Now.</remarks>
    private DateTime? GetNextPeriod(string periodExpression, DateTime? baseDate, int recursionLevel)
    {
      if (recursionLevel > 20)
        throw new ArgumentException("Не удается вычислить следующий период. Слишком большая цепочка выражений или цикл."); // TODO доделать обработку ошибки
      
      var resultDate =  PublicFunctions.RelativeDate.GetDateFromUIExpression(periodExpression, baseDate).Key;
      
      if (!resultDate.HasValue)
        return resultDate;
      
      if (resultDate <= Calendar.Now)
        resultDate = GetNextPeriod(periodExpression, resultDate, ++recursionLevel);
      
      if (_obj.DateBegin.HasValue && resultDate < _obj.DateBegin.Value)
        return _obj.DateBegin.Value;
      
      return resultDate;
    }
  }
}