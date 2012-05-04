using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Data;


using IQObjectMapper.Impl;

namespace IQObjectMapper.Adapters
{

    public class ValueDelegate
    {
        /// <summary>
        /// Create a delegate for a real property
        /// </summary>
        /// <param name="key"></param>
        public ValueDelegate(Func<object> getter, Action<object> setter, bool readOnly) {
            Getter = getter;
            Setter = readOnly ?
                SetInternalFail :
                setter;
            IsProperty = true;
        }
        /// <summary>
        /// Create a delegate for a stored value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="readOnly"></param>
        public ValueDelegate(string key,object value,bool readOnly)
        {
            Value = value;
            IsProperty = false;

            if (readOnly)
            {
                Setter = SetInternalFail;  
            } else {
                Setter = SetInternal;
            }

            Getter = GetInternal;
        }
        public bool IsProperty;
        public object Value;
        public Func<object> Getter;
        public Action<object> Setter;

        private object GetInternal() {
            return Value;
        }
        private void SetInternal(object value) {
            Value=value;
        }

        private static void SetInternalFail(object value) {
            throw new Exception("Updating values is prohibited by IsReadOnly=false");
        }

    }
    /// <summary>
    /// Expose the properties of an object as IDictionary-string,object-. Changes to the dictionary
    /// (if allowed) will affect the underlying bound object.
    /// </summary>
    public class PropertyDictionaryAdapter : 
        IDictionary<string, object>, 
        IEnumerable<KeyValuePair<string,object>>
    {

        #region constructor

        /// <summary>
        /// When UpdateSource is true, the underlying object will be updated to reflect dictionary changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="updateSource"></param>
        /// <param name="readOnly"></param>
        /// <param name="options"></param>
        public PropertyDictionaryAdapter(object obj, IMapOptions options=null)
        {
            Options = MapOptions.From(options);
            InnerObject = obj;
            Initialize();
        }
        #endregion

        #region private properties
        
        protected object InnerObject;

        // normally I would used lazy initialization for these things but this class needs to iterate
        // the properties as soon as it's created to map which ones are part of the underlying object.
        // each get/set operation would require an extra test on up to 3 objects to do this lazily, 
        // not worthwhile.

        protected IClassInfo ClassInfo;
        protected IDictionary<string, IDelegateInfo> ClassInfoData;
        protected IDictionary<string, ValueDelegate> UserData;

        #endregion

        #region public properties

        public MapOptions Options { get; set; }

        #endregion

        #region IDictionary members

        public void Add(string key, object value)
        {
            UserData.Add(key, GetValueDelegate(key,value,true));
        }

        public bool ContainsKey(string key)
        {
            return UserData.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get {
                return UserData.Keys;
            }
        }

        public bool Remove(string key)
        {
            
            return Options.CanAlterProperties ?
                UserData.Remove(key) :
                (bool)Unallowed<InvalidOperationException>("Removing properties is prohibited by CanAlterProperties=false");
        }

        public bool TryGetValue(string key, out object value)
        {
            ValueDelegate del;
            if (UserData.TryGetValue(key, out del)) {
                value = del.Getter();
                return true;
            } else {
                value = null;
                return false;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                IList<object> values = new List<object>();
                foreach (var key in UserData.Keys)
                {
                    values.Add(UserData[key].Getter());
                }
                return values;
            }
        }

        public object this[string key]
        {
            get
            {
                try
                {
                    return UserData[key].Getter();
                }
                catch(KeyNotFoundException)
                {
                    return Options.CanAccessMissingProperties ?
                        Options.UndefinedValue :
                        Unallowed<KeyNotFoundException>("The key was not present in the dictionary.");
                }
            }
            set
            {
                try
                {
                    UserData[key].Setter(value);
                }
                catch(KeyNotFoundException)
                {
                    Add(key, value);
                }
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key,item.Value);
        }

        public void Clear()
        {
            if (Options.CanAlterProperties) {
                UserData.Clear();
            } else {
                Unallowed<InvalidOperationException>("Removing properties is prohibited by CanAlterProperties=false");
            }
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            ValueDelegate del;
            if (UserData.TryGetValue(item.Key, out del)) {
                var value = del.Getter();

                if ((item.Value == null && value == null) ||
                    (item.Value.Equals(value)))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            int index=0;
            foreach (var key in UserData.Keys)
            {
                var value = UserData[key].Getter();
                array[index + arrayIndex] = new KeyValuePair<string, object>(key, value);
            }
        }

        public int Count
        {
            get { return UserData.Count; }
        }

        /// <summary>
        /// When false, changes are not allowed to the underlying data
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return Options.IsReadOnly;
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Options.CanAlterProperties && Contains(item) ?
                Remove(item.Key) : 
                false;
            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return UserDataEnumerable().GetEnumerator();
        }
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return UserDataEnumerable().GetEnumerator();
        }


        #endregion

        #region private methods

        /// <summary>
        /// Enumerate the data from both teh
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<KeyValuePair<string, object>> UserDataEnumerable()
        {
            foreach (var key in UserData.Keys)
            {
                yield return new KeyValuePair<string, object>(key, UserData[key].Getter());

            }
        }

        /// <summary>
        /// Set the underlying object value through it's delegate
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        //protected void Set(string key, object value)
        //{
        //    if (!Options.UpdateSource)
        //    {
        //        return;
        //    }
        //    var fldInfo = ClassInfoData[key];
        //    object finalVal;
        //    if (Options.ParseValues && 
        //        !fldInfo.Type.IsAssignableFrom(value.GetType())) {
        //            finalVal = Types.Parse(value,fldInfo.Type);
        //    } else {
        //        finalVal = value;
        //    }
        //    fldInfo.SetValue(InnerObject, finalVal);
        //}
        /// <summary>
        /// Set either the local value, or the underlying value if the key represents a property/field
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        //protected void SetAny(string key, object value)
        //{

        //    if (RealProps.Contains(key) && Options.UpdateSource)
        //    {
        //        IDelegateInfo del=ClassInfoData[key];
        //        del.SetValue(InnerObject,
        //            Options.ParseValues ?
        //                Types.Parse(value,del.Type) :
        //                value);
                     
        //    } else {
        //        UserData[key] = value;
        //    }


        //        //// try/catch to optimize accessing missing properties. 
        //        //object dictValue;
        //        //try
        //        //{
        //        //    dictValue = UserData[key];
        //        //}
        //        //catch
        //        //{
        //        //    // will just be added to dictionary
        //        //    dictValue = null;
        //        //}
        //        //IDelegateInfo del = dictValue as IDelegateInfo;
        //        //if (del!=null)
        //        //{
        //        //    if (Options.ParseValues &&
        //        //        !del.Type.IsAssignableFrom(value.GetType()))
        //        //    {
        //        //        del.SetValue(InnerObject, Types.Parse(value, del.Type));
        //        //    }
        //        //    else
        //        //    {
        //        //        del.SetValue(InnerObject, value);
        //        //    }
        //        //} else {
        //        //    UserData[key] = value;
        //        //}

        //        //object userValue;
        //        //if (UserData.TryGetValue(key, out userValue)
        //        //    && userValue == Undefined.Value)
        //        //{
        //        //    Set(key, value);
        //        //}
        //        //else
        //        //{
        //        //    UserData[key] = value;
        //        //}
        //   //}
        //}
        /// <summary>
        /// Get the underlying object value through it's delegate
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //protected object Get(string key) {
        //    return ClassInfoData[key].GetValue(InnerObject);
        //}
        /// <summary>
        /// Get either the local value, or the underlying value if the key represents a prop/field
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //protected object GetAny(string key)
        //{

