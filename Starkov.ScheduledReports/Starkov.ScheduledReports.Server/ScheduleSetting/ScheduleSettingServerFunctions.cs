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
    /// Получить следующую дату выполнения.
    /// </summary>
    [Remote, Public]
    public DateTime? GetNextPeriod()
    {
      return GetNextPeriod(_obj.PeriodNumber);
    }
    
    /// <summary>
    /// Получить следующую дату выполнения.
    /// </summary>
    /// <param name="number">Множитель.</param>
    [Remote, Public]
    public DateTime? GetNextPeriod(int? number)
    {
      if (_obj.DateBegin.HasValue && Calendar.Now < _obj.DateBegin.Value)
        return _obj.DateBegin.Value;
      
      //      if (!times.HasValue || times.Value == 0)
      //        times = 1;
      
      
      //for (int i = 1; i != times.Value; i + times.Value)
      if (_obj.Period == null)
        return null;
      
      var resultDate = Calendar.Now;
      
      if (PublicFunctions.RelativeDate.IsInitialized(_obj.Period))
        resultDate = PublicFunctions.RelativeDate.CalculateDate(_obj.Period, resultDate, number);
      else
      {
        if (!number.HasValue || number.Value == 0)
          number = 1;
        
        for (int i = 1; i != number.Value; i = number.Value > 0 ? i++ : i--)
          resultDate = PublicFunctions.RelativeDate.CalculateDate(_obj.Period, resultDate);
      }
      
      if (_obj.DateEnd.HasValue && resultDate > _obj.DateEnd.Value)
        return null;
      
      return resultDate;// RelativeDateCalculator.Calculator.Calculate(_obj.Period.RelativeExpression);
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
          reportParam.ValueId = entityParameter.Entity.Id; //EntityIdentifier.Id;
          reportParam.ValueText = entityParameter.Entity.DisplayValue;
          reportParam.EntityGuid = entityParameter.EntityType.ToString();
          reportParam.InternalDataTypeName = entityParameter.GetType().ToString();
        }
        else
          reportParam.ValueText = parameter.Value.ToString();
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
            reportParam.ValueId = entityParameter.Entity.Id; //EntityIdentifier.Id;
            reportParam.ValueText = entityParameter.Entity.DisplayValue;
          }
          else
            reportParam.ValueText = parameter.Value.ToString();
        }
      }
    }
    
    /// <summary>
    /// Получить объект из параметров отчета в настройках.
    /// </summary>
    /// <param name="reportParam">Строка коллекции параметров отчета.</param>
    /// <returns>Объект.</returns>
    public static object GetObjectFromReportParam(Starkov.ScheduledReports.IScheduleSettingReportParams reportParam)
    {
      try
      {
        Guid typeGuid;
        if (Guid.TryParse(reportParam.EntityGuid, out typeGuid))
          return PublicFunctions.Module.Remote.GetEntitiesByGuid(typeGuid, reportParam.ValueId);
        
        if (reportParam.InternalDataTypeName == "System.DateTime")
          return GetDateFromReportParam(reportParam);
        
        var type = System.Type.GetType(reportParam.InternalDataTypeName);
        if (type != null)
          return System.Convert.ChangeType(reportParam.ValueText, type);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetObjectFromReportParam. Не удалось получить объект: Parameter={0}, InternalDataTypeName={1}, EntityGuid={2}, ValueText={3}",
                           ex, reportParam.Parameter, reportParam.InternalDataTypeName, reportParam.EntityGuid, reportParam.ValueText);
        throw ex;
      }

      return null;
    }
    
    /// <summary>
    /// Полуить дату из параметра настроек.
    /// </summary>
    /// <param name="reportParam">Строка коллекции параметров отчета.</param>
    /// <returns>Дата.</returns>
    public static DateTime? GetDateFromReportParam(Starkov.ScheduledReports.IScheduleSettingReportParams reportParam)
    {
      DateTime date;
      
      if (reportParam.IsRelativeDate != true)
        System.DateTime.TryParse(reportParam.ValueText, out date);
      else
      {
        // TODO Изменить логику вычисления относительной даты
        //        var expression = reportParam.ValueId.HasValue
        //          ? RelativeDates.GetAll(r => r.Id == reportParam.ValueId.Value).FirstOrDefault()?.RelativeExpression ?? string.Empty
        //          : reportParam.ValueText;
        //        date = RelativeDateCalculator.Calculator.Calculate(expression);
        date = Calendar.Now;
      }
      
      return date;
    }
    
  }
}