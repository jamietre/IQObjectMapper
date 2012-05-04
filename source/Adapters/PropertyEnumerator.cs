using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

using IQObjectMapper.Impl;

namespace IQObjectMapper.Adapters
{
    public class PropertyEnumerator : IEnumerator<KeyValuePair<string,object>>
    {
        #region constructors

        public PropertyEnumerator(object source)
        {
            Source = source;
        }
        
        #endregion

        #region private properties

        private object _Source;
        private Type Type;
        protected int CurrentIndex = -1;
        protected int ItemCount=-1;
        protected IEnumerator<KeyValuePair<string,object>> CurrentItem;
        protected IEnumerable<KeyValuePair<string, object>> InnerDict;

        protected object Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = value;
                Type = value.GetType();
                if (!Types.IsMappableClass(Type))
                {
                    throw new Exception("The type \"{0}\" cannot be mapped, it is a value or value-like type.");

                }
                if (typeof(IEnumerable<KeyValuePair<string, object>>).IsAssignableFrom(value.GetType()))
                {
                    InnerDict = (IEnumerable<KeyValuePair<string, object>>)value;
                    CurrentItem = InnerDict.GetEnumerator();
                }
                else if (Types.IsAnonymousType(Type))
                {
                    CurrentItem = ReflectAnonType().GetEnumerator();
                }
                else
                {
                    CurrentItem = ReflectPoco().GetEnumerator();

                }
            }
        }

        protected BindingFlags BindingFlags
        {
            get {
                return BindingFlags.Instance |
                    (IncludePrivate ? BindingFlags.Public : 0) |
                    (DeclaredOnly ? BindingFlags.DeclaredOnly : 0);

            }
        }

        #endregion

        #region public properties

        public bool Deep { get; set; }
        public bool IncludeProperties { get; set; }
        public bool IncludeFields { get; set; }
        public bool IncludePrivate { get; set; }
        public bool DeclaredOnly { get; set; }

        #endregion

        #region public methods


        public bool MoveNext()
        {
            return CurrentItem.MoveNext();
        }

        public void Reset()
        {
            CurrentItem.Reset();
        }
        public KeyValuePair<string, object> Current
        {
            get
            {
                return CurrentItem.Current;
            }
        }

        public void Dispose()
        {
            CurrentItem.Dispose();
        }

        #endregion

      
        protected IEnumerable<KeyValuePair<string, object>> ReflectAnonType()
        {
            IEnumerable<MemberInfo> members = Type.GetMembers(BindingFlags);
            foreach (var member in members)
            {
                string name = member.Name;
                MethodInfo info = (MethodInfo)member;
                if (name.Length >= 4 && name.StartsWith("get_"))
                {
                    yield return new KeyValuePair<string, object>(info.Name, 
                        ParseSubObjects(info.Invoke(_Source, null)));
                }
            }
        }
        protected IEnumerable<KeyValuePair<string,object>> ReflectPoco() {
            
            object obj = Source;
            object value = null;
            
            IEnumerable<MemberInfo> members = Type.GetMembers(BindingFlags);

            foreach (var member in members)
            {
                string name = member.Name;

             if (member is PropertyInfo && IncludeProperties)
                {
                    PropertyInfo propInfo = (PropertyInfo)member;
                    if (propInfo.GetIndexParameters().Length == 0 &&
                        propInfo.CanRead)
                    {
                        // wrap this because we are testing every single property - if it doesn't work we don't want to use it
                        try
                        {
                            value = ((PropertyInfo)member).GetGetMethod().Invoke(obj, null);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                else if (member is FieldInfo && IncludeFields)
                {
                    value = ((FieldInfo)member).GetValue(obj);
                }
                else
                {
                    continue;
                }
                yield return new KeyValuePair<string, object>(name, ParseSubObjects(value));
               
            }

        }
        /// <summary>
        /// Deal with "deep" by recursing inner containers
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected object ParseSubObjects(object value)
        {
            throw new NotImplementedException();
            if (!Deep || value == null || !Types.IsObjectTargetObject(value))
            {
                return value;
            }
            else if (value.GetType().IsArray)
            {


            }
            else if (value is IEnumerable && !(value is IEnumerable<KeyValuePair<string, object>>))
            {



            }

            else if (value is IEnumerable<KeyValuePair<string, object>>)
            {
                var subObj = new PropertyDictionaryAdapter(value);
                return subObj;
            }

            else if (value is IEnumerable)
            {
                //newValue = MapFromEnumerable(fldInfo, (IEnumerable)value);
            }


            else
            {
                //Map(value, true);
            }
                
        }
        // TODO: create a list of a type common to all elements in the list.
        protected object MapToList(IEnumerable source)
        {
            throw new NotImplementedException();
            //List<object> list = new List<object>();
            //Type oneType = null;
            //bool typed = false;

            //foreach (var el in source)
            //{
            //    if (!typed)
            //    {
            //        if (el == null)
            //        {
            //            oneType = typeof(object);
            //        }
            //        else
            //        {
            //            typed = true;
            //            oneType = el.GetType();
            //        }
            //    }
            //    else if (typed)
            //    {
            //        if (typed(object))
            //        {
            //            oneType = typeof(object);
            //            typed = false;
            //        }
            //    }
            //    list.Add(json);
            //}
            //if (typed)
            //{
            //    Type listType = typeof(List<>).MakeGenericType(new Type[] { oneType });
            //    IList typedList = (IList)Activator.CreateInstance(listType);
            //    foreach (var item in list)
            //    {
            //        typedList.Add(item);
            //    }
            //    return typedList;
            //}
            //else
            //{
            //    return list;
            //}
        }

      

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }

        }

        #region private mathods

        #endregion

    }

}
