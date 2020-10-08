using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using BeauData;
using UnityEngine;

namespace Logging
{
    public class SimpleLog
    {
        private string player_id;

        private bool flushing = false;
        private AccruedLog accrued_log;
        private int flushed_to = 0;
        private int flush_index = 0;

        private string app_id;
        private int app_version;
        private long session_id;
        private string persistent_session_id;

        private string req_url;

        public SimpleLog(string app_id, int app_version)
        {
            this.app_id = app_id;
            this.app_version = app_version;

            session_id = UUIDint();

            // TODO: Change type of persistent_session_id to fix this mess
            persistent_session_id = GetCookie("persistent_session_id");

            if (persistent_session_id == null)
            {
                persistent_session_id = session_id.ToString();
                SetCookie("persistent_session_id", Int32.Parse(persistent_session_id), 100);
            }

            string player_id_str = player_id != null ? "&player_id=" + Uri.EscapeDataString(player_id.ToString()) : "";

            req_url = "https://fielddaylab.wisc.edu/logger/log.php?app_id=" + Uri.EscapeDataString(app_id) + 
                        "&app_version=" + Uri.EscapeDataString(app_version.ToString()) + "&session_id=" + 
                        Uri.EscapeDataString(session_id.ToString()) + "&persistent_session_id=" + 
                        Uri.EscapeDataString("pers") + player_id_str;
        }

        public void Log(Dictionary<string, string> data)
        {
            data["session_n"] = flush_index.ToString();
            // data[client_time] = (new Date()).toISOString().split('T').join(" ");

            flush_index++;
            accrued_log = new AccruedLog(data);
            Flush();
        }

        public void Flush()
        {
            if (flushing || accrued_log.Count() == 0) return;

            flushing = true;

            string post_url = req_url + "&req_id=" + Uri.EscapeDataString(UUIDint().ToString());

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(post_url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            
            HttpWebResponse res = (HttpWebResponse)req.GetResponse(); 

            int flushed = Int32.Parse(accrued_log.Data[accrued_log.Count() - 1]["session_n"]);
            int cutoff = accrued_log.Count() - 1;

                for (var i = accrued_log.Count() - 1; i >= 0 && Int32.Parse(accrued_log.Data[i]["session_n"]) > flushed; --i) 
                {
                    cutoff = i - 1;
                }

                if (cutoff >= 0) 
                {
                    Splice(accrued_log.Data, 0, cutoff + 1);
                }

                flushing = false;

            string post = "data=" + Uri.EscapeDataString(btoa(Serializer.Write<AccruedLog>(accrued_log)));

            
            res.Close();
        }

        // From https://stackoverflow.com/questions/28833373/javascript-splice-in-c-sharp
        private List<T> Splice<T>(List<T> source, int index, int count)
        {
            var items = source.GetRange(index, count);
            source.RemoveRange(index, count);
            return items;
        }

        // From https://stackoverflow.com/questions/46093210/c-sharp-version-of-the-javascript-function-btoa
        private string btoa(string str)
        {
            byte[] bytes = Encoding.GetEncoding(28591).GetBytes(str);
            string str64 = System.Convert.ToBase64String(bytes);
            return str64;
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

        private string GetCookie(string name)
        {
            return "";
        }

        private string SetCookie(string name, int val, int days)
        {
            DateTime dt = DateTime.Now;
            //dt.Add(dt.TimeOfDay + (dt.Day * 24 * 60 * 60 * 1000));
            return "";
        }
    }
}

