using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IQObjectMapper.Impl;
namespace IQObjectMapper
{
    /// <summary>
    /// Represents the list of IDelegateInfo objects associated with a class. This is
    /// separated from the ClassInfo object to account for different filtering of the
    /// data based on options, e.g. we may want to change what fields/properties are 
    /// exposed after reflecting the class based on current option settings. By building
    /// ClassInfo as a composite object of metadata and the list of fields, it is much
    /// easier to subclass ClassInfo without having to re-implement the functionality for
    /// filtering the field list.
    /// </summary>
    public interface IClassInfoData : IEnumerable<KeyValuePair<string, IDelegateInfo>>
    {
        void Add(string key, IDelegateInfo value);
        bool ContainsKey(string key);
        ICollection<string> Keys { get; }
        bool TryGetValue(string key, out IDelegateInfo value);
        ICollection<IDelegateInfo> Values { get; }
        IDelegateInfo this[string key] { get; }
        bool Contains(KeyValuePair<string, IDelegateInfo> item);
        int Count { get; }

        Type Type { get; }
        ReflectionOptions Options { get; set; }
        IClassInfoData Clone(IReflectionOptions options);
    }
}
