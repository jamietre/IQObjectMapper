using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQObjectMapper.Adapters
{
    /// <summary>
    /// Exposes an IDataRecord as a sequence of KeyValuePair-string,object- for each column
    /// </summary>
    public class DataRecordAdapter : IEnumerable<KeyValuePair<string, object>>
    {
        public DataRecordAdapter(IDataRecord record, IMapOptions options=null)
        {
            Record = record;
        }
        private IDataRecord Record;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new DataRecordEnumerator(Record);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
