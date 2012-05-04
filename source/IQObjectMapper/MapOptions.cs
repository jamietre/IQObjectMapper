using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Collections;

namespace IQObjectMapper
{
    /// <summary>
    /// The global options object. To override options for a specific instance, pass in an instance of this, e.g. 
    ///    var opts = new Options { ParseValues=true, IncludeProperties=false};
    ///    
    /// Anything not specified in the constructor will default to the overall default options defined in ObjectMapper.DefaultOptions
    /// 
    /// </summary>
    public class MapOptions: IGlobalOptions, IReflectionOptions,IDictionaryOptions, IMapOptions
    {
        #region constructors

        /// <summary>
        /// Constructor used by the setup code. Otherwise, this will always copy settings from the defaults
        /// </summary>
        /// <param name="raw"></param>
        internal MapOptions(bool raw) {
            // leave these as the most permissive
            IncludeProperties = true;
            IncludeFields = true;
            IncludePrivate = false;
            DeclaredOnly = false;
            CaseSensitive = false;
            DynamicObjectType = typeof(ExpandoObject);
            FailOnMismatchedTypes = true;
            CanAlterProperties = true;
            CanAccessMissingProperties = true;
            UndefinedValue = Undefined.Value;
            ParseValues = false;
            UpdateSource = true;
            IsReadOnly = false;
        }
        /// <summary>
        /// default constructor copies options from the global default settings
        /// </summary>
        public MapOptions()
        {
            Copy(ObjectMapper.DefaultOptions, this, false);
        }
        /// <summary>
        /// Return a new instance of the active global default options
        /// </summary>
        /// <returns></returns>
        public static MapOptions Default()
        {
            return new MapOptions();
        }

        #endregion

        #region static methods

        /// <summary>
        /// Create a new options object based on another object implementing IGlobalOptions
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static MapOptions From(IMapOptions options)
        {
            if (options is MapOptions && !ReferenceEquals(options, ObjectMapper.DefaultOptions))
            {
                // just pass through the object if it's a concrete instance, and not the default object.
                return (MapOptions)options;
            }
            else
            {
                var opts = new MapOptions();
                Copy(options, opts, true);
                return opts;
            }
        }
        /// <summary>
        /// Copy the default options to any object implementing one of the options interfaces. Objects that do not implement
        /// any of the interfaces will be unchanged.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static void DefaultsTo(IMapOptions target)
        {
            Copy(null, target,true);
        }
        public static void Copy(IMapOptions source, IMapOptions target, bool withDefaults=true) {

            if (source == null)
            {
                return;
            }
            MapOptions defaults = ObjectMapper.DefaultOptions;

            IReflectionOptions moSource = source as IReflectionOptions ?? defaults;
            IReflectionOptions moTarget = target as IReflectionOptions;
            if (moTarget!=null) {
                moTarget.IncludeFields = moSource.IncludeFields;
                moTarget.IncludePrivate = moSource.IncludePrivate;
                moTarget.IncludeProperties = moSource.IncludeProperties;
                moTarget.DeclaredOnly = moSource.DeclaredOnly;
                moTarget.CaseSensitive = moSource.CaseSensitive;
            }

            IGlobalOptions goSource = source as IGlobalOptions ?? defaults;
            IGlobalOptions goTarget = target as IGlobalOptions;
            if (goTarget!=null) {
                goTarget.FailOnMismatchedTypes = goSource.FailOnMismatchedTypes;
                goTarget.DynamicObjectType = goSource.DynamicObjectType;
                goTarget.ParseValues = goSource.ParseValues;
                goTarget.UndefinedValue = goSource.UndefinedValue;

            }

            IDictionaryOptions doSource = source as IDictionaryOptions ?? defaults;
            IDictionaryOptions doTarget = target as IDictionaryOptions;

            if (doTarget!=null) {
                doTarget.UpdateSource = doSource.UpdateSource;
                doTarget.CanAlterProperties = doSource.CanAlterProperties;
                doTarget.IsReadOnly = doSource.IsReadOnly;
                doTarget.CanAccessMissingProperties = doSource.CanAccessMissingProperties;
            }
        }
        #endregion

        #region public properties

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

        public bool ParseValues
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
        public bool CanAccessMissingProperties
        {
            get;
            set;
        }
        public bool IsReadOnly
        {
            get;
            set;
        }
        public object UndefinedValue
        {
            get;
            set;
        }
        #endregion

        #region private mehtods

        private BitArray OptionsArray(IGlobalOptions options)
        {
            return new BitArray(new bool[] {
                IncludeProperties,
                IncludeFields,
                IncludePrivate,
                CaseSensitive,
                FailOnMismatchedTypes,
                DeclaredOnly,
                ParseValues, 
                UpdateSource,
                CanAlterProperties,
                CanAccessMissingProperties,
                IsReadOnly});
        }
        public override int GetHashCode()
        {
            return OptionsArray(this).GetHashCode() +
            DynamicObjectType.GetHashCode() +
            UndefinedValue.GetHashCode();

        }
        public override bool Equals(object obj)
        {
            if (obj is IGlobalOptions)
            {
                IGlobalOptions other = (IGlobalOptions)obj;

                return
                    OptionsArray(this).Equals(OptionsArray(other)) &&
                    other.DynamicObjectType == DynamicObjectType &&
                    other.UndefinedValue == UndefinedValue;
            }
            else
            {
                return false;
            }
        }
        #endregion

    }
}
