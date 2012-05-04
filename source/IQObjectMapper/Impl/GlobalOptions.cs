using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Collections;

namespace IQObjectMapper.Impl
{
    public struct GlobalOptions: IGlobalOptions
    {
        public static GlobalOptions Default()
        {
            var opts = new GlobalOptions();

            // leave these as the most permissive
            opts.IncludeProperties = true;
            opts.IncludeFields = true;
            opts.IncludePrivate = false;
            opts.DeclaredOnly = false;
            opts.CaseSensitive = false;
            opts.OverwriteExisting = true;
            opts.DynamicObjectType = typeof(ExpandoObject);
            opts.FailOnMismatchedTypes = true;
            opts.CanAlterProperties=true;
            opts.MissingPropertyValue = UndefinedObject.Value;
            opts.ParseSetData = false;
            opts.UpdateSource=true;
            opts.IsReadOnly = false;
            return opts;
            
        }
         public static GlobalOptions From(IGlobalOptions options)
        {
            var opts = new GlobalOptions();

            // leave these as the most permissive
            opts.IncludeProperties = options.IncludeProperties;
            opts.IncludeFields = options.IncludeFields;
            opts.IncludePrivate = options.IncludePrivate;
            opts.DeclaredOnly = options.DeclaredOnly;
            opts.CaseSensitive = options.CaseSensitive;
            opts.ParseSetData = options.ParseSetData;
            opts.OverwriteExisting = options.OverwriteExisting;
            opts.DynamicObjectType = options.DynamicObjectType;
            opts.FailOnMismatchedTypes = options.FailOnMismatchedTypes;
             opts.UpdateSource = options.UpdateSource;
             opts.IsReadOnly = options.IsReadOnly;
            
            return opts;
            
        }
        public static void DefaultsTo(object target) {

            var defaults = GlobalOptions.Default();
            IMapOptions moTarget = target as IMapOptions;
            if (moTarget!=null) {
                MapOptions.Copy(defaults,moTarget);
            }

            IGlobalOptions goTarget = target as IGlobalOptions;
            if (goTarget!=null) {
                 goTarget.FailOnMismatchedTypes=defaults.FailOnMismatchedTypes;
                goTarget.DynamicObjectType = defaults.DynamicObjectType ;
                goTarget.OverwriteExisting = defaults.OverwriteExisting;
            }

            IDictionaryOptions doTarget= target as IDictionaryOptions;

            if (doTarget!=null) {
                 doTarget.ParseSetData=defaults.ParseSetData;
                doTarget.UpdateSource = defaults.UpdateSource ;
                doTarget.CanAlterProperties = defaults.CanAlterProperties;
                doTarget.IsReadOnly = defaults.IsReadOnly ;
                doTarget.MissingPropertyValue = defaults.MissingPropertyValue;
            }
        }
        /// <summary>
        /// Add in global options not included in IMapOptions
        /// </summary>
        /// <param name="options"></param>
        public static GlobalOptions WithGlobal(IMapOptions options) {
            GlobalOptions final = GlobalOptions.Default();
            MapOptions.Copy(options,final);
            final.FailOnMismatchedTypes = ObjectMapper.Options.FailOnMismatchedTypes;
            final.DynamicObjectType = ObjectMapper.Options.DynamicObjectType ;
            final.OverwriteExisting = ObjectMapper.Options.OverwriteExisting;
            return final;
        }
        

        public bool OverwriteExisting
        {
            get;
            set;
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


        public Type DynamicObjectType
        {
            get;
            set;
        }

        public bool FailOnMismatchedTypes
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
            return new BitArray(new bool[] {
                IncludeProperties,
                IncludeFields,
                IncludePrivate,
                CaseSensitive,
                FailOnMismatchedTypes,
                OverwriteExisting,
                DeclaredOnly,
                ParseSetData, 
                UpdateSource,
                CanAlterProperties,
                IsReadOnly}).GetHashCode() +
            DynamicObjectType.GetHashCode() +
            MissingPropertyValue.GetHashCode();

        }
        public override bool Equals(object obj)
        {
            if (obj is IGlobalOptions)
            {
                IGlobalOptions other = (IGlobalOptions)obj;

                return
                    other.IncludeProperties == IncludeProperties &&
                    other.IncludeFields == IncludeFields &&
                    other.IncludePrivate == IncludePrivate &&
                    other.CaseSensitive == CaseSensitive &&
                    other.FailOnMismatchedTypes == FailOnMismatchedTypes &&
                    other.DynamicObjectType == DynamicObjectType &&
                    other.OverwriteExisting == OverwriteExisting &&
                    other.DeclaredOnly == DeclaredOnly &&
                    other.ParseSetData == ParseSetData &&
                    other.UpdateSource == UpdateSource &&
                    other.CanAlterProperties == CanAlterProperties && 
                    other.IsReadOnly == IsReadOnly && 
                    other.MissingPropertyValue == MissingPropertyValue;
            }
            else
            {
                return false;
            }
        }

        public bool ParseSetData
        {
            get;
            set;
        }

        public bool UpdateSource
        {
            get;
            set;
        }

        public bool CanAlterProperties
        {
            get;
            set;
        }

        public bool IsReadOnly
        {
            get;
            set;
        }
        public object MissingPropertyValue
        {
            get;
            set;
        }
    }
}
