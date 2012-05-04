using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace IQObjectMapper
{
    public interface IReflectionOptions : IMapOptions
    {
        /// <summary>
        ///  Fields will be mapped
        /// </summary>
        bool IncludeFields { get; set; }
        /// <summary>
        /// Properties will be mapped
        /// </summary>
        bool IncludeProperties { get; set; }
        bool IncludePrivate { get; set; }
        bool DeclaredOnly { get; set; }
        bool CaseSensitive { get; set; }
    }
  
}
