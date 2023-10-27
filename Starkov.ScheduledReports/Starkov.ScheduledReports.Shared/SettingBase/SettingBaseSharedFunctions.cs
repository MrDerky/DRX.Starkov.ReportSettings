using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.SettingBase;
using Sungero.Metadata;
using Sungero.Domain.Shared;

namespace Starkov.ScheduledReports.Shared
{
  partial class SettingBaseFunctions
  {

    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public virtual void FillName()
    {
      if (string.IsNullOrEmpty(_obj.Name) || _obj.State.Properties.ReportName.PreviousValue == _obj.Name)
        _obj.Name = _obj.ReportName;
      else if (!string.IsNullOrEmpty(_obj.State.Properties.ReportName.PreviousValue) && _obj.Name.Contains(_obj.State.Properties.ReportName.PreviousValue))
        _obj.Name = _obj.Name.Replace(_obj.State.Properties.ReportName.PreviousValue, _obj.ReportName);
    }
    
    /// <summary>
    /// Загрузить параметры отчета.
    /// </summary>
    [Public]
    public virtual void SaveReportParams()
    {
      Guid reportGuid;
      if (!Guid.TryParse(_obj.ReportGuid, out reportGuid))
        return;
      
      var report = Starkov.ScheduledReports.PublicFunctions.Module.GetReportMetaData(reportGuid);
      
      _obj.Parameters.Clear();
      foreach (var parameter in report.Parameters)
      {
        if (parameter.NameResourceKey == "ReportSessionId")
          continue;
        
        var reportParam = _obj.Parameters.AddNew();
        reportParam.ParameterName = parameter.NameResourceKey;
        reportParam.InternalDataTypeName = parameter.InternalDataTypeName;
        if (parameter.EntityMetadata != null)
        {
          reportParam.DisplayName = Sungero.Domain.Shared.TypeExtension.GetTypeByGuid(parameter.EntityType).GetEntityMetadata().GetDisplayName();
          reportParam.EntityGuid = parameter.EntityType.ToString();
        }
      }
    }
    
    /// <summary>
    /// Загрузить параметры из карточки в отчет.
    /// </summary>
    [Public]
    public void WriteParamsToReport(Sungero.Reporting.Shared.ReportBase report)
    {
      foreach (var parameter in report.Parameters)
      {
        var reportParam = _obj.Parameters.FirstOrDefault(p => p.ParameterName == parameter.Key);
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
    
    /// <summary>
    /// Заполнить параметры отчета из настроек.
    /// </summary>
    /// <param name="report">Отчет.</param>
    public void FillReportParams(Sungero.Reporting.Shared.ReportBase report)
    {
      var reportParams = _obj.Parameters.Where(p => !string.IsNullOrEmpty(p.ViewValue));
      Logger.DebugFormat("FillReportParams. setting={0}, reportParam={1}", _obj.Id, string.Join(", ", reportParams.Select(p => string.Format("{0}: ViewValue={1}, Id={2}", p.ParameterName, p.ViewValue, p.Id))));
      foreach (var parameter in reportParams)
        report.SetParameterValue(parameter.ParameterName, Functions.SettingBase.GetObjectFromReportParam(parameter));
    }
    
    /// <summary>
    /// Признак что заполнен значением хотя бы один параметр.
    /// </summary>
    public bool IsFillReportParamsAny()
    {
      return _obj.Parameters.Any(r => !string.IsNullOrEmpty(r.ViewValue));
    }

    /// <summary>
    /// Получить объект из параметров отчета в настройках.
    /// </summary>
    /// <param name="reportParam">Строка коллекции параметров отчета.</param>
    /// <returns>Объект.</returns>
    public static object GetObjectFromReportParam(Starkov.ScheduledReports.ISettingBaseParameters reportParam)
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
                           ex, reportParam.ParameterName, reportParam.InternalDataTypeName, reportParam.EntityGuid, reportParam.ViewValue);
        
        throw ex;
      }

      return null;
    }
    
    /// <summary>
    /// Получить дату из параметра настроек.
    /// </summary>
    /// <param name="reportParam">Строка коллекции параметров отчета.</param>
    /// <returns>Дата, или текущая дата и время, если получить из настроек не удалось.</returns>
    private static DateTime? GetDateFromReportParam(Starkov.ScheduledReports.ISettingBaseParameters reportParam)
    {
      DateTime date = Calendar.Now;
      
      if (reportParam.EntityId.HasValue)
      {
        var relativeDate = PublicFunctions.RelativeDate.Remote.GetRelativeDate(reportParam.EntityId.Value);
        if (relativeDate != null)
          date = PublicFunctions.RelativeDate.CalculateDate(relativeDate, null, Functions.SettingBase.GetIncrementForRelativeDateFromViewValue(reportParam.ViewValue));
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