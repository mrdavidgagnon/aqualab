using System.Collections.Generic;
using BeauData;
using BeauUtil;
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
        
        #region Log Variables

        private string currTick;
        private string currSync;

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
        public void LogModelingBehaviorChange(string scenarioId, string actorType, string prevValue, string newValue)//, string prevSync, string newSync, string modelingViewCurrent)
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "scenario_id", scenarioId },
                { "curr_tick", currTick },
                { "curr_sync", currSync }, // TODO: Will sync update before/after actor states are logged?
                { "actor_type", actorType },
                { "prev_value", prevValue },
                { "new_value", newValue}
            };

            m_Logger.Log(new LogEvent(data));
        }

        // TODO: Find elegant way to get scenarioId
        public void LogModelingTickChange(string prevTick, string newTick)
        {
            currTick = newTick;

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "prev_tick", prevTick },
                { "new_tick", newTick }
            };

            m_Logger.Log(new LogEvent(data));
        }

        public void LogModelingSyncChange(string scenarioId, string prevSync, string newSync)
        {
            currSync = newSync;

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
