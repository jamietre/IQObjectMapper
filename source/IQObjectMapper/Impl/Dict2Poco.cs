using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace IQObjectMapper.Impl
{
    /// <summary>
    /// Maps a dictionary to an object
    /// </summary>
    public class Dict2Poco
    {

        public Dict2Poco(IMapOptions options=null)
        {
            Options = MapOptions.From(options);
        }

        public MapOptions Options
        {
            get;
            set;
        }
        private Object2Object _O2O;
        protected Object2Object O2O 
        {
            get
            {
                if (_O2O == null)
                {
                    _O2O = new Object2Object(Options);
                }
                return _O2O;
            }
        }
        /// <summary>
        /// Map a dictionary-type object to a strongly typed object.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Map(IEnumerable<KeyValuePair<string, object>> source, Type type)
        {
            object target= type==typeof(object) || type == typeof(IDynamicMetaObjectProvider) ?
                Types.GetInstanceOf(Options.DynamicObjectType) :
                Types.GetInstanceOf(type);
            Map(source, target);
            return target;

        }
        public void Map(IEnumerable<KeyValuePair<string, object>> source, object target)
        {
            if (Types.IsValueTarget(target.GetType()))
            {
                throw new Exception("You can't map from an object to a value type. Use Types.Convert instead.");
            }
            bool targetIsDict = target is IEnumerable<KeyValuePair<string, object>>;

            IClassInfo classInfo = null;

            IDictionary<string, object> targetDict = null;

            if (targetIsDict)
            {
                targetDict = (IDictionary<string, object>)target;
            }
            else
            {
                classInfo = ObjectMapper.MapperCache.GetClassInfo(target.GetType(),Options);
            }
            foreach (var kvp in source)
            {
                if (targetIsDict)
                {
                    // Mapping a dict to a dict. Inner dictionaries are converted to dynamics

                    if (kvp.Value is IEnumerable<KeyValuePair<string, object>>)
                    {
                        var subValue = Types.GetInstanceOf(Options.DynamicObjectType);
                        Map((IEnumerable<KeyValuePair<string, object>>)kvp.Value,subValue);
                        targetDict[kvp.Key] = subValue;
                    } 
                    else
                    {
                        targetDict[kvp.Key] = kvp.Value;
                    }
  
                }
                else
                {
                    IDelegateInfo fldInfo;
                    if (classInfo.TryGetValue(kvp.Key, out fldInfo))
                    {
                        object value = O2O.Map(kvp.Value, fldInfo.Type);
                        if (value != Undefined.Value)
                        {
                            fldInfo.SetValue(target, value);
                        }
                    }
                   
                }
            }

        }
    }
}
