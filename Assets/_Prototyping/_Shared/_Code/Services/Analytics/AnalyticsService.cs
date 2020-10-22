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

        // TODO: The idea here is to take in the necessary log values for each type of click event,
        //       construct a dictionary for all the values (so that the values can be passed in line
        //       to this service), and then send the data to the logger as a LogEvent.

        // Note: Used params array here for brevity, otherwise method headers could look really long.
        //       Could be changed to list each parameter if that makes more sense / provides more clarity.
        public void LogArgumentClick(params string[] args)
        {
            // Make sure that all the expected values for logging are passed in.
            if (args.Length != 1) 
            {
                throw new ArgumentException("Invalid args length.");
            }

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "argumentId", args[0] }
            };

            m_Logger.Log(new LogEvent(data));
        }

        public void LogExperimentClick(params string[] args)
        {
            throw new NotImplementedException();
        }

        public void LogObservationClick(params string[] args)
        {
            throw new NotImplementedException();
        }

        public void LogModelingClick(params string[] args)
        {
            throw new NotImplementedException();
        }

        #endregion // Logging
    }
}
