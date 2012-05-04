using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQObjectMapper.Impl
{

    /// <summary>
    /// A class to encapsulate fast access to reflected properties.
    /// We can't create strongly typed delgates to field get/set accessors, so they are still
    /// cast.
    /// </summary>
    public abstract class DelegateInfo: IDelegateInfo
    {
        #region constructors

        public DelegateInfo()
        {
            ConfigureTypeSpecificProperties();
        }
        public DelegateInfo(Delegate getDel, Delegate setDel)
        {
            GetDelegate = getDel;
            SetDelegate = setDel;
            ConfigureTypeSpecificProperties();
        }
        public void SetDelegates(Func<object, object> getDel, Action<object, object> setDel)
        {
            GetDelegate = getDel;
            SetDelegate = setDel;
            ConfigureTypeSpecificProperties();

        }

        #endregion

        #region private properties

        private Delegate _GetDelegate;

        #endregion

        #region public properties

        public string Name { get; set; }
        public int Index { get; set; }
        public abstract Type ValueType {get;}
        public abstract Type ObjectType { get; }

        public virtual Delegate GetDelegate
        {
            get
            {
                return _GetDelegate;
            }
            protected set
            {
             
                _GetDelegate = value;
            }
        }
        public virtual Delegate SetDelegate {get; protected set;}

        // Properties that describe the delegate

        public bool IsObjectTarget { get; set; }   
        public bool IsProperty { get; set; }
        public bool IsField { get;  set; }
        public bool IsPrivate { get;  set; }
        public bool IsDeclared { get;  set; }
        public bool IsValueTarget { get;  set; }
        public bool IsNullable { get;  set; }
        public bool IsReadOnly { get;  set; }
        public bool IgnoreNull { get;  set; }
        public bool HasPublicGetter { get;  set; }
        public bool? Include { get; set; }

        #endregion

        #region public methods

        public abstract object GetValue(object source);
       
        public abstract void SetValue(object source, object value);

        // The strongly-typed methods exposed here are just for testing
        // It should automatically use the ones from the typed implementation

        public T GetValueT<U, T>(U obj)
        {
            return ((Func<U, T>)GetDelegate)(obj);
        }
        public void SetValueT<U, T>(U obj, T value)
        {
            ((Action<U, T>)SetDelegate)(obj, value);
        }
        #endregion

        #region private methods

        // The typed implementation must implement these to refer to the strongly typed functions
        // used to create it

        //protected abstract object GetValueImpl(object source);
        //protected abstract void SetValueImpl(object source, object value);

        protected void ConfigureTypeSpecificProperties()
        {
            IsValueTarget = Types.IsValueTarget(ValueType);
            IsObjectTarget = Types.IsObjectTarget(ValueType);
            IsNullable = Types.IsNullableType(ValueType);
        }

        #endregion

    }
}

