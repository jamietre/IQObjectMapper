using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace IQObjectMapper.Impl
{
    public interface IMapperCache
    {
        IClassInfo GetClassInfo(Type type, IReflectionOptions options);
        bool TryGetClassInfo(Type type, out IClassInfo classInfo);
        bool HasClassInfo(Type type);
        IClassInfo ClassInfo<U>() where U : class;
        void ForgetAbout(Type type);
        void ClearCache();

        IClassInfoBuilder GetClassInfoBuilder();
    }

    /// <summary>
    /// Internal singleton for manageing ClassInfo structure cache
    /// </summary>
    public class MapperCache: IMapperCache
    {
       
        private static bool Instantiated;
        private static object _Locker = new Object();
        public MapperCache()
        {
            lock (_Locker)
            {
                if (Instantiated)
                {
                    throw new Exception("You can only have one instance of MapperData.");
                }
                Instantiated = true;
            }
        }

        protected IDictionary<Type, IClassInfo> ClassInfoCache
             = new ConcurrentDictionary<Type, IClassInfo>();
        
        protected IDictionary<Tuple<Type,ReflectionOptions>, IClassInfoData> ClassInfoDataCache
          = new ConcurrentDictionary<Tuple<Type, ReflectionOptions>, IClassInfoData>();


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


        IClassInfo IMapperCache.GetClassInfo(Type type, IReflectionOptions options)
        {
            return GetClassInfo(type, options);
        }

        IClassInfo IMapperCache.ClassInfo<U>()
        {
            return ClassInfo<U>();
        }
    }
}
