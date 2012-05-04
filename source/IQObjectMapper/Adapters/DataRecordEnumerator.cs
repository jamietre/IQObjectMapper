using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQObjectMapper.Adapters
{

    /// <summary>
    /// Exposes an enumerator for the elements in an IDataRecord object
    /// </summary>
    public class DataRecordEnumerator : IEnumerator<KeyValuePair<string, object>>
    {
        public DataRecordEnumerator(IDataRecord record)
        {
            Record = record;
            Length = record.FieldCount;
        }

        #region private properties

        protected IDataRecord Record;
        protected int CurrentIndex = -1;
        protected int Length;

        #endregion

        #region public properties

        public KeyValuePair<string, object> Current
        {
            get
            {
                return new KeyValuePair<string, object>(Record.GetName(CurrentIndex), Record.GetValue(CurrentIndex));
            }
        }

        public void Dispose()
        {

        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return ++CurrentIndex < Length;
        }

        public void Reset()
        {
            CurrentIndex = -1;
        }

        #endregion
    }
}
