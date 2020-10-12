using System.Collections.Generic;
using BeauData;

namespace Logging
{
    public class LogData : ISerializedObject
    {
        private Dictionary<string, string> data = new Dictionary<string, string>();

        public Dictionary<string, string> Data 
        {
            get { return data; }
            set { data = value; }
        }

        public LogData(Dictionary<string, string> data)
        {
            this.data = data;
        }

        public void Serialize(Serializer ioSerializer)
        {
            ioSerializer.Map("data", ref data);
        }
    }
}

