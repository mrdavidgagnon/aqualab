using System.Collections.Generic;
using BeauData;

namespace Logging
{
    public class AccruedLog : ISerializedObject
    {
        private List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

        // TODO: What needs to be serialized here?
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
