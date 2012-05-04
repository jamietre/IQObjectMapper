using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Dynamic;
using IQObjectMapper.Adapters;

namespace IQObjectMapper.Impl
{
    public class Poco2Dict
    {
        public Poco2Dict(IMapOptions options=null)
        {
            Options = MapOptions.From(options);
        }

        protected virtual IDictionary<string, object> GetDictionary(Type type)
        {
            if (type == typeof(Dictionary<string, object>))
            {
                return new Dictionary<string, object>(Options.CaseSensitive  ?
                    StringComparer.Ordinal :
                    StringComparer.OrdinalIgnoreCase 
                    );
            }
            else
            {
                return (IDictionary<string, object>)Activator.CreateInstance(type);
            }
               
        }
        public MapOptions Options { get; set; }
        public IDictionary<string, object> Map(object source, bool deep = false)
        {
            return Map(source, Options.DynamicObjectType, deep);
        }
        public T Map<T>(object source, bool deep = false) where T: IDynamicMetaObjectProvider
        {
            return (T)Map(source, typeof(T), deep);

        }
        public IDictionary<string,object> Map(object source, Type targetType, bool deep=false)
        {

            if (!typeof(IDictionary<string, object>).IsAssignableFrom(targetType))
            {
                throw new Exception("The target must inherit IDictionary<string,object>");
            }
            IDictionary<string, object> dict = GetDictionary(targetType);
            

            if (source == null || Types.IsValueTargetObject(source))
            {
                throw new Exception("The object can't be null or a value-mappable type to convert to a dictionary.");
            }

            var sourceList = new PropertyKVPAdapter(source,deep,Options);

            foreach (var kvp in sourceList)
            {
                dict[kvp.Key] = kvp.Value;

            }
            return dict;
            
        }

    }
}
