using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using BeauData;
using BeauUtil;
using UnityEngine;
using UnityEngine.Networking;

namespace FieldDay
{
    /// <summary>
    /// Simple Logger
    /// </summary>
    public class SimpleLog
    {
        private string playerId;
        Regex playerIdRegex = new Regex("/^([a-zA-Z][0-9]{3})$/");

        private bool flushing = false;
        private AccruedLog accruedLog = new AccruedLog();
        private int flushedTo = 0;
        private int flushIndex = 0;

        private string appId;
        private int appVersion;
        private long sessionId;
        private string persistentSessionId;
        private Cookie cookie = new Cookie();

        private string reqUrl;

        public SimpleLog(string inAppId, int inAppVersion)
        {
            appId = inAppId;
            appVersion = inAppVersion;

            //QueryParams queryParams = Services.Data.PeekQueryParams();
            //playerId = queryParams.Get("player_id");

            if (playerId != null && playerIdRegex.IsMatch(playerId))
            {
                // Application.OpenUrl("https://fielddaylab.wisc.edu/studies/" + Uri.EscapeDataString(appId.ToLower()))
                playerId = null;
            }

            sessionId = SimpleLogUtils.UUIDint();
            persistentSessionId = SimpleLogUtils.GetCookie(cookie, "persistent_session_id");

            if (persistentSessionId == null)
            {
                persistentSessionId = sessionId.ToString();
                SimpleLogUtils.SetCookie(cookie, "persistent_session_id", Int64.Parse(persistentSessionId), 100);
            }

            string playerIdStr = playerId != null ? "&player_id=" + Uri.EscapeDataString(playerId.ToString()) : "";
            reqUrl = "https://fielddaylab.wisc.edu/logger/log.php?app_id=" + Uri.EscapeDataString(appId) +
                        "&app_version=" + Uri.EscapeDataString(appVersion.ToString()) + "&session_id=" +
                        Uri.EscapeDataString(sessionId.ToString()) + "&persistent_session_id=" +
                        Uri.EscapeDataString(persistentSessionId) + playerIdStr;
        }

        /// <summary>
        /// Logs a new event.
        /// </summary>
        public void Log(ILogEvent inData)
        {
            inData.Data["session_n"] = flushIndex.ToString();
            inData.Data["client_time"] = DateTime.Now.ToString();
            flushIndex++;
            accruedLog.DataStrings.Add(SimpleLogUtils.BuildDataString(inData.Data));
            
            Flush();
        }

        /// <summary>
        /// Flushes all queued events
        /// </summary>
        public void Flush()
        {
            if (flushing || accruedLog.Count() == 0) return;
            flushing = true;

            string postUrl = reqUrl + "&req_id=" + Uri.EscapeDataString(SimpleLogUtils.UUIDint().ToString());

            // Send a POST request to https://fielddaylab.wisc.edu/logger/log.php with the proper content type
            UnityWebRequest req = new UnityWebRequest(postUrl, "POST");
            req.SetRequestHeader("Content-type", "application/x-www-form-urlencoded");

            // Write the AccruedLog to a JSON string, convert it to base64, and create a byte array from that base64 string
            // Stringify and ensure ASCII (?) from jowilder simplelog
            string post = "data=" + Uri.EscapeDataString(SimpleLogUtils.btoa(Serializer.Write<AccruedLog>(accruedLog)));
            byte[] postArray = Encoding.ASCII.GetBytes(post);

            UnityWebRequestAsyncOperation reqOperation = req.SendWebRequest();

            // TODO: Make sure this is called before connection ends
            reqOperation.completed += obj => 
            {
                Debug.Log(req.responseCode);
                /*
                int flushed = Int32.Parse(accruedLog.LogData[accruedLog.Count() - 1]["session_n"]);
                int cutoff = accruedLog.Count() - 1;

                for (var i = accruedLog.Count() - 1; i >= 0 && Int32.Parse(accruedLog.LogData[i]["session_n"]) > flushed; --i)
                {
                    cutoff = i - 1;
                }

                if (cutoff >= 0)
                {
                    SimpleLogUtils.Splice(accruedLog.LogData, 0, cutoff + 1);
                }

                flushing = false;
                */
            };
        }
    }

    public interface ILogEvent
    {
        Dictionary<string, string> Data { get; set; }
    }
}