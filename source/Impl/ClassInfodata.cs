using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQObjectMapper.Impl
{


    /// <summary>
    /// Pluggable dictionary to back up ClassInfo. The interface IClassInfoData hides the methods that alter a dictionary, but the
    /// object exposes the full interface for internal use.
    /// </summary>
    public class ClassInfoData: IDictionary<string,IDelegateInfo>, IClassInfoData
    {

        #region constructors
        
        /// <summary>
        /// When passed with no options we don't want to use the defaults - we always need to capture as much detail as possible
        /// </summary>
        /// <param name="options"></param>
        public ClassInfoData(Type type, IReflectionOptions options=null)
        {
            Type = type;
            Options = ReflectionOptions.From(options);
            InnerDict = new Dictionary<string, IDelegateInfo>(Options.CaseSensitive ?
                        StringComparer.Ordinal :
                        StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region private properties

        private IDictionary<string, IDelegateInfo> InnerDict;
        //protected IDictionary<string, IDelegateInfo> InnerDict
        //{
        //    get
        //    {
        //        if (_InnerDict == null)
        //        {
        //            _InnerDict = new Dictionary<string, IDelegateInfo>(Options.CaseSensitive ?
        //                StringComparer.Ordinal :
        //                StringComparer.OrdinalIgnoreCase);
        //        }
        //        return _InnerDict;
        //    }
        //}

        #endregion

        #region public properties

        public ReflectionOptions Options {get; set;}
        public Type Type { get; protected set; }

        #endregion

        #region public methods

        public IClassInfoData Clone(IReflectionOptions options)
        {
            ClassInfoData cid = new ClassInfoData(Type,options);
            IDictionary<string, IDelegateInfo> dict = (IDictionary<string, IDelegateInfo>)cid;

            ReflectionOptions opts= ReflectionOptions.From(options);
            cid.Options = opts;
            foreach (var kvp in this)
            {
                var item = kvp.Value;
                if ((opts.IncludePrivate || !item.IsPrivate) &&
                    (item.IsDeclared || !opts.DeclaredOnly) &&
                    ((opts.IncludeFields && item.IsField) ||
                    (opts.IncludeProperties && item.IsProperty)))
                {
                        if (dict.ContainsKey(kvp.Key))
                        {
                            throw new Exception("The key {0} could not be added, probably because there are two different cased versions of the same string in the original map.");
                        }
                        dict.Add(kvp);
                }
            }
            return cid;
        }

        public override int GetHashCode()
        {
            return Options.GetHashCode() +
            Type.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            ClassInfoData other = obj as ClassInfoData;
            return other != null &&
                Type == other.Type &&
                Options.Equals(other.Options);
                
        }

        #endregion

        #region Idictionary members

        public void Add(string key, IDelegateInfo value)
        {
            InnerDict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return InnerDict.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return InnerDict.Keys; }
        }

        bool IDictionary<string, IDelegateInfo>.Remove(string key)
        {
            return InnerDict.Remove(key);
        }

        public bool TryGetValue(string key, out IDelegateInfo value)
        {
            return InnerDict.TryGetValue(key, out value);
        }

        public ICollection<IDelegateInfo> Values
        {
            get { return InnerDict.Values; }
        }

        public IDelegateInfo this[string key]
        {
            get
            {
                return InnerDict[key];
            }
            set
            {
                InnerDict[key] = value;
            }
        }


        public bool Contains(KeyValuePair<string, IDelegateInfo> item)
        {
            return InnerDict.Contains(item);
        }

        public int Count
        {
            get { return InnerDict.Count; }
        }

        public IEnumerator<KeyValuePair<string, IDelegateInfo>> GetEnumerator()
        {
            return InnerDict.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDictionary interface

        void IDictionary<string, IDelegateInfo>.Add(string key, IDelegateInfo value)
        {
            InnerDict.Add(key, value);
        }

        IDelegateInfo IDictionary<string, IDelegateInfo>.this[string key]
        {
            get
            {
                return InnerDict[key];
            }
            set
            {
                InnerDict[key] = value;
            }
        }

        void ICollection<KeyValuePair<string, IDelegateInfo>>.Add(KeyValuePair<string, IDelegateInfo> item)
        {
            InnerDict.Add(item);
        }

        void ICollection<KeyValuePair<string, IDelegateInfo>>.Clear()
        {
            InnerDict.Clear();
        }


        void ICollection<KeyValuePair<string, IDelegateInfo>>.CopyTo(KeyValuePair<string, IDelegateInfo>[] array, int arrayIndex)
        {
            InnerDict.CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<string, IDelegateInfo>>.Count
        {
            get {return Count; }
        }

        bool ICollection<KeyValuePair<string, IDelegateInfo>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<string, IDelegateInfo>>.Remove(KeyValuePair<string, IDelegateInfo> item)
        {
            return InnerDict.Remove(item);
        }

        IEnumerator<KeyValuePair<string, IDelegateInfo>> IEnumerable<KeyValuePair<string, IDelegateInfo>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
