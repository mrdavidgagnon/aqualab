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

        private ModelData m_PrevModelData;
        private ModelData m_NewModelData;

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

        // TODO: Instead of logging model values here, log that a value was changed, then log the details in LogModelingSyncChange
        public void LogModelingBehaviorChange(string scenarioId, string actorType, string valueType, string prevValue, string newValue)
        {
            if (m_ScenarioId != scenarioId)
            {
                m_ScenarioId = scenarioId;
            }

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "scenario_id", scenarioId },
                { "curr_tick", m_CurrTick },
                { "prev_sync", m_CurrSync },
                { "prev_value", prevValue },
                { "rule_data", new RuleData(actorType, valueType, newValue).DataString }
            };

            m_Logger.Log(new LogEvent(data));
        }

        public void LogModelingTickChange(string clickLocation, string prevTick, string newTick)
        {
            m_CurrTick = newTick;

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "scenario_id", m_ScenarioId},
                { "click_location", clickLocation },
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

            if (m_PrevModelData != null && m_NewModelData != null)
            {
                Dictionary<string, string> data = new Dictionary<string, string>()
                {
                    { "scenario_id", scenarioId },
                    { "prev_sync", prevSync },
                    { "new_sync", newSync },
                    { "prev_model_data", m_PrevModelData.DataString },
                    { "new_model_data", m_NewModelData.DataString }
                };
                m_Logger.Log(new LogEvent(data));
            }
        }

        #endregion // Logging
    }

    #region Data Classes

    // TODO: Refactor classes to implement an interface, since both have DataString field ?

    public class RuleData
    {
        public string DataString { get; set; }

        public RuleData(string organism, string valueType, string currValue)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{{\"organism\":\"{0}\"}},{{\"value_type\":\"{1}\"}},{{\"curr_value\":\"{2}\"}}", organism, valueType, currValue);
            DataString = stringBuilder.ToString();
            stringBuilder.Clear();
        }
    }

    public class ModelData
    {
        public string DataString { get; set; }

        // string prevTick, string newTick, string changedRuleData
        public ModelData(Dictionary<string, string> data)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (KeyValuePair<string, string> kvp in data)
            {
                stringBuilder.AppendFormat("{{\"{0}\":\"{1}\"}},", kvp.Key, kvp.Value);
            }

            stringBuilder.Length--;
            DataString = stringBuilder.ToString();
            stringBuilder.Clear();
        }
    }
    
    #endregion // Data Classes
}
