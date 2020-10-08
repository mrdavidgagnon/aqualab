using System.Collections.Generic;
using UnityEngine;

namespace Logging
{
    public class Logger : MonoBehaviour
    {
        private void Awake()
        {
            SimpleLog slog = new SimpleLog("AQUALAB", 0);

            Dictionary<string, string> data = new Dictionary<string, string>();
            data["test"] = "test";

            //slog.Log(data);
        }
    }
}
