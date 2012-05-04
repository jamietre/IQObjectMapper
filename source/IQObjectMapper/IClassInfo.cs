using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQObjectMapper.Impl;

namespace IQObjectMapper
{
    /// <summary>
    /// Metadata about classes. 
    /// </summary>
    public interface IClassInfo : IEnumerable<IDelegateInfo>
    {
        IDelegateInfo this[string fieldName] { get; }
        Type Type { get; }
        IClassInfoData Data { get; }
        IEnumerable<IDelegateInfo> Fields { get; }
        IEnumerable<string> FieldNames { get; }
        int Count { get; }

        bool ContainsField(string fieldName);
        bool TryGetValue(string fieldName, out IDelegateInfo info);

    }

}
