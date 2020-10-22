using System.Collections.Generic;

namespace FieldDay 
{
    public class LogEvent : ILogEvent
    {
        public Dictionary<string, string> Data { get; set; }

        public LogEvent(Dictionary<string, string> data)
        {
            Data = data;
        }
    }
}
