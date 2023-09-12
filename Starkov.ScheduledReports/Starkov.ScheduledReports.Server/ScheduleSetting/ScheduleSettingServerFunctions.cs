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
      
//      var stateView = StateView.Create();
//      stateView.IsPrintable = true;
//      
//      var scheduleLogs = Functions.Module.GetScheduleLogs(_obj)
//        .OrderByDescending(s => s.StartDate)
//        .Take(10);
//      
//      if (!scheduleLogs.Any())
//        return stateView;
//      
//      // TODO Реализовать возможность просмотра всех записей (Листание или отчет)
//      var block = stateView.AddBlock();
//      block.AddHyperlink("Показать все записи", Hyperlinks.Get(ScheduleLogs.Info));
//      
//      var iconSize = StateBlockIconSize.Large;
//      
//      foreach (var log in scheduleLogs)
//      {
//        block = stateView.AddBlock();
//        
//        #region Стили
//        var statusStyle = StateBlockLabelStyle.Create();
//        statusStyle.FontWeight = FontWeight.Bold;
//        
//        if (log.Status == ScheduledReports.ScheduleLog.Status.Complete)
//        {
//          block.AssignIcon(ScheduleLogs.Resources.Complete, iconSize);
//        }
//        else if (log.Status == ScheduledReports.ScheduleLog.Status.Error)
//          statusStyle.Color = Colors.Common.Red;
//        else if (log.Status == ScheduledReports.ScheduleLog.Status.Waiting)
//        {
//          statusStyle.Color = Colors.Common.Green;
//          block.AssignIcon(ScheduleLogs.Resources.Waiting, iconSize);
//        }
//        else if(log.Status == ScheduledReports.ScheduleLog.Status.Closed)
//        {
//          statusStyle.Color = Colors.Common.LightGray;
//        }
//        #endregion
//        
//        block.AddLabel(log.Info.Properties.Status.GetLocalizedValue(log.Status.Value), statusStyle);
//        
//        var content = block.AddContent();
//        content.AddLabel("Плановый запуск: " + log.StartDate.Value.ToUserTime().ToString("g"));
//        
//        content.AddLineBreak();
//        content.AddLabel(log.Comment);
//        
//        block.AddLineBreak();
//        if (log.DocumentId.HasValue)
//          block.AddHyperlink("Просмотр", Hyperlinks.Get(Sungero.Docflow.OfficialDocuments.Info, log.DocumentId.Value));
//      }
//      
//      return stateView;
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
      
      if (_obj.DateEnd.HasValue && resultDate > _obj.DateEnd.Value)
        return null;
      
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