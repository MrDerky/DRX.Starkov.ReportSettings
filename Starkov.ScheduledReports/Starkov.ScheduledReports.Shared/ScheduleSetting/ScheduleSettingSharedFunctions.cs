﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ScheduleSetting;

namespace Starkov.ScheduledReports.Shared
{
  partial class ScheduleSettingFunctions
  {

    /// <summary>
    /// Установить доступность свойств.
    /// </summary>       
    public void SetPropertyStates()
    {
      var properties = _obj.State.Properties;
      var canChangeSchedule = _obj.Status != Status.Active;
      
      properties.Name.IsEnabled = canChangeSchedule;
      properties.DateBegin.IsEnabled = canChangeSchedule;
      properties.Period.IsEnabled = canChangeSchedule;
      properties.PeriodNumber.IsEnabled = canChangeSchedule;
      properties.IsAsyncExecute.IsEnabled = canChangeSchedule;
      properties.Observers.IsEnabled = canChangeSchedule;
      
      var isHasReport = !string.IsNullOrEmpty(_obj.ReportGuid);
      properties.Observers.IsVisible = isHasReport;
      properties.Period.IsVisible = isHasReport;
      properties.DateBegin.IsVisible = isHasReport;
      properties.DateEnd.IsVisible = isHasReport;
      properties.ReportName.IsVisible = isHasReport;
      properties.ShowParams.IsVisible = isHasReport;
    }

    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public virtual void FillName()
    {
      _obj.Name = _obj.ReportName;
    }
    
    /// <summary>
    /// Признак что заполнен значением хотя бы один параметр.
    /// </summary>
    public bool IsFillReportParamsAny()
    {
      return _obj.ReportParams.Any(r => !string.IsNullOrEmpty(r.ViewValue));
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
          return PublicFunctions.Module.Remote.GetEntitiesByGuid(typeGuid, reportParam.EntityId);
        
        if (reportParam.InternalDataTypeName == "System.DateTime")
          return GetDateFromReportParam(reportParam);
        
        var type = System.Type.GetType(reportParam.InternalDataTypeName);
        if (type != null)
          return System.Convert.ChangeType(reportParam.ViewValue, type);
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("GetObjectFromReportParam. Не удалось получить объект: Parameter={0}, InternalDataTypeName={1}, EntityGuid={2}, ViewValue={3}",
                           ex, reportParam.Parameter, reportParam.InternalDataTypeName, reportParam.EntityGuid, reportParam.ViewValue);
        throw ex;
      }

      return null;
    }
    
    /// <summary>
    /// Получить дату из параметра настроек.
    /// </summary>
    /// <param name="reportParam">Строка коллекции параметров отчета.</param>
    /// <returns>Дата, или текущая дата и время, если получить из настроек не удалось.</returns>
    public static DateTime? GetDateFromReportParam(Starkov.ScheduledReports.IScheduleSettingReportParams reportParam)
    {
      DateTime date = Calendar.Now;
      
      if (reportParam.EntityId.HasValue)
      {
        var relativeDate = PublicFunctions.Module.Remote.GetRelativeDate(reportParam.EntityId.Value);
        if (relativeDate != null)
          date = PublicFunctions.RelativeDate.CalculateDate(relativeDate, null, Functions.ScheduleSetting.GetIncrementForRelativeDateFromViewValue(reportParam.ViewValue));
      }
      else if (!string.IsNullOrEmpty(reportParam.ViewValue))
        Calendar.TryParseDateTime(reportParam.ViewValue, out date);
      
      return date;
    }
    
    /// <summary>
    /// Получить представление относительной даты.
    /// </summary>
    /// <param name="relativeDate">Относительная дата.</param>
    /// <param name="increment">Множитель.</param>
    public static string BuildViewValueForRelativeDate(IRelativeDate relativeDate, int? increment)
    {
      return string.Join(GetDelimeter().ToString(), increment, relativeDate.Name);
    }
    
    /// <summary>
    /// Получить множитель из представления относительной даты.
    /// </summary>
    /// <param name="viewValue">Строка представления.</param>
    /// <returns>Число.</returns>
    public static int? GetIncrementForRelativeDateFromViewValue(string viewValue)
    {
      var delimeter = GetDelimeter();
      var viewValueParts = viewValue.Split(delimeter);
      if (viewValueParts.Count() < 2)
        return null;
      
      int increment;
      if (int.TryParse(viewValueParts[0], out increment))
        return increment;
          
      return null;
    }
    
    private static char GetDelimeter()
    {
      return ' ';
    }
  }
}