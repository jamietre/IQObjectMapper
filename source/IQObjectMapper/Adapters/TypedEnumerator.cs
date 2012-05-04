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
    /// An enumerator that casts an enumeration of KVP-string,object- into a strongly typed object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypedEnumerator<T> : IEnumerator<T>
    {
        public TypedEnumerator(IEnumerable<IEnumerable<KeyValuePair<string, object>>> source, IMapOptions options = null)
        {
            Source = source;
            InnerEnumerator = Source.GetEnumerator();
            Options = MapOptions.From(options);
            Mapper = new Dict2Poco(options);
            IsValueTarget = Types.IsValueTarget<T>();
        }
        protected bool IsValueTarget;
        protected Dict2Poco Mapper;
        protected IEnumerable<IEnumerable<KeyValuePair<string, object>>> Source;
        protected IEnumerator<IEnumerable<KeyValuePair<string, object>>> InnerEnumerator;
        
        #region public properties
        /// <summary>
        /// An existing object that should be the target
        /// </summary>
        public T Target {get;set;}
        public MapOptions Options { get; set; }
        public Action<T> OnLoad { get; set; }

        public T Current
        {
            get
            {
                T obj;
                if (IsValueTarget)
                {
                    // when mapping to a value target, we have no choice but to take the first value and parse it. It's up to the
                    // client to ensure that there is only one column in the inner data reader (or source) or that they are ordered
                    // correctly.

                    if (Options.ParseValues)
                    {
                        obj = (T)Types.Parse(InnerEnumerator.Current.First().Value, typeof(T));
                    }
                    else
                    {
                        obj = (T)Types.ChangeType(InnerEnumerator.Current.First().Value, typeof(T));
                    }
                }
                else
                {
                    
                    if (Target!=null)
                    {
                        Mapper.Map(InnerEnumerator.Current, Target);
                        obj = Target;
                    }
                    else
                    {
                        obj = (T)Mapper.Map(InnerEnumerator.Current, typeof(T));
                    }
                }
                if (OnLoad!=null)
                {
                    OnLoad(obj);
                }
                return obj;

            }
        }

        public void Dispose()
        {
            if (Source is IDisposable)
            {
                ((IDisposable)Source).Dispose();
            }
        }
        #endregion

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
