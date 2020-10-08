using System.Collections.Generic;
using BeauData;

namespace Logging
{
    public class AccruedLog : ISerializedObject
    {
        private List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

        public AccruedLog(Dictionary<string, string> data)
        {
            this.data.Add(data);
        }

        public void Serialize(Serializer ioSerializer)
        {

        }

        public int Count()
        {
            return data.Count;
        }

        public List<Dictionary<string, string>> Data
        {
            get { return data; }
        }
    }
}
