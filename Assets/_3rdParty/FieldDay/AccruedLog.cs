using System.Collections.Generic;
using BeauData;

namespace FieldDay
{
    public class AccruedLog : ISerializedObject
    {
        private List<Dictionary<string, string>> logData = new List<Dictionary<string, string>>();
        private List<string> dataStrings = new List<string>();

        public void Serialize(Serializer ioSerializer)
        {
            ioSerializer.Array("logData", ref dataStrings);
        }

        public int Count()
        {
            return dataStrings.Count;
        }

        public List<string> DataStrings
        {
            get { return dataStrings; }
            set { dataStrings = value; }
        }

        public List<Dictionary<string, string>> LogData
        {
            get { return logData; }
            set { logData = value; }
        }
    }
}
