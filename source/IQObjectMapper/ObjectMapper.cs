using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Data;
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

        #region public conversion methods
      
        /// <summary>
        /// Map a dictionary-like object to a new dynamic object. The type of object created is DefaultOptions.DynamicObjectType
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDynamicMetaObjectProvider ToDynamic(object source, bool deep=false, IMapOptions options = null)
        {
            return (IDynamicMetaObjectProvider)MapFrom(options).Map(source, DefaultOptions.DynamicObjectType, deep);
        }
        /// <summary>
        /// Map a POCO to a new dynamic object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="deep">When true, inner mappable objects will also be converted to dynamic objects</param>
        /// <returns></returns>
        public static T ToDynamic<T>(object source, bool deep = false, IMapOptions options = null) where T : IDynamicMetaObjectProvider
        {
            return MapFrom(options).Map<T>(source, deep);
        }
        /// <summary>
        /// Map a dictionary to a new instance of type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ToNew(IEnumerable<KeyValuePair<string, object>> source, Type type, IMapOptions options = null)
        {
            return MapTo(options).Map(source, type);
        }
        /// <summary>
        /// Map a dictionary to a new instance of T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T ToNew<T>(IEnumerable<KeyValuePair<string, object>> source, IMapOptions options = null)
        {
            return (T)MapTo(options).Map(source, typeof(T));
        }
        /// <summary>
        /// Map a dictionary to an existing object 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void ToExisting(IEnumerable<KeyValuePair<string, object>> source, object target, IMapOptions options=null)
        {
            MapTo(options).Map(source, target);
        }
        /// <summary>
        /// Map a dictionary to an existing instance o T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void ToExisting<T>(IEnumerable<KeyValuePair<string, object>> source, T target, IMapOptions options=null)
        {
            MapTo(options).Map(source,target);
        }
        /// <summary>
        /// Map the properties of an object to a new dictionary. 
        /// </summary>
        /// <param name="source">The POCO object to be mapped</param>
        /// <param name="deep">Inner POCO objects will also be mapped to dictionaries</param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(object source, bool deep = false, IMapOptions options=null)
        {
            return MapFrom(options).Map(source, deep);
        }
        /// <summary>
        /// Convert a value-like object into a specific type. Can be used to parse value types, or convert incompatible array and list types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T ParseValue<T>(object source) 
        {
            return (T)ParseValue(source, typeof(T));
        }
        /// <summary>
        /// Map any value-type to another value type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static object ParseValue(object source, Type type)
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

        public static T Map<T>(T source, Func<IDelegateInfo,object,object> mapFunc, IMapOptions options=null) where T: class, new()
        {
            var opts =MapOptions.From(options);
            IDictionary<string,object> dict = new Dictionary<string,object>(ObjectMapper.MapperCache.GetStringComparer(opts));
            var delegates = new Adapters.DelegateAdapter(typeof(T),opts);

            foreach (var del in delegates)
            {
                dict[del.Name] = mapFunc(del, del.GetValue(source));
            }
            return ObjectMapper.ToNew<T>(dict);
        }

        #endregion

        #region public adapter methds

        // these methods instantiate one of the adapaters

        /// <summary>
        /// Return a datareader as an enumeration of IDictionary-string,value-
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IEnumerable<IDictionary<string, object>> ToDictionarySequence(IDataReader reader, IMapOptions options = null)
        {
            return new Adapters.DataReaderDictionary(reader,options);
        }
        /// <summary>
        /// Convert the current row of a DataReader to an IDictionary-string,value-
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(IDataRecord reader, IMapOptions options=null)
        {
            return new Adapters.DataRecordDictionary(reader,options);
        }
        /// <summary>
        /// Cast the source as a dynamic object. Unlike ToDynamic, this is bound to the underlying object, and changes
        /// to bound properties will affect the underlying object.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDynamicMetaObjectProvider AsDynamic(object source, IMapOptions options = null)
        {
            return new Adapters.DynamicAdapter(source,options);
        }
        /// <summary>
        /// Cast the source as a dynamic object of a specific type.Unlike ToDynamic, this is bound to the underlying 
        /// object, and changes to bound properties will affect the underlying object.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDynamicMetaObjectProvider AsDynamic<T>(object source, IMapOptions options=null) where T: IDynamicMetaObjectProvider
        {
            var opts = MapOptions.From(options);
            opts.DynamicObjectType = typeof(T);

            return new Adapters.DynamicAdapter(source,opts);
        }
        /// <summary>
        /// Cast the source as an IDictionary-string,object-. Unlike ToDictionary, changes are reflected in the source.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDictionary<string, object> AsDictionary(object source, IMapOptions options=null)
        {
            return new Adapters.PropertyDictionaryAdapter(source,options);
        }
        /// <summary>
        /// Convert a sequence of dictionary-like sources (e.g, a list of dynamic objects) to a sequence of strongly-typed objects
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="?"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToTypedSequence<T>(IEnumerable<IEnumerable<KeyValuePair<string,object>>> source) {
            return new Adapters.TypedAdapter<T>(source);
            
        }

        #endregion

        #region lower level methods

        public static IClassInfo GetClassInfo(object source, IReflectionOptions options=null)
        {
            return MapperCache.GetClassInfo(source.GetType(), options);
        }
        public static IClassInfo GetClassInfo<T>(IReflectionOptions options = null)
        {
            return MapperCache.GetClassInfo(typeof(T), options);
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
