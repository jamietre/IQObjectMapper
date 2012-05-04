using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace IQObjectMapper.Impl
{
  
    /// <summary>
    /// Internal singleton for managing ClassInfo structure cache. This is also used as a 
    /// basic factory/DI manager just for convenience (e.g. rather than creating another singleton).
    /// This class may be subclassed to alter library behavior or extend its internal objects.
    /// </summary>
    public class MapperCache: IMapperCache
    {
        #region constructor

        private static bool _Instantiated;
        private static object _Locker = new Object();
        public MapperCache()
        {
            lock (_Locker)
            {
                if (_Instantiated)
                {
                    throw new Exception("You can only have one instance of MapperData.");
                }
                _Instantiated = true;
            }
        }

        #endregion

        #region private properties

        /// <summary>
        /// A cache of reflection/metadata about classes
        /// </summary>
        protected IDictionary<Type, IClassInfo> ClassInfoCache
             = new ConcurrentDictionary<Type, IClassInfo>();
        
        /// <summary>
        /// A cache of the property data lists. We allow clients to specify options when accessing
        /// the refleciton information for a class that can change what properties are exposed. Rather
        /// than re-reflecting each time, we subset the list of delegate data only by filtering based
        /// on options. To speed this process, the sets are stored and keyed to the options used to
        /// create them. The amount of data involved in doing this is pretty small (e.g. one reference *
        /// number of properties * permutations * unique classes). It might not save much time for classes
        /// with few properties, but probably is signficant if there are many properties. Some performance
        /// testing would be useful to determine if it's worth caching the filtered lists.
        /// </summary>
        protected IDictionary<Tuple<Type,ReflectionOptions>, IClassInfoData> ClassInfoDataCache
          = new ConcurrentDictionary<Tuple<Type, ReflectionOptions>, IClassInfoData>();

        #endregion

        #region factory methods


        /// <summary>
        /// Get a class info structure from the cache, or map to a class if the structure doesn't exist.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IClassInfo GetClassInfo(Type type, IReflectionOptions options = null)
        {
            IClassInfo info;

            if (!ClassInfoCache.TryGetValue(type, out info))
            {
                if (Types.IsMappableClass(type))
                {

                    var builder = GetClassInfoBuilder();
                    info = builder.MapClass(type);

                    if (!Types.IsAnonymousType(type))
                    {
                        ClassInfoCache[type] = info;
                        ClassInfoDataCache[Tuple.Create<Type, ReflectionOptions>(type, new ReflectionOptions())] = info.Data;
                    }
                }
            }

            if (info != null)
            {
                // if options that affect the list of fields are not the same as when it was 
                if (options==null || info.Data.Options.Equals(options))
                {
                    return info;
                }
                else
                {
                    var builder = GetClassInfoBuilder();
                    IClassInfoData data;
                    var dataKey = Tuple.Create<Type,ReflectionOptions>(type,ReflectionOptions.From(options));
                    if (!ClassInfoDataCache.TryGetValue(dataKey, out data))
                    {
                        data = info.Data.Clone(options);
                        ClassInfoDataCache[dataKey] = data;
                    }
                    return builder.ComposeClass(info, data);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This method must be overridden to provide the correctly typed ClassInfoBuilder, if using derivitaves of ClassInfo or DelegateInfo
        /// </summary>
        /// <returns></returns>
        public virtual IClassInfoBuilder GetClassInfoBuilder()
        {
            return new ClassInfoBuilder<ClassInfo>(ObjectMapper.DefaultOptions, typeof(DelegateInfo<,>));
        }

        public StringComparer GetStringComparer(IReflectionOptions options)
        {
            MapOptions opts = MapOptions.From(options);
            return options == null || opts.CaseSensitive == true ?
               StringComparer.CurrentCulture :
               StringComparer.CurrentCultureIgnoreCase;

        }

        /// <summary>
        /// Create a dictionary instance for options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public virtual IDictionary<string, T> GetDictionary<T>(IReflectionOptions options,
            IEnumerable<KeyValuePair<string,T>> initialData)
        {
            ManagedDictionary<T> dict;
           
            if (initialData != null)
            {
                MapOptions opts = MapOptions.From(options);
                dict = new ManagedDictionary<T>(new MapOptions
                {
                    CaseSensitive = opts.CaseSensitive,
                    CanAlterProperties = true,
                    IsReadOnly = false
                });
                foreach (var kvp in initialData)
                {
                    dict.Add(kvp);
                }
                dict.Options = opts;
            }
            else
            {
                dict = new ManagedDictionary<T>(options);
            }
            return dict;
        }

        #endregion

        #region Cache access methods

        /// <summary>
        /// Try to get a structure from the cache
        /// </summary>
        /// <param name="type"></param>
        /// <param name="classInfo"></param>
        /// <returns></returns>
        public bool TryGetClassInfo(Type type, out IClassInfo classInfo)
        {
            return ClassInfoCache.TryGetValue(type, out classInfo);
        }
        
        public bool HasClassInfo(Type type)
        {
            return ClassInfoCache.ContainsKey(type);
        }
        public IClassInfo ClassInfo<U>() where U : class
        {
            return GetClassInfo(typeof(U));
        }
        public void ClearCache()
        {
            ClassInfoCache.Clear();
        }

        /// <summary>
        /// Remove cached info about a class type
        /// </summary>
        /// <param name="t"></param>
        public void ForgetAbout(Type type)
        {
            ClassInfoCache.Remove(type);
        }

        #endregion

        #region private methods

        IClassInfo IMapperCache.GetClassInfo(Type type, IReflectionOptions options)
        {
            return GetClassInfo(type, options);
        }

        IClassInfo IMapperCache.ClassInfo<U>()
        {
            return ClassInfo<U>();
        }

        #endregion

    }
}
