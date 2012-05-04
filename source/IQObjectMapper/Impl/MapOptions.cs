using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;


namespace IQObjectMapper.Impl
{
    public struct MapOptions: IMapOptions
    {
        public static MapOptions Default() {
            var opts = new MapOptions();
            opts.CaseSensitive=true;
            opts.DeclaredOnly=false;
            opts.IncludeFields=true;
            opts.IncludePrivate=true;
            opts.IncludeProperties=true;
            return opts;
        }
        public static MapOptions From(IMapOptions options)
        {
            var opts = new MapOptions();
            opts.IncludeProperties = options.IncludeProperties;
            opts.IncludeFields = options.IncludeFields;
            opts.IncludePrivate = options.IncludePrivate;
            opts.DeclaredOnly = options.DeclaredOnly;
            opts.CaseSensitive = options.CaseSensitive;
            return opts;
        }
        public static void Copy(IMapOptions source, IMapOptions target, bool useDefaults = true)
        {
            IMapOptions finalSource = source ?? (useDefaults ? ObjectMapper.Options : null);
            if (finalSource == null || target == null)
            {
                return;
            }
            target.IncludeFields = finalSource.IncludeFields;
            target.IncludePrivate = finalSource.IncludePrivate;
            target.IncludeProperties = finalSource.IncludeProperties;
            target.DeclaredOnly = finalSource.DeclaredOnly;
            target.CaseSensitive = finalSource.CaseSensitive;
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
            if (obj is IMapOptions)
            {
                IMapOptions other = (IMapOptions)obj;
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
