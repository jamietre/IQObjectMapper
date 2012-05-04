using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace IQObjectMapper
{
    /// <summary>
    /// Options that apply to dictionary or dictionary-like adapters 
    /// </summary>
    public interface IDictionaryOptions : IMapOptions
    {
        /// <summary>
        /// When true, underlying objects will be updated when an adapter property is updated
        /// </summary>
        bool UpdateSource { get; set; }
        /// <summary>
        /// When true, adapters that map hard object properties to a dictionary will allow members to be
        /// added or removed from the dictionary or dynamic object
        /// </summary>
        bool CanAlterProperties { get; set; }
        /// <summary>
        /// When true, data in the dictionary (and hence the underlying object) cannot be changed through the
        /// adapter. This also causes CanAlterProperties to always be false.
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// When true, accesses to missing properties in expando objects are permitted.
        /// </summary>
        bool CanAccessMissingProperties { get; set; }

    }
    
}
