using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQObjectMapper.Adapters
{
    /// <summary>
    /// Enumerate an IDataReader as an IEumerableKVP
    /// </summary>
    class DataReaderEnumerator : IEnumerator<IEnumerable<KeyValuePair<string, object>>>, IDisposable
    {
        public DataReaderEnumerator(IDataReader reader)
        {
            if (reader.IsClosed)
            {
                throw new Exception("The IDataReader provided to DataReaderEnumerator must be open.");
            }
            Reader = reader;
            CurrentIndex = -1;
        }

        protected IDataReader Reader;
        protected int CurrentIndex;

        public IEnumerable<KeyValuePair<string, object>> Current
        {
            get
            {
                return new DataRecordAdapter(Reader);
            }
        }

        public void Dispose()
        {
            Reader.Dispose();
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            CurrentIndex++;
            return Reader.Read();

        }

        public void Reset()
        {
            if (CurrentIndex >= 0)
            {
                throw new Exception("The DataReader cannot be reset.");
            }
        }
    }
}
