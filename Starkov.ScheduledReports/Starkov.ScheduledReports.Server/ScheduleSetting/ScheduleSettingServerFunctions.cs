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
    public static Starkov.ScheduledReports.IScheduleSetting GetScheduleSetting(int? id)
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
      return GetNextPeriod(_obj.PeriodNumber, null);
    }
    
    /// <summary>
    /// Получить следующую дату выполнения.
    /// </summary>
    /// <param name="baseDate">Дата, от которой идет вычисление.</param>
    /// <remarks>Если baseDate = null, берется Calendar.Now.</remarks>
    [Remote, Public]
    public DateTime? GetNextPeriod(DateTime? baseDate)
    {
      return GetNextPeriod(_obj.PeriodNumber, baseDate);
    }
    
    /// <summary>
    /// Получить следующую дату выполнения.
    /// </summary>
    /// <param name="number">Множитель.</param>
    /// <param name="baseDate">Дата, от которой идет вычисление.</param>
    /// <remarks>Если baseDate = null, берется Calendar.Now.</remarks>
    [Remote, Public]
    public DateTime? GetNextPeriod(int? number, DateTime? baseDate)
    {
      if (_obj.DateBegin.HasValue && Calendar.Now < _obj.DateBegin.Value)
        return _obj.DateBegin.Value;
      
      if (_obj.Period == null)
        return null;
      
      if (!baseDate.HasValue)
        baseDate = Calendar.Now;
      
      var resultDate =  PublicFunctions.RelativeDate.CalculateDate(_obj.Period, baseDate, number);
      
      if (resultDate < Calendar.Now)
        resultDate = PublicFunctions.RelativeDate.CalculateDate(_obj.Period, Calendar.Now, number);
      
      if (_obj.DateBegin.HasValue && resultDate < _obj.DateBegin.Value)
        return _obj.DateBegin.Value;
      
      if (resultDate < Calendar.Now)
        return GetNextPeriod(number, resultDate);
      
      return resultDate;
    }

  }
}