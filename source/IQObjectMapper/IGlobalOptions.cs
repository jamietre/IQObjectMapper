using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQObjectMapper
{
    /// <summary>
    /// Library-level default options
    /// </summary>
    public interface IGlobalOptions : IMapOptions
    {
        /// <summary>
        /// When true, values of incompatible types will be parsed or coerced if possible into the target type
        /// </summary>
        bool ParseValues { get; set; }
        
        /// <summary>
        /// Type mismatches for matching properties will cause an error
        /// </summary>
        bool FailOnMismatchedTypes { get; set; }

        /// <summary>
        /// The default type of object to create for "object" or dynamic targets;
        /// </summary>
        Type DynamicObjectType { get; set; }

        /// <summary>
        /// When accessing a property of a dynamic object that doesn't exist, this value is returned.
        /// This is also used by default when a type conversion fails, but options prevented an error from
        /// being thrown.
        /// </summary>
        object UndefinedValue { get; set; }
    }
}
