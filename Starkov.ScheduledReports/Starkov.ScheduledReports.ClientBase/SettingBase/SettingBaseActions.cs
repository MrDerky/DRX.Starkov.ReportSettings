using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.SettingBase;
using Sungero.Domain.Shared;

namespace Starkov.ScheduledReports.Client
{
  partial class SettingBaseParametersActions
  {

    public virtual bool CanEditParameterValue(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return true;
    }

    public virtual void EditParameterValue(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      Functions.SettingBase.EditParameter(_obj);
    }

    public virtual bool CanClearParameterValue(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      return !string.IsNullOrEmpty(_obj.ViewValue);
    }

    public virtual void ClearParameterValue(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      Functions.SettingBase.ClearReportParameter(_obj);
    }
  }

  partial class SettingBaseActions
  {


    public virtual void StartReportWithParameters(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        var report = PublicFunctions.Module.GetModuleReportByGuid(Guid.Parse(_obj.ModuleGuid), Guid.Parse(_obj.ReportGuid));
        if (report == null)
          return;
        
        Functions.SettingBase.FillReportParams(_obj, report);
        report.Open();
      }
      catch (Exception ex)
      {
        e.AddError(SettingBases.Resources.FillRequiredParametersError);
      }
    }

    public virtual bool CanStartReportWithParameters(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return Functions.SettingBase.IsFillReportParamsAny(_obj);
    }

    public virtual void StartReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        var report = PublicFunctions.Module.GetModuleReportByGuid(Guid.Parse(_obj.ModuleGuid), Guid.Parse(_obj.ReportGuid));
        if (report == null)
          return;
        
        report.Open();
        
        Functions.SettingBase.WriteParamsToReport(_obj, report);
      }
      catch (Exception ex)
      {
        e.AddError(SettingBases.Resources.FillRequiredParametersError);
      }
    }

    public virtual bool CanStartReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !string.IsNullOrEmpty(_obj.ReportGuid);
    }

  }


}