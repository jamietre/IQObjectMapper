using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using IQObjectMapper.Impl;

namespace IQObjectMapper
{
    public static class ObjectMapper
    {
        static ObjectMapper()
        {
            DefaultOptions = new MapOptions(true);
        }

        #region public properties

        /// <summary>
        /// A singleton to manage access and creation of the typed metadata. This pattern (static instance of a class) 
        /// allows a client of this library to overriede the default caching object.
        /// </summary>
        public static IMapperCache MapperCache
        {
            get
            {
                if (_MapperCache == null)
                {
                    _MapperCache = new MapperCache();
                }
                return _MapperCache;
            }
            set
            {
                _MapperCache = value;
            }
        }
        /// <summary>
        /// Default options used when creating object instances
        /// </summary>
        public static MapOptions DefaultOptions
        {
            get;
            private set;
        }

        #endregion

        #region public methods
      
        /// <summary>
        /// Map a dictionary-like object to a new dynamic object. The type of object created is DefaultOptions.DynamicObjectType
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(IEnumerable<KeyValuePair<string, object>> source)
        {
            IDynamicMetaObjectProvider output = (IDynamicMetaObjectProvider)Types.GetInstanceOf(DefaultOptions.DynamicObjectType);
            var mapper = MapTo();
            mapper.Map(source, output);
            return output;
        }
        /// <summary>
        /// Map a POCO to a new dynamic object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="deep">When true, inner mappable objects will also be converted to dynamic objects</param>
        /// <returns></returns>
        public static T ToDynamic<T>(object source, bool deep = false) where T : IDynamicMetaObjectProvider
        {
            return MapFrom().Map<T>(source, deep);
        }
        /// <summary>
        /// Map a dictionary to a new instance of type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ToNew(IEnumerable<KeyValuePair<string, object>> source, Type type)
        {
            return MapTo().Map(source, type);
        }
        /// <summary>
        /// Map a dictionary to a new instance of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T ToNew<T>(IEnumerable<KeyValuePair<string, object>> source)
        {
            return (T)MapTo().Map(source, typeof(T));
        }
        /// <summary>
        /// Map a dictionary to an existing object 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void ToExisting(IEnumerable<KeyValuePair<string, object>> source, object target)
        {
            MapTo().Map(source, target);
        }
        /// <summary>
        /// Map a dictionary to an existing instance o T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void ToExisting<T>(IEnumerable<KeyValuePair<string, object>> source, T target)
        {
            MapTo().Map(source,target);
        }
        /// <summary>
        /// Map the properties of an object to a new dictionary. 
        /// </summary>
        /// <param name="source">The POCO object to be mapped</param>
        /// <param name="deep">Inner POCO objects will also be mapped to dictionaries</param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(object source, bool deep = false)
        {
            return MapFrom().Map(source, deep);
        }
        /// <summary>
        /// Convert a value-like object into a specific type. Can be used to parse value types, or convert incompatible array and list types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T MapValue<T>(object source) 
        {
            return (T)MapValue(source, typeof(T));
        }
        /// <summary>
        /// Map any value-type to another value type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static object MapValue(object source, Type type)
        {
            if (Types.IsValueTarget(type) || Types.IsListTargetType(type))
            {
                return Types.Parse(source, type);
            }
            else
            {
                throw new Exception(String.Format("Unable to convert from type {0} to {1}", source.GetType(), type));
            }

        }

        #endregion

        #region private methods

        private static IMapperCache _MapperCache;
        private static Dict2Poco MapTo(IMapOptions options = null)
        {
            return new Dict2Poco(options);
        }
        private static Poco2Dict MapFrom(IMapOptions options = null)
        {
            return new Poco2Dict(options);
        }

        #endregion
    }
}
