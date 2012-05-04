using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQObjectMapper
{
    /// <summary>
    /// Strongly typed interface to setters/getters. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    //public interface IDelegateInfo<T, U> : IDelegateInfo where T : class
    //{
        //U GetValue(T source);
        //void SetValue(T source, U value);
    //}

    public interface IDelegateInfo
    {
        string Name { get; }
        int Index { get; }
        Type Type { get; }
        Type OwnerType { get; }

        Delegate GetDelegate { get; }
        Delegate SetDelegate { get; }

        object GetValue(object source);
        void SetValue(object source, object value);

        /// <summary>
        /// When true, indicates that the type of this field can be re-mapped, e.g.
        /// it's something with properties
        /// </summary>
        /// <returns></returns>
        bool IsObjectTarget { get; }

        bool IsProperty { get; }
        bool IsField { get; }
        bool IsPrivate { get; }
        bool IsDeclared { get; }
        bool IsNullable { get; set; }
        bool IsReadOnly { get; set; }
        bool IgnoreNull { get; set; }

        bool IsValueTarget { get; }
        bool HasPublicGetter { get; }
        
        /// <summary>
        /// When set by a derived class, the delegate will be explicitly included or excluded
        /// </summary>
        bool? Include { get; set; }

        // for testing only

        //T GetValueT<U, T>(U obj);
        //void SetValueT<U, T>(U obj, T value);

    }


}
