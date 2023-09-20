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
    public static IScheduleSetting CreateScheduleSetting()
    {
      return ScheduleSettings.Create();
    }
    
    /// <summary>
    /// Получить запись настройки расписания для отчета по ИД.
    /// </summary>
    [Public, Remote(IsPure = true)]
    public static IScheduleSetting GetScheduleSetting(int? id)
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

    /// <summary>
    /// Загрузить параметры отчета.
    /// </summary>
    [Public]
    public void SaveReportParams()
    {
      //var reportGuid = Guid.Parse(_obj.ReportGuid);
      _obj.ReportParams.Clear();
      var report = _obj.ReportSetting;// Starkov.ScheduledReports.PublicFunctions.Module.GetReportMetaData(reportGuid);
      if (report == null)
        return;
      
      foreach (var parameter in report.Parameters.Where(p => !string.IsNullOrEmpty(p.DisplayName)))
      {
        var reportParam = _obj.ReportParams.AddNew();
        reportParam.ParameterName = parameter.ParameterName;
        reportParam.ParameterDisplay = parameter.DisplayName;
        
        if (!string.IsNullOrEmpty(parameter.DisplayValue))
          reportParam.DisplayValue = parameter.DisplayValue;
        
        if (!string.IsNullOrEmpty(parameter.EntityGuid))
          reportParam.EntityGuid = parameter.EntityGuid;
        
        if (parameter.EntityId.HasValue)
          reportParam.EntityId = parameter.EntityId.Value;
        
        if (!string.IsNullOrEmpty(parameter.InternalDataTypeName))
          reportParam.InternalDataTypeName = parameter.InternalDataTypeName;
        
        if (!string.IsNullOrEmpty(parameter.DisplayValue))
          reportParam.ViewValue = parameter.ViewValue;
      }
    }
    
    //    /// <summary>
    //    /// Загрузить параметры отчета.
    //    /// </summary>
    //    [Public]
    //    public void SaveReportParamsClear(Sungero.Reporting.Shared.ReportBase report)
    //    {
    //      _obj.ReportParams.Clear();
    //      var reportMetaData = Starkov.ScheduledReports.PublicFunctions.Module.GetReportMetaData(report.Info.ReportTypeId);
//
    //      foreach (var parameter in report.Parameters)
    //      {
    //        if (parameter.Key == "ReportSessionId")
    //          continue;
//
    //        var reportParam = _obj.ReportParams.AddNew();
//
    //        reportParam.Parameter = parameter.Key;
//
    //        var entityParameter = parameter.Value as Sungero.Reporting.Shared.EntityParameter;
    //        if (entityParameter != null)
    //        {
    //          reportParam.EntityId = entityParameter.Entity.Id;
    //          reportParam.ViewValue = entityParameter.Entity.DisplayValue;
    //          reportParam.EntityGuid = entityParameter.EntityType.ToString();
    //          reportParam.InternalDataTypeName = entityParameter.GetType().ToString();
    //        }
    //        else
    //          reportParam.ViewValue = parameter.Value.ToString();
    //      }
    //    }
    
    /// <summary>
    /// Загрузить параметры отчета.
    /// </summary>
    [Public]
    public void SaveReportParams(Sungero.Reporting.Shared.ReportBase report)
    {
      foreach (var parameter in report.Parameters)
      {
        var reportParam = _obj.ReportParams.FirstOrDefault(p => p.ParameterName == parameter.Key);
        if (reportParam != null)
        {
          var entityParameter = parameter.Value as Sungero.Reporting.Shared.EntityParameter;
          if (entityParameter != null)
          {
            reportParam.EntityId = entityParameter.Entity.Id;
            reportParam.ViewValue = entityParameter.Entity.DisplayValue;
          }
          else
            reportParam.ViewValue = parameter.Value.ToString().Contains(reportParam.InternalDataTypeName) ? string.Empty : parameter.Value.ToString();
        }
      }
    }
    
  }
}