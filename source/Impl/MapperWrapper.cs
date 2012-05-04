using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQObjectMapper.Impl;

namespace IQObjectMapper
{
    /// <summary>
    /// A class that exposes IEnumerable-T- given a list of keyvalue pairs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MapperWrapper<T> : IEnumerable<T>
    {
        public MapperWrapper(IEnumerable<IEnumerable<KeyValuePair<string, object>>> source, IMapOptions options = null)
        {
            Source = source;
            Options = options;
        }
        protected IEnumerable<IEnumerable<KeyValuePair<string, object>>> Source;
        protected IMapOptions Options;
        protected MapperWrapperEnumerator<T> Enumerator;
        public IEnumerator<T> GetEnumerator()
        {
            return new MapperWrapperEnumerator<T>(Source, Options); ;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class MapperWrapperEnumerator<T>: IEnumerator<T>
    {
        public MapperWrapperEnumerator(IEnumerable<IEnumerable<KeyValuePair<string, object>>> source, IMapOptions options = null)
        {
            Source = source;
            InnerEnumerator = Source.GetEnumerator();
            Mapper = new Dict2Anything(options ?? ObjectMapper.MapOptions);
            IsValueTarget = Types.IsValueTarget<T>();
        }
        protected bool IsValueTarget;
        protected IEnumerable<IEnumerable<KeyValuePair<string, object>>> Source;
        protected Dict2Anything Mapper;
        protected IEnumerator<IEnumerable<KeyValuePair<string, object>>> InnerEnumerator;

        public T Current
        {
            get
            {
                if (IsValueTarget)
                {
                    return (T)Types.Parse(InnerEnumerator.Current, typeof(T));
                }
                else
                {
                    return (T)Mapper.Map(InnerEnumerator.Current, typeof(T));
                }
            }
        }
        
        public void Dispose()
        {
            if (Source is IDisposable)
            {
                ((IDisposable)Source).Dispose();
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
        T IEnumerator<T>.Current
        {
            get
            {
                return Current;
            }
        }
        public bool MoveNext()
        {
            return InnerEnumerator.MoveNext();

        }

        public void Reset()
        {
            InnerEnumerator.Reset();
        }


    }


}
