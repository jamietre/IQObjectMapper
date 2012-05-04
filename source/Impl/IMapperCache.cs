using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQObjectMapper.Impl
{
    /// <summary>
    /// Interface defining the factory object
    /// </summary>
    public interface IMapperCache
    {
        bool TryGetClassInfo(Type type, out IClassInfo classInfo);
        bool HasClassInfo(Type type);
        IClassInfo ClassInfo<U>() where U : class;
        void ForgetAbout(Type type);
        void ClearCache();

        // factory methods

        IClassInfoBuilder GetClassInfoBuilder();
        IClassInfo GetClassInfo(Type type, IReflectionOptions options);
        IDictionary<string, T> GetDictionary<T>(IReflectionOptions options,
            IEnumerable<KeyValuePair<string,T>> initialData=null);
        StringComparer GetStringComparer(IReflectionOptions options);
    }

}
