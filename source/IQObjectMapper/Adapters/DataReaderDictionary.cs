using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQObjectMapper.Adapters
{
    /// <summary>
    /// Converts an IDataReader to IEnumerableKVP, a general-purpose interface that allows easy access
    /// to a variety of data structures. This structure is assignable from IDictionary-String,Object- and is
    /// also the internal data structore for ExpandoObjects. As such it's also representative of the structure
    /// of POCO objects (a list of property/object combinations). By mapping to this structure as a generic
    /// format we can create adapters for most anything that will be compatible with IQObjectMapper.
    /// </summary>
    public class DataReaderDictionary : IEnumerable<IDictionary<string, object>>, IDisposable
    {
        public DataReaderDictionary(IDataReader reader)
        {
            InnerDataReader = reader;

        }
        IDataReader InnerDataReader;

        IEnumerable<IDictionary<string, object>> DictionaryEnumerable()
        {
            var adapter = new DataReaderAdapter(InnerDataReader);

            foreach (IEnumerable<KeyValuePair<string, object>> row in adapter)
            {
                IDictionary<string, object> dict = new Dictionary<string, object>();
                foreach (var obj in row)
                {
                    dict[obj.Key] = obj.Value;
                }
                yield return dict;
            }
        }

        IEnumerator<IDictionary<string, object>> IEnumerable<IDictionary<string, object>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new DataReaderEnumerator(InnerDataReader);
        }

        public void Dispose()
        {
            InnerDataReader.Dispose();
        }

    }


}
