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
        
        var info = Functions.SettingBase.GetReportParameterInfo(reportParam);
        info.ParameterName = parameter.NameResourceKey;
        info.InternalDataTypeName = parameter.InternalDataTypeName;
        info.IsCollection = parameter.IsCollection;
        
        if (parameter.EntityMetadata != null)
        {
          reportParam.DisplayName = Sungero.Domain.Shared.TypeExtension.GetTypeByGuid(parameter.EntityType).GetEntityMetadata().GetDisplayName();
          info.IsEntity = true;
          info.EntityGuid = parameter.EntityType.ToString();
        }
        
        Functions.SettingBase.WriteReportParameterInfo(reportParam, info);
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
        if (reportParam == null)
          continue;
        
        var paramInfo = Functions.SettingBase.GetReportParameterInfo(reportParam);
        if (!paramInfo.IsCollection)
        {
          var entityParameter = parameter.Value as Sungero.Reporting.Shared.EntityParameter;
          if (entityParameter != null)
            WriteEntityToParameterInfo(paramInfo, entityParameter.Entity);
          else
            paramInfo.DisplayValue = parameter.Value.ToString().Contains(reportParam.InternalDataTypeName) ? string.Empty : parameter.Value.ToString();
        }
        else
        {
          // Обработка коллекции
          var entityParameter = parameter.Value as Sungero.Reporting.Shared.CollectionAdapter<Sungero.Domain.Shared.IEntity>;
          if (entityParameter != null)
            WriteEntityToParameterInfo(paramInfo, entityParameter.ToList());
        }
        
        Functions.SettingBase.WriteReportParameterInfo(reportParam, paramInfo);
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
        var info = Functions.SettingBase.GetReportParameterInfo(reportParam);
        Guid typeGuid;
        if (Guid.TryParse(reportParam.EntityGuid, out typeGuid))
        {
          if (!info.IsCollection)
            return PublicFunctions.Module.Remote.GetEntitiesByGuid(typeGuid, info.EntityIds.FirstOrDefault());
          else
            return PublicFunctions.Module.Remote.GetEntitiesByGuid(typeGuid, info.EntityIds);
        }
        
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
    private static DateTime GetDateFromReportParam(Starkov.ScheduledReports.ISettingBaseParameters reportParam)
    {
      DateTime date = Calendar.Now;
      
      var info = Functions.SettingBase.GetReportParameterInfo(reportParam);
      
      if (info.IsRelatedDate)
        date = PublicFunctions.RelativeDate.GetDateFromExpression(reportParam.ViewValue).GetValueOrDefault();
      else if (!string.IsNullOrEmpty(info.DisplayValue))
        Calendar.TryParseDateTime(info.DisplayValue, out date);
      
      return date;
    }
    
    /// <summary>
    /// Получить информацию о параметре отчета в виде структуры.
    /// </summary>
    [Public]
    public static Structures.Module.IReportParameterInfo GetReportParameterInfo(ISettingBaseParameters parameter)
    {
      return IsolatedFunctions.JsonParser.GetReportParameterInfoStruct(parameter.ParameterInfo);
    }
    
    /// <summary>
    /// Записать информацию о параметре из струкуры.
    /// </summary>
    /// <param name="parameter">Параметр в коллекции.</param>
    /// <param name="parameterInfo">Структура с данными.</param>
    [Public]
    public static void WriteReportParameterInfo(ISettingBaseParameters parameter, Structures.Module.IReportParameterInfo info)
    {
      if (parameter.ParameterName != info.ParameterName)
        parameter.ParameterName = info.ParameterName;
      
      // DisplayName редактируется из интерфейса и обновляется в структуру
      if (parameter.DisplayName != info.DisplayName)
        info.DisplayName = parameter.DisplayName;
      
      if (parameter.ViewValue != info.DisplayValue)
        parameter.ViewValue = info.DisplayValue;

      var parameterInfo = IsolatedFunctions.JsonParser.GetReportParameterInfoText(info);
      if (parameter.ParameterInfo != parameterInfo)
        parameter.ParameterInfo = parameterInfo;
    }
    
    public static void WriteEntityToParameterInfo(Structures.Module.IReportParameterInfo info, List<Sungero.Domain.Shared.IEntity> entities)
    {
      info.EntityIds = entities.Select(_ => _.Id).ToList();
      info.DisplayValue = string.Join("; ", entities.Select(_ => _.DisplayValue).ToList());
    }
    
    public static void WriteEntityToParameterInfo(Structures.Module.IReportParameterInfo info, Sungero.Domain.Shared.IEntity entity)
    {
      info.EntityIds = new List<long> { entity.Id };
      info.DisplayValue = entity.DisplayValue;
    }
    
    /// <summary>
    /// Очистить значение параметра.
    /// </summary>
    /// <param name="parameter">Параметр.</param>
    public static void ClearReportParameter(ISettingBaseParameters parameter)
    {
      var info = GetReportParameterInfo(parameter);
      info.DisplayValue = string.Empty;
      info.EntityIds.Clear();
      
      WriteReportParameterInfo(parameter, info);
    }
  }
}