using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace IQObjectMapper.Impl
{
    /// <summary>
    /// Maps a value or value-like type to another. This isn't intended to be restricted to value types; rather it deals with things that
    /// don't have their own properties (or don't act as if they do).
    /// 
    /// One of the general difficulties in property mapping is determining whether something should be treated like an object (e.g. and its
    /// properties enumerated) or a value (and the object itself perhaps enumerated, e.g. as in a list or array). This attempts to deal with
    /// that logic in the case of value-like targets (e.g. actual value types or array/list types)
    ///
    /// </summary>
    public class Object2Object
    {
        public Object2Object(IMapOptions options=null)
        {
            Options = MapOptions.From(options);
        }

        public MapOptions Options;

        /// <summary>
        /// Map a valuelike object to type T. T must be anything other than a POCO. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public object Map<T>(object source)
        {
            return (T)MapTypedField(source, typeof(T));
        }

        public object Map(object source, Type type)
        {
            return MapTypedField(source, type);
        }
       
        protected object MapTypedField(object value, Type type)
        {
            object newValue=Options.UndefinedValue;
            bool fail=false;
            if (value == System.DBNull.Value || value == null)
            {
                newValue = MapFromNull(value,type);
            }
            else
            {
                // Logic: if the value we are assigning is a true value type (or value-like type such as a byte array or string)...
                if (Types.IsValueTargetObject(value)) 
                {
                    // and the target is also a value type, then try to convert them
                    if (Types.IsValueTarget(type))
                    {
                        if (Options.ParseValues)
                        {
                            fail = !Types.TryParse(value,type,out newValue,Options.UndefinedValue);
                        }
                        else 
                        {
                            fail = !Types.TryChangeType(value, type, out newValue);
                        }
                    }
                    // or if the target is just something general enough to accept the value as-is... (this is basically to handle object and dynamic types)
                    else if (type.IsAssignableFrom(value.GetType())) 
                    {
                        newValue = value;
                    } 
                    // otherwise we have no way to handle it.
                    else {
                        fail = true;
                    }
                }
                // or if the value is not a value-like type, then try to 
                else if (Types.IsObjectTarget(type))
                {
                    if (value is IEnumerable<KeyValuePair<string, object>>)
                    {
                        newValue = ObjectMapper.ToNew((IEnumerable<KeyValuePair<string, object>>)value, type);
                    }

                    else if (value is IEnumerable)
                    {
                        newValue = MapFromEnumerable((IEnumerable)value,type);
                    }
                    // both source and target are otherwise general purpose objects, try to map.
                    else if (Types.IsObjectTargetObject(value))
                    {
                        newValue = MapFromObject(value);
                    }
                    else
                    {
                        fail = true;
                    }
                }
              
                else if (Types.IsListTargetType(type)) {
                    newValue = MapFromEnumerable((IEnumerable)value,type);
                }
                else
                {
                    fail = true;
                }
            }
            if (fail)
            {
                if (Options.FailOnMismatchedTypes)
                {
                    throw new InvalidCastException(String.Format("No known method to map from {0} to {1}", value.GetType(), type));
                }
                else
                {
                    newValue = Options.UndefinedValue;
                }
            }
            return newValue;
        }
        protected IDictionary<string, object> MapFromObject(object value)
        {
            var converter = new Poco2Dict(Options);
            return converter.Map(value,Options.DynamicObjectType, true);
        }
        protected object MapFromNull(object value, Type type)
        {
            if (Types.IsNullableType(type))
            {
                return null;
            }
            else
            {
                throw new InvalidCastException(String.Format("Can't assign null value to type {0}", type));
            }

        }
        protected object MapFromEnumerable(IEnumerable value, Type type)
        {
            if (type.IsArray)
            {
                return MapToArray(value,type);
            }
            else if (Types.IsListTargetType(type))
            {
                return MapToList(value,type);
            }
            else
            {
                throw new InvalidCastException(String.Format("The target type {0} could not be mapped from type {1}",
                    type, value.GetType()));
            }
        }

        protected object MapToArray(IEnumerable source, Type type)
        {
            int count = 0;

            foreach (var item in source)
            {
                count++;
            }
            Type targetElementType = type.GetElementType();
            Array subValue = Array.CreateInstance(targetElementType, count);

            count = 0;
            foreach (var item in source)
            {
                subValue.SetValue(Map(item,targetElementType), count++);
            }
            return subValue;
        }
        protected object MapToList(IEnumerable value, Type type)
        {

            // map the enumerable source to a new list. If we can obtain the type of the original,
            // we'll create the same. If the target is an interface, then we'll do the best we can
            // by either mimicing a generic type or just creating a new list of obejcts.

            Type targetElementType=null;
            Type[] genTypeArgs = null;

            if (type.IsGenericType)
            {
                // the object is a generic list. We can create an instance of it.

                genTypeArgs = type.GetGenericArguments();

                // Don't deal with anything more than a single generic type - not really a list type
                if (genTypeArgs.Length > 1)
                {
                    throw new InvalidCastException(String.Format("The target type {0} had multiple generic arguments. I could not figure out how to map it from type {1}",
                        type, value.GetType()));
                }
                targetElementType = genTypeArgs[0];
            } else {
                genTypeArgs = new Type[1] { typeof(object) };
                targetElementType = typeof(object);
            }


            IList targetList;

            // if the target type is not directly instantiable, or not compatible with the source,
            // create something that is

            if (type.IsInterface || !typeof(IList).IsAssignableFrom(type))
            {
                Type listType = typeof(List<>).MakeGenericType(genTypeArgs);
                targetList = (IList)Activator.CreateInstance(listType);
            }
            else
            {
                targetList = (IList)Types.GetInstanceOf(type);
            }
           
            foreach (var item in value)
            {
                targetList.Add(Map(item,targetElementType));
            }
           
            return targetList;

        }
        
    }
}
