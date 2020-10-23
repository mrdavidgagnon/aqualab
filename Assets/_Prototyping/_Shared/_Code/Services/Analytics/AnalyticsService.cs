using System;
using System.Collections;
using System.Collections.Generic;
using BeauData;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Tags;
using FieldDay;
using UnityEngine;

namespace ProtoAqua
{
    public partial class AnalyticsService : ServiceBehaviour
    {
        #region Inspector

        [SerializeField] private string m_AppId = "Aqualab";
        [SerializeField] private int m_AppVersion = 1;
        [SerializeField] private QueryParams m_QueryParams = null;

        #endregion // Inspector

        private SimpleLog m_Logger;
        public enum ClickType { ARG, EXP, OBS, MDL };

        #region IService

        protected override void OnRegisterService()
        {
            m_QueryParams = Services.Data.PeekQueryParams();
            m_Logger = new SimpleLog(m_AppId, m_AppVersion, m_QueryParams);
        }

        protected override void OnDeregisterService()
        {
            m_Logger?.Flush();
            m_Logger = null;
        }

        public override FourCC ServiceId()
        {
            return ServiceIds.Analytics;
        }

        #endregion // IService

        #region Logging

        // TODO: Should dictionaries be created here, or before the data gets passed to these functions?
        public void LogModelingBehaviorChange(string scenarioId, IRuleData ruleData, string prevValue, string prevSync, string newSync, string modelingViewCurrent)
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "scenario_id", scenarioId },
                { "rule_data", ruleData.ToJSONString() },
                { "prev_value", prevValue },
                { "prev_sync", prevSync },
                { "new_sync", newSync },
                { "modeling_view_current", modelingViewCurrent }
            };

            m_Logger.Log(new LogEvent(data));
        }

        public void LogModelingViewChange(string scenarioId, string clickLocation, string modelingViewLast, string modelingViewCurrent)
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "scenario_id", scenarioId },
                { "click_location", clickLocation },
                { "modeling_view_last", modelingViewLast },
                { "modeling_view_current", modelingViewCurrent }
            };

            m_Logger.Log(new LogEvent(data));
        }

        #endregion // Logging
    }

    // TODO
    public interface IRuleData
    {
        string Organism { get; set; }
        string ValueType { get; set; }
        string CurrValue { get; set; }

        // Return rule data values as a single string in JSON format
        string ToJSONString();
    }
}
