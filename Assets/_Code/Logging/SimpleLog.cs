using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Logging
{
    public class SimpleLog
    {
        private string player_id;

        private bool flushing = false;
        private List<Dictionary<string, string>> accrued_log = new List<Dictionary<string, string>>();
        private int flushed_to = 0;
        private int flush_index = 0;

        private string app_id;
        private int app_version;
        private long session_id;
        private string persistent_session_id;

        public SimpleLog(string app_id, int app_version)
        {
            this.app_id = app_id;
            this.app_version = app_version;

            session_id = UUIDint();

            Debug.Log(session_id);

            // persistent_session_id = getCookie("persistent_session_id");

            // if (persistent_session_id == null)

        }

        public void Log(Dictionary<string, string> data)
        {
            data[session_id.ToString()] = flush_index.ToString();
            // data[client_time] = (new Date()).toISOString().split('T').join(" ");

            flush_index++;
            accrued_log.Add(data);
            Flush();
        }

        public void Flush()
        {
            if (flushing) return;
            flushing = true;

            if (accrued_log.Count == 0) return;

            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("");
        }

        private long UUIDint()
        {
            DateTime dt = DateTime.Now;
            string id = ("" + dt.Year).Substring(2);

            if (dt.Month < 10) id += "0";
            id += dt.Month;

            if (dt.Day < 10) id += "0";
            id += dt.Day;

            if (dt.Hour < 10) id += "0";
            id += dt.Hour;

            if (dt.Minute < 10) id += "0";
            id += dt.Minute;

            if (dt.Second < 10) id += "0";
            id += dt.Second;

            System.Random rand = new System.Random();

            for (int i = 0; i < 5; ++i) 
            {
                id += Math.Floor(rand.NextDouble() * 10);
            }

            return Int64.Parse(id);
        }
    }
}

