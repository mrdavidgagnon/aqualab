using System.Collections.Generic;
using BeauData;

namespace Logging
{
    public class AccruedLog : ISerializedObject
    {
        private List<LogData> logData = new List<LogData>();

        public void Serialize(Serializer ioSerializer)
        {
            ioSerializer.ObjectArray("logData", ref logData);
        }

        public int Count()
        {
            return logData.Count;
        }

        public List<LogData> LogData
        {
            get { return logData; }
            set { logData = value; }
        }
    }
}
