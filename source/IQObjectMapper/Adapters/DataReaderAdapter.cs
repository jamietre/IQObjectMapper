using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQObjectMapper.Impl;

namespace IQObjectMapper.Adapters
{
    /// <summary>
    /// Converts an IDataReader to IEnumerableKVP, a general-purpose interface that allows easy access
    /// to a variety of data structures. This structure is assignable from IDictionary-String,Object- and is
    /// also the internal data structore for ExpandoObjects. As such it's also representative of the structure
    /// of POCO objects (a list of property/object combinations). By mapping to this structure as a generic
    /// format we can create adapters for most anything that will be compatible with IQObjectMapper.
    /// </summary>
    public class DataReaderAdapter : IEnumerable<IEnumerable<KeyValuePair<string, object>>>, IDisposable
    {
        public DataReaderAdapter(IDataReader reader)
        {
            InnerReader = reader;
        }

        IDataReader InnerReader;
        
        public IEnumerator<IEnumerable<KeyValuePair<string, object>>> GetEnumerator()
        {
            return new DataReaderEnumerator(InnerReader);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            InnerReader.Dispose();
        }
    }


}
