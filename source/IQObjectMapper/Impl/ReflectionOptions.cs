using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;


namespace IQObjectMapper.Impl
{
    public struct ReflectionOptions : IReflectionOptions
    {
        public static ReflectionOptions Default()
        {
            var opts = new ReflectionOptions();
            opts.CaseSensitive = true;
            opts.DeclaredOnly = false;
            opts.IncludeFields = true;
            opts.IncludePrivate = true;
            opts.IncludeProperties = true;
            return opts;
        }
        public static ReflectionOptions From(IReflectionOptions options)
        {
            var opts = new ReflectionOptions();
            opts.IncludeProperties = options.IncludeProperties;
            opts.IncludeFields = options.IncludeFields;
            opts.IncludePrivate = options.IncludePrivate;
            opts.DeclaredOnly = options.DeclaredOnly;
            opts.CaseSensitive = options.CaseSensitive;
            return opts;
        }


        public bool IncludeFields
        {
            get;
            set;
        }
        public bool IncludeProperties
        {
            get;
            set;
        }
        public bool IncludePrivate
        {
            get;
            set;
        }

        public bool DeclaredOnly
        {
            get;
            set;
        }
        public bool CaseSensitive
        {
            get;
            set;
        }

        public override int GetHashCode()
        {
            return IncludeProperties.GetHashCode() +
                IncludeFields.GetHashCode() +
                IncludePrivate.GetHashCode() +
                CaseSensitive.GetHashCode() +
                DeclaredOnly.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is IReflectionOptions)
            {
                IReflectionOptions other = (IReflectionOptions)obj;
                return other != null &&
                    other.IncludeProperties == IncludeProperties &&
                    other.IncludeFields == IncludeFields &&
                    other.IncludePrivate == IncludePrivate &&
                    other.CaseSensitive == CaseSensitive &&
                    other.DeclaredOnly == DeclaredOnly;
            }
            else
            {
                return false;
            }
        }
    }
}
