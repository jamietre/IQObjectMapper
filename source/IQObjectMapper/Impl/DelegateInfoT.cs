using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQObjectMapper.Impl
{

    public class DelegateInfo<T, U> : DelegateInfo, IDelegateInfo<T, U> where T: class
    {
        #region constructors

        public DelegateInfo(Func<T, object, U> getDel, Action<T, U, object> setDel)
            : base(getDel, setDel)
        {

        }

        public DelegateInfo(Func<T, U> getDel, Action<T,U> setDel)
            : base(getDel, setDel)
        {

        
        }
        public DelegateInfo(Func<object, object> getDel, Action<object, object> setDel)
            : base(getDel, setDel)
        {

        }

        #endregion

        #region private properties

        private Func<T, U> GetDelegateFunc;
        private Action<T, U> SetDelegateFunc;
        
        #endregion

        #region public properties

        public override Type Type
        {
            get
            {
                return typeof(U);
            }
        }

        public override Delegate GetDelegate
        {
            get
            {
                return GetDelegateFunc;
            }
            protected set
            {
                if (value == null)
                {
                    GetDelegateFunc = new Func<T, U>((source) =>
                    {
                        throw new Exception(String.Format("There is no getter for the {0} property.",Name));
                    });
                    return;
                }

                Type type = value.GetType();
                
                if (type == typeof(Func<T, object, U>))
                {
                    GetDelegateFunc = new Func<T, U>((source) =>
                    {
                        return ((Func<T, object, U>)value)(source, null);
                    });                        
                }
                else if (type == typeof(Func<T, U>))
                {
                    GetDelegateFunc = (Func<T, U>)value;
                }
                else if (type == typeof(Func<object, object>))
                {
                    GetDelegateFunc = new Func<T, U>((source) =>
                    {
                        return (U)((Func<object, object>)value)(source);
                    });
                }
                else
                {
                    throw new Exception("The type of the delegate passed is not valid.");
                }
            }
        }

        public override Delegate SetDelegate
        {
            get
            {
                return SetDelegateFunc;
            }
            protected set
            {
                if (value == null)
                {
                    SetDelegateFunc = new Action<T, U>((source,val) =>
                    {
                        throw new Exception(String.Format("There is no setter for the {0} property.", Name));
                    });
                    return;
                }
                if (value.GetType() == typeof(Action<T, U,object>))
                {
                    SetDelegateFunc = new Action<T, U>((source,val) =>
                    {
                        ((Action<T, U,object>)value)(source, val, null);
                    });
                }
                else if (value.GetType() == typeof(Action<T, U>))
                {
                    SetDelegateFunc = (Action<T, U>)value;
                }
                else
                {
                    SetDelegateFunc = new Action<T, U>((source,val) =>
                    {
                        ((Action<object, object, object>)value)(source,val,null);
                    });

                }
            }
        }

        #endregion

        #region public methods
        
        // Strongly typed version of the Get/Set functions.

        public U GetValue(T source)
        {
            return GetDelegateFunc(source);
        }
        public void SetValue(T source, U value)
        {
            SetDelegateFunc(source, value);
        }

        #endregion

        #region private members

        // These overrides ensure that when a nontyped cast is called for a typed instance, the typed functions
        // are used

        protected override object GetValueImpl(object source)
        {
            return GetDelegateFunc((T)source);

        }

        protected override void SetValueImpl(object source, object value)
        {
            SetDelegateFunc((T)source, (U)value);
        }

        #endregion

    }
}

