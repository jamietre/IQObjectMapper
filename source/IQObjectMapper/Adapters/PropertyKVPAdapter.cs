using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQObjectMapper.Impl;

namespace IQObjectMapper.Adapters
{
    /// <summary>
    /// Enumerate the properties of an object as KeyValuePair-string,object-
    /// </summary>
    public class PropertyKVPAdapter : IEnumerable<KeyValuePair<string, object>>
    {
        #region constructor

        public PropertyKVPAdapter(object obj, bool deep=false, IMapOptions options=null)
        {
            InnerObject = obj;
            Deep = deep;
            Options = MapOptions.From(options);
        }

        #endregion

        #region private properties

        protected object InnerObject;
        #endregion
        
        #region public properties

        public bool Deep { get; set; }
        public MapOptions Options { get; set; }

        #endregion

        #region private methods

        protected IEnumerable<KeyValuePair<string, object>> GetDelegates()
        {
            var delegates = ObjectMapper.MapperCache.GetClassInfo(InnerObject.GetType(), Options);

            foreach (var del in delegates.Data.Values)
            {
                var value = del.GetValue(InnerObject);
                yield return new KeyValuePair<string,object>(
                        del.Name,
                       
                            DeepCopy(value) 
                            
                );
            }
        }

      

        protected object DeepCopy(object source)
        {
            if (!Deep || !Types.IsCloneableObject(source))
            {
                return source;
            }
            else
            {
                Type type = source.GetType();

                if (type.IsArray)
                {
                    return DeepCopyArray((Array)source, type);
                }
                else if (type is IEnumerable<KeyValuePair<string, object>>)
                {
                    return DeepCopyIEKVP((IEnumerable<KeyValuePair<string, object>>)source);
                }
                else if (!(source is IEnumerable))
                {
                    return new PropertyKVPAdapter(source, true, Options);
                }
                else
                {
                    return source;
                }
            }
        }
        
        protected IEnumerable<KeyValuePair<string,object>> DeepCopyIEKVP(IEnumerable<KeyValuePair<string,object>> source) {
            foreach (var kvp in source)
            {
                if (!Types.IsCloneableObject(kvp.Value))
                {
                    yield return kvp;
                } else {
                    yield return new KeyValuePair<string, object>(
                        Options.CaseSensitive ?
                            kvp.Key.ToLower() :
                            kvp.Key,
                        DeepCopy(kvp.Value)
                    );
                }
            }
        }
        protected Array DeepCopyArray(Array source, Type type)
        {
            
            Array src =source;
            int len = source.Length;
            Type elType = type.GetElementType();
            Array clone = Array.CreateInstance(elType,len);
            if (!Types.IsValueTarget(elType))
            {
                for (int i = 0; i < len; i++)
                {
                    clone.SetValue(DeepCopy(src.GetValue(i)), i);
                }
            }
            else
            {
                src.CopyTo(clone, 0);
            }

            return clone;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetDelegates().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        #endregion
    }


}
