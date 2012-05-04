using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace IQObjectMapper.Impl
{

    /// <summary>
    /// Represents the structure of a database-bound class. The ClassInfo object i
    /// </summary>
    [Serializable]
    public class ClassInfo : IClassInfo
    {
        #region constructor

        public ClassInfo()
        {
        }

        #endregion

        #region private properties

        private Type _Type;
        public virtual Type Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (Types.IsValueType(value) || !value.IsClass)
                {
                    throw new Exception("You can only map to strongly typed classes (and not Object).");
                }

                _Type = value;
            }
        }
        public IClassInfoData Data { get; set; }

        #endregion

        #region public properties


        public virtual IEnumerable<IDelegateInfo> Fields
        {
            get
            {
                return Data.Values;
            }
        }

        public virtual IEnumerable<string> FieldNames
        {
            get
            {
                return Data.Keys;
            }

        }

        public virtual bool TryGetValue(string fieldName, out IDelegateInfo info)
        {
            return Data.TryGetValue(fieldName, out info);
        }

        public virtual IDelegateInfo this[string fieldName]
        {
            get
            {
                return Data[fieldName];
            }
        }

        #endregion

        #region public methods


        
        public bool ContainsField(string fieldName)
        {
            return Data.ContainsKey(fieldName);
        }
        public int Count
        {
            get { return Data.Count; }
        }

        #endregion

    }
}

