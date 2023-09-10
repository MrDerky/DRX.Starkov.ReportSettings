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
      var stateView = StateView.Create();
      stateView.IsPrintable = true;
      
      var scheduleLogs = Functions.Module.GetScheduleLogs(_obj)
        .OrderByDescending(s => s.StartDate)
        .Take(10);
      
      var iconSize = StateBlockIconSize.Large;
      
      foreach (var log in scheduleLogs)
      {
        var block = stateView.AddBlock();
        
        block.AddLabel("Плановый запуск: " + log.StartDate.Value.ToUserTime().ToString("g"));
        var content = block.AddContent();
        
        var statusStyle = StateBlockLabelStyle.Create();
        statusStyle.FontWeight = FontWeight.Bold;
        
        if (log.Status == ScheduledReports.ScheduleLog.Status.Error)
          statusStyle.Color = Colors.Common.Red;
        else if (log.Status == ScheduledReports.ScheduleLog.Status.Waiting)
        {
          statusStyle.Color = Colors.Common.Green;
          block.AssignIcon(ScheduleLogs.Resources.Waiting, iconSize);
        }
        else if(log.Status == ScheduledReports.ScheduleLog.Status.Closed)
        {
          statusStyle.Color = Colors.Common.LightGray;
        }
        
        block.AddLabel(log.Info.Properties.Status.GetLocalizedValue(log.Status.Value), statusStyle);
        block.AddLineBreak();
        block.AddLabel(log.Comment);
        
        if (log.DocumentId.HasValue)
          block.AddHyperlink("Открыть", Hyperlinks.Get(Sungero.Docflow.OfficialDocuments.Info, log.DocumentId.Value));
      }
      
      
      return stateView;
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
        //        if (parameter.NameResourceKey == "ReportSessionId")
        //          continue;
        
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
        var reportParam = _obj.ReportParams.AddNew();

        reportParam.Parameter = parameter.Key;

        var entityParameter = parameter.Value as Sungero.Reporting.Shared.EntityParameter;
        if (entityParameter != null)
        {
          reportParam.EntityId = entityParameter.Entity.Id; //EntityIdentifier.Id;
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
            reportParam.EntityId = entityParameter.Entity.Id; //EntityIdentifier.Id;
            reportParam.ViewValue = entityParameter.Entity.DisplayValue;
          }
          else
            reportParam.ViewValue = parameter.Value.ToString();
        }
      }
    }
    
    //    /// <summary>
    //    /// Получить объект из параметров отчета в настройках.
    //    /// </summary>
    //    /// <param name="reportParam">Строка коллекции параметров отчета.</param>
    //    /// <returns>Объект.</returns>
    //    public static object GetObjectFromReportParam(Starkov.ScheduledReports.IScheduleSettingReportParams reportParam)
    //    {
    //      try
    //      {
    //        Guid typeGuid;
    //        if (Guid.TryParse(reportParam.EntityGuid, out typeGuid))
    //          return PublicFunctions.Module.Remote.GetEntitiesByGuid(typeGuid, reportParam.EntityId);
//
    //        if (reportParam.InternalDataTypeName == "System.DateTime")
    //          return GetDateFromReportParam(reportParam);
//
    //        var type = System.Type.GetType(reportParam.InternalDataTypeName);
    //        if (type != null)
    //          return System.Convert.ChangeType(reportParam.ViewValue, type);
    //      }
    //      catch (Exception ex)
    //      {
    //        Logger.ErrorFormat("GetObjectFromReportParam. Не удалось получить объект: Parameter={0}, InternalDataTypeName={1}, EntityGuid={2}, ViewValue={3}",
    //                           ex, reportParam.Parameter, reportParam.InternalDataTypeName, reportParam.EntityGuid, reportParam.ViewValue);
    //        throw ex;
    //      }
//
    //      return null;
    //    }
//
    //    /// <summary>
    //    /// Получить дату из параметра настроек.
    //    /// </summary>
    //    /// <param name="reportParam">Строка коллекции параметров отчета.</param>
    //    /// <returns>Дата.</returns>
    //    public static DateTime? GetDateFromReportParam(Starkov.ScheduledReports.IScheduleSettingReportParams reportParam)
    //    {
    //      DateTime date;
//
    //      if (reportParam.IsRelativeDate != true)
    //        System.DateTime.TryParse(reportParam.ViewValue, out date);
    //      else
    //      {
    //        // TODO Изменить логику вычисления относительной даты
    //        //        var expression = reportParam.EntityId.HasValue
    //        //          ? RelativeDates.GetAll(r => r.Id == reportParam.EntityId.Value).FirstOrDefault()?.RelativeExpression ?? string.Empty
    //        //          : reportParam.ViewValue;
    //        //        date = RelativeDateCalculator.Calculator.Calculate(expression);
    //        date = Calendar.Now;
    //      }
//
    //      return date;
    //    }
//
    //    public static void BuildViewValueForRelativeDate(IRelativeDate relativeDate, int? increment)
    //    {
//
    //    }
//
    //    public static int? GetIncrementForRelativeDateFromViewValue(string ViewValue)
    //    {
//
    //    }
    
  }
}