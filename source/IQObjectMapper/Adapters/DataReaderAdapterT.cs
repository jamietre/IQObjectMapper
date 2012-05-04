using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQObjectMapper.Adapters
{
    /// <summary>
    /// Converts an IDataReader to a sequence of strongly-typed objects
    /// </summary>
    public class DataReaderAdapter<T> : TypedAdapter<T>, IDisposable
    {

        public DataReaderAdapter(IDataReader reader, IMapOptions options=null): 
            base(new DataReaderAdapter(reader),options)
        {
            InnerDataReader = reader;

        }
        IDataReader InnerDataReader;

        public void Dispose()
        {
            InnerDataReader.Dispose();
        }
    }
   

}
