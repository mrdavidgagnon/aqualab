using System.Collections.Generic;
using System.Text;
using BeauData;
using BeauUtil;
using FieldDay;
using UnityEngine;

namespace ProtoAqua
{
    public partial class AnalyticsService : ServiceBehaviour
    {
        #region Inspector

        [SerializeField] private string m_AppId = "AQUALAB";
        [SerializeField] private int m_AppVersion = 1;
        [SerializeField] private QueryParams m_QueryParams = null;

        #endregion // Inspector

        private SimpleLog m_Logger;
        
        #region Log Variables

        private string m_ScenarioId;
        private string m_CurrTick;
        private string m_CurrSync;

        #endregion // Log Variables

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
        public void LogModelingBehaviorChange(string scenarioId, string actorType, string valueType, string prevValue, string newValue)//, string prevSync, string newSync, string modelingViewCurrent)
        {
            RuleData ruleData = new RuleData(actorType, valueType, newValue);

            if (m_ScenarioId != scenarioId)
            {
                m_ScenarioId = scenarioId;
            }

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "scenario_id", scenarioId },
                { "curr_tick", m_CurrTick },
                { "curr_sync", m_CurrSync }, // TODO: Sync updated after values are changed and logged
                { "prev_value", prevValue },
                { "rule_data", ruleData.ToJSONString() }
            };

            m_Logger.Log(new LogEvent(data));
        }

        public void LogModelingTickChange(string prevTick, string newTick)
        {
            m_CurrTick = newTick;

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "scenario_id", m_ScenarioId},
                { "prev_tick", prevTick },
                { "new_tick", newTick }
            };

            m_Logger.Log(new LogEvent(data));
        }

        public void LogModelingSyncChange(string scenarioId, string prevSync, string newSync)
        {
            m_CurrSync = newSync;

            if (m_ScenarioId != scenarioId)
            {
                m_ScenarioId = scenarioId;
            }

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "scenario_id", scenarioId },
                { "prev_sync", prevSync },
                { "new_sync", newSync }
            };

            m_Logger.Log(new LogEvent(data));
        }

        #endregion // Logging
    }

    public class RuleData
    {
        public string Organism { get; set; }
        public string ValueType { get; set; }
        public string CurrValue { get; set; }

        public RuleData(string organism, string valueType, string currValue)
        {
            Organism = organism;
            ValueType = valueType;
            CurrValue = currValue;
        }

        // Return rule data values as a single string in JSON format
        public string ToJSONString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{{\"organism\":\"{0}\"}},{{\"value_type\":\"{1}\"}},{{\"curr_value\":\"{2}\"}}", Organism, ValueType, CurrValue);
            string jsonString = sb.ToString();
            sb.Clear();

            return jsonString;
        }
    }
}