        //    if (RealProps.Contains(key))
        //    {
        //        return ClassInfoData[key].GetValue(InnerObject);
        //    }
        //    else
        //    {
        //        object value;
        //        return UserData.TryGetValue(key, out value) ?
        //            value :
        //            Options.CanAccessMissingProperties ?
        //                Options.UndefinedValue :
        //                new Func<object>(() =>
        //                {
        //                    throw new KeyNotFoundException(String.Format("The key {0} was not found in the collection.", key));
        //                });
        //    }
        //        //try
        //        //{
        //        //    value = UserData[key];
        //        //}
        //        //catch
        //        //{
        //        //    if (Options.CanAccessMissingProperties)
        //        //    {
        //        //        value = Options.UndefinedValue;
        //        //    }
        //        //    else
        //        //    {
        //        //        throw new KeyNotFoundException(String.Format("The key {0} was not found in the collection.", key));
        //        //    }
        //        //}

        //    //}
        //    //if (value != null && value is IDelegateInfo)
        //    //{
        //    //    return ((IDelegateInfo)value).GetValue(InnerObject);
        //    //}
        //    //else
        //    //{
        //    //    return value;
        //    //}

        //    //var value = UserData[key];
        //    //if (value == Undefined.Value)
        //    //{
        //    //    return Get(key);
        //    //}
        //    //else
        //    //{
        //    //    return value;
        //    //}

        //}
           /// <summary>
        /// Populate the user dictionary with a stub
        /// </summary>
        protected void Initialize()
        {
            
            ClassInfo = ObjectMapper.MapperCache.GetClassInfo(InnerObject.GetType(), Options);
            ClassInfoData = (IDictionary<string, IDelegateInfo>)ClassInfo.Data;
            //UserData = ObjectMapper.MapperCache.GetDictionary<ValueDelegate>(Options, InitialDictionaryElements());
            UserData = new Dictionary<string,ValueDelegate>(ObjectMapper.MapperCache.GetStringComparer(Options));
            foreach (var item in InitialDictionaryElements()) {
                UserData.Add(item);
            }
            Options.OnChange = ChangeProp;

        }
        protected void ChangeProp(string name) {
            if (name=="ParseValues") {
                foreach (string key in ClassInfoData.Values.Select(item => item.Name))
                {
                    UserData.Remove(key);
                }
            }
            foreach (var kvp in InitialDictionaryElements())
            {
                UserData.Add(kvp);
            }
        }
        /// <summary>
        /// Data to populate the dictionary on creation - just the names of each property
        /// </summary>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<string,ValueDelegate>> InitialDictionaryElements()
        {
            // for fields that are part of the class, keep direct refs to their getters/setters
            // to avoid redundant lookups.
            foreach (var fldInfo in ClassInfoData.Values)
            {
                yield return new KeyValuePair<string,ValueDelegate>(fldInfo.Name,GetObjectDelegate(fldInfo.Name));
               // var delegate = GetValueDelegate(fldInfo.Name,fldInfo.);
               // yield return new KeyValuePair<string, object>(fldInfo.Name,fldInfo);
            }
        }

        private object Unallowed<T>(string message) where T: Exception, new()
        {
            T exception = (T)Activator.CreateInstance(typeof(T), message);
            throw exception;
        }
        private ValueDelegate GetObjectDelegate(string key) {
            
            var del = ClassInfoData[key];

            Func<object> getFunc = new Func<object>(() =>
            {
                return del.GetValue(InnerObject);
            });

            Action<object> setFunc = Options.ParseValues ?
                new Action<object>((value) =>
                {
                    if (value==null || del.Type.IsAssignableFrom(value.GetType())) {
                        del.SetValue(InnerObject, value);
                    } else {
                        del.SetValue(InnerObject, Types.Parse(value, del.Type));
                    }
                }):
                new Action<object>((value) =>
                {
                    del.SetValue(InnerObject, value);
                });

            

            return new ValueDelegate(
                getFunc,
                setFunc,
                IsReadOnly);
            
        }
        private ValueDelegate GetValueDelegate(string key,object value,bool isNew)
        {


            if (isNew && !Options.CanAlterProperties) {
                throw new InvalidOperationException("Adding properties is prohibited by CanAlterProperties=false");
            }
            return new ValueDelegate(key,value,Options.IsReadOnly);
        }}
        #endregion

}
