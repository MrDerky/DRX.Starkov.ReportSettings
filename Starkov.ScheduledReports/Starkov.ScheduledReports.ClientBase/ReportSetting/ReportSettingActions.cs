using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Starkov.ScheduledReports.ReportSetting;

namespace Starkov.ScheduledReports.Client
{
  partial class ReportSettingActions
  {
    public virtual void StartReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      try
      {
        var report = PublicFunctions.Module.GetModuleReportByGuid(Guid.Parse(_obj.ModuleGuid), Guid.Parse(_obj.ReportGuid));
        if (report == null)
          return;
        
        report.Open();
        
        PublicFunctions.ReportSetting.SaveReportParams(_obj, report);
        
//        if (_obj.ShowParams != true && Functions.ScheduleSetting.IsFillReportParamsAny(_obj))
//          _obj.ShowParams = _obj.State.Properties.ReportParams.IsVisible = true;
      }
      catch (Exception ex)
      {
//        if (_obj.ShowParams != true)
//          _obj.ShowParams = _obj.State.Properties.ReportParams.IsVisible = true;
        
        e.AddError(Starkov.ScheduledReports.ScheduleSettings.Resources.FillRequiredParametersError);
      }
    }

    public virtual bool CanStartReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !string.IsNullOrEmpty(_obj.ReportGuid);
    }

    public virtual void SetReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Functions.ReportSetting.SelectReport(_obj);
    }

    public virtual bool CanSetReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }

}