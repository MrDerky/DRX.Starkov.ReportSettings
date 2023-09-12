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
      
      return resultDate;
    }

    /// <summary>
    /// Загрузить параметры отчета.
    /// </summary>
    [Public]
    public void SaveReportParams()
    {
      var reportGuid = Guid.Parse(_obj.ReportGuid);
      var report = Starkov.ScheduledReports.PublicFunctions.Module.GetReportMetaData(reportGuid);
      
      _obj.ReportParams.Clear();
      foreach (var parameter in report.Parameters)
      {
        if (parameter.NameResourceKey == "ReportSessionId")
          continue;
        
        var reportParam = _obj.ReportParams.AddNew();
        reportParam.Parameter = parameter.NameResourceKey;
        reportParam.InternalDataTypeName = parameter.InternalDataTypeName;
        if (parameter.EntityMetadata != null)
          reportParam.EntityGuid = parameter.EntityType.ToString();
      }
    }
    
    /// <summary>
    /// Загрузить параметры отчета.
    /// </summary>
    [Public]
    public void SaveReportParamsClear(Sungero.Reporting.Shared.ReportBase report)
    {
      _obj.ReportParams.Clear();
      var reportMetaData = Starkov.ScheduledReports.PublicFunctions.Module.GetReportMetaData(report.Info.ReportTypeId);
      
      foreach (var parameter in report.Parameters)
      {
        if (parameter.Key == "ReportSessionId")
          continue;
        
        var reportParam = _obj.ReportParams.AddNew();

        reportParam.Parameter = parameter.Key;

        var entityParameter = parameter.Value as Sungero.Reporting.Shared.EntityParameter;
        if (entityParameter != null)
        {
          reportParam.EntityId = entityParameter.Entity.Id;
          reportParam.ViewValue = entityParameter.Entity.DisplayValue;
          reportParam.EntityGuid = entityParameter.EntityType.ToString();
          reportParam.InternalDataTypeName = entityParameter.GetType().ToString();
        }
        else
          reportParam.ViewValue = parameter.Value.ToString();
      }
    }
    
    /// <summary>
    /// Загрузить параметры отчета.
    /// </summary>
    [Public]
    public void SaveReportParams(Sungero.Reporting.Shared.ReportBase report)
    {
      foreach (var parameter in report.Parameters)
      {
        var reportParam = _obj.ReportParams.FirstOrDefault(p => p.Parameter == parameter.Key);
        if (reportParam != null)
        {
          var entityParameter = parameter.Value as Sungero.Reporting.Shared.EntityParameter;
          if (entityParameter != null)
          {
            reportParam.EntityId = entityParameter.Entity.Id;
            reportParam.ViewValue = entityParameter.Entity.DisplayValue;
          }
          else
            reportParam.ViewValue = parameter.Value.ToString();
        }
      }
    }
    
  }
}