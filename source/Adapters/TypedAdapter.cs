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
    /// A class that exposes IEnumerable-T- given a list of KeyValuePair-string,object-
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypedAdapter<T> : IEnumerable<T>
    {
        public TypedAdapter(IEnumerable<IEnumerable<KeyValuePair<string, object>>> source, IMapOptions options = null)
        {
            Source = source;
            Options = MapOptions.From(options);
        }

        protected IEnumerable<IEnumerable<KeyValuePair<string, object>>> Source;
        protected TypedEnumerator<T> Enumerator;

        public MapOptions Options { get; set; }
        public Action<T> OnLoad { get; set; }
        public T Target { get; set; }

        public IEnumerator<T> GetEnumerator()
        {
            var enumerator= new TypedEnumerator<T>(Source, Options);
            enumerator.OnLoad = OnLoad;
            enumerator.Target = Target;
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
