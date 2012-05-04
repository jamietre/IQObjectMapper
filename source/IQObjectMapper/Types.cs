using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Dynamic;

namespace IQObjectMapper
{
    /// <summary>
    /// A set of Type-related utility fuctions
    /// </summary>
    public static class Types
    {
        #region constructors

        private static IDictionary<Type, int> TypeSizes;

        static Types()
        {
            TypeSizes = new Dictionary<Type, int>();

            TypeSizes[typeof(short)] = sizeof(short);
            TypeSizes[typeof(int)] = sizeof(int);
            TypeSizes[typeof(double)] = sizeof(double);
            TypeSizes[typeof(float)] = sizeof(float);
            TypeSizes[typeof(long)] = sizeof(long);
            TypeSizes[typeof(ushort)] = sizeof(ushort);
            TypeSizes[typeof(uint)] = sizeof(uint);
            TypeSizes[typeof(ulong)] = sizeof(ulong);
            TypeSizes[typeof(char)] = sizeof(char);
            TypeSizes[typeof(bool)] = sizeof(bool);
        }
        #endregion

        #region public methods
        public static int GetSizeEstimate(object obj)
        {
            int size = -1;
            Type type = obj.GetType();
            if (TypeSizes.TryGetValue(GetUnderlyingType(type), out size))
            {
                if (IsNullableType(type))
                {
                    size += 4;
                }
            }
            else if (type == typeof(string))
            {
                size = obj == null ? 4 : ((string)obj).Length;
            }
           
            return size;
        }
        /// <summary>
        /// Determine if the type is an anonymous type. See http://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAnonymousType(Type type)
        {
            return Attribute.IsDefined(type, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
        
        /// <summary>
        /// Try to convert an abitrary value into a specific type. This method, unlike Convert, will try to map certain
        /// values of non-compatible types e.g. "false" to (bool)false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Parse<T>(object value)
        {
            T output;
            if (!TryParse<T>(value, out output))
            {
                throw new Exception("Unable to convert to type " + typeof(T).ToString());
            }
            return output;
        }
        public static object Parse(object value, Type type)
        {
            object output;
            if (!TryParse(value, type, out output, Types.DefaultValue(type)))
            {
                throw new Exception("Unable to convert to type " + type.ToString());
            }
            return output;
        }
        /// <summary>
        /// Attempt to parse a value into a particular type, returning the default for the type if parsing fails
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ParseOrDefault<T>(object value)
        {
            return Parse<T>(value, (T)Types.DefaultValue<T>());
        }
        public static T Parse<T>(object value, T defaultValue)
        {
            T output;
            if (!TryParse<T>(value, out output))
            {
                output = defaultValue;
            }
            return output;
        }
        /// <summary>
        /// Try to convert an object value into a specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="typedValue"></param>
        /// <returns></returns>
        public static bool TryParse<T>(object value, out T typedValue)
        {
            object interimValue;
            bool result = TryParse(value, typeof(T), out interimValue, default(T));
            if (result)
            {
                typedValue = (T)interimValue;
            }
            else
            {
                typedValue = default(T);
            }
            return result;
        }
        /// <summary>
        /// Try to convert an object of arbitrary type to a specific type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="typedValue"></param>
        /// <param name="type"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool TryParse(object value, Type type, out object output, object defaultValue=null)
        {
            
            if (value == null || value == System.DBNull.Value || value.ToString()=="")
            {
                output = type==typeof(string) ? value : null;
                return IsNullable(type);
            }
            else if (type.IsAssignableFrom(value.GetType()))
            {
                output = ChangeType(value, type);
                return true;
            }
            else if (type == typeof(string))
            {
                output = value.ToString();
                    return true;
                }
            var stringVal = value.ToString().ToLower().Trim();
            Type realType = Types.GetUnderlyingType(type);

                if (stringVal == String.Empty)
                {
                    output = DefaultValue(type);
                    return false;
                }
            else if (realType == typeof(bool))
            {
                switch (stringVal)
                {
                    case "on":
                    case "yes":
                    case "true":
                    case "enabled":
                    case "active":
                    case "1":
                        output = true;
                        break;
                    case "off":
                    case "no":
                    case "false":
                    case "disabled":
                    case "0":
                        output = false;
                        break;
                    default:
                        output = DefaultValue(type);
                        return false;
                }
            }
            else if (realType.IsEnum)
            {
                output = ChangeType(Enum.Parse(realType, stringVal), realType);
            }
            else if (IsNumericType(realType))
            {
                object val;

                if (TryParseNumber(stringVal, out val, realType))
                {
                    output = ChangeType(val, realType); ;
                }
                else
                {
                    output = DefaultValue(type);
                    return false;
                }
            }
            else if (realType == typeof(DateTime))
            {
                DateTime val;
                if (DateTime.TryParse(stringVal, out val))
                {
                    output = ChangeType(val, realType);
                }
                else
                {
                    output = DefaultValue(type);
                    return false;
                }
            }
            else if (realType == typeof(object))
            {
                output = value;
            }
            else
            {
                throw new Exception("Don't know how to convert type " + type.UnderlyingSystemType.ToString());
            }

            return true;
        }

        /// <summary>
        /// Returns true if the type can accept a null value.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullable(Type type)
        {
            return type == typeof(string) || type.IsClass || IsNullableType(type);
        }
        public static bool IsNullable<T>()
        {
            return IsNullable(typeof(T));
        }
        /// <summary>
        /// Returns true if the type is a System.Nullable type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullableType(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
        public static bool IsNullableType<T>()
        {
            return IsNullableType(typeof(T));
        }
        
        /// <summary>
        /// Returns the default value or instance for an object or value type. 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object DefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
        public static T DefaultValue<T>()
        {
            return (T)DefaultValue(typeof(T));
        }
        public static Type GetUnderlyingType(Type type)
        {

            if (type != typeof(string) && IsNullableType(type))
            {
                return Nullable.GetUnderlyingType(type);
            }
            else
            {
                return type;
            }

        }
        /// <summary>
        /// Returns a nullable version of a type, or the same type if already nullable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetNullableType(Type type)
        {
            // Use Nullable.GetUnderlyingType() to remove the Nullable<T> wrapper if type is already nullable.
            type = GetUnderlyingType(type);
            if (type.IsValueType && type != typeof(string))
                return typeof(Nullable<>).MakeGenericType(type);
            else
                return type;
        }
        /// <summary>
        /// Returns true if an instance of the type can be created with no parameters. This could be a class, or a value type.
        /// Event though "object" is allowed to be created, it is excluded, becuase you can't actually map to it.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsInstantiableType(Type type)
        {
            Type t = GetUnderlyingType(type);

            bool isValidType = t.IsEnum || t.IsValueType || t.IsPrimitive || t == typeof(string) ||
                 (t.IsClass &&
                    (!t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null));
            bool isInvalidType = t == typeof(object) || t == typeof(void) || t.ContainsGenericParameters;

            return isValidType && !isInvalidType;
        }

        public static bool IsInstantiableType<T>()
        {
            return IsInstantiableType(typeof(T));
        }
        /// <summary>
        /// Returns true if the type can be mapped directly from a JSON parameter.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsRESTParameterType(Type type)
        {
            Type t = GetUnderlyingType(type);
            return t.IsEnum || t.IsValueType || t.IsPrimitive || t == typeof(string);
            // TODO: Autoconvert base64
            //           || t == typeof(byte[]);
        }
        /// <summary>
        /// Return the fully qualified path to  a method (including namespace)
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static string GetFullyQualifiedPath(MethodInfo methodInfo)
        {
            return String.Format("{0}.{1}.{2}",methodInfo.ReflectedType.Namespace, methodInfo.ReflectedType.Name, methodInfo.Name);
        }

        /// <summary>
        /// return an instance of an object or value type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetInstanceOf<T>()
        {

            T obj;
            if (typeof(T) == typeof(object))
            {
                return (T)Activator.CreateInstance(ObjectMapper.DefaultOptions.DynamicObjectType);
            }
            else if (typeof(T)==typeof(string)) {
                return default(T);
            } 
            else
            {
                obj = Activator.CreateInstance<T>();
            }

            return obj;
        }
        /// <summary>
        // return an instance of an object or value type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object GetInstanceOf(Type type)
        {

            object obj;
            if (type == typeof(object))
            {
                return Activator.CreateInstance(ObjectMapper.DefaultOptions.DynamicObjectType);
            } 
            else if (IsValueTarget(type))
            {
                obj = DefaultValue(type);
            }
            else
            {
                obj = Activator.CreateInstance(type);
            }

            return obj;
        }
        /// <summary>
        /// The type is a class that can have metadata associated with it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsMappableClass<T>()
        {
            return IsMappableClass(typeof(T));
        }
        public static bool IsMappableClass(Type type)
        {
            Type t = GetUnderlyingType(type);
            return t.IsClass && !t.IsArray && t != typeof(string) && t!=typeof(object) && t!=typeof(IDynamicMetaObjectProvider);
        }
        /// <summary>
        /// If the type can be the *value* target of a mapping. This means that a single value instance can be created for this type. It
        /// specifically excludes object and most array types (which are sub-objects) but includes byte arrays (which are basically a binary
        /// string and would not be read as an array in C#, but a stream)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsValueTarget<T>()
        {
            return IsValueTarget(typeof(T));
        }
        public static bool IsValueTarget(Type type)
        {
            Type t = GetUnderlyingType(type);
            bool isInvalid = type == typeof(void);
            bool isValid = t.IsEnum || t.IsValueType || t.IsPrimitive
                || t == typeof(string) ||
                (t.IsArray && IsValueType(t.GetElementType()));
            return isValid && !isInvalid;
        }
        public static bool IsValueTargetObject(object obj)
        {
            return obj == null ? false :
                IsValueTarget(obj.GetType());

        }
        public static bool IsValueType(Type type)
        {
            Type t = GetUnderlyingType(type);
            bool isInvalid = type == typeof(void);
            bool isValid = t.IsEnum || t.IsValueType || t.IsPrimitive
                || t == typeof(string);
            return isValid && !isInvalid;
        }
        public static bool IsCloneableObject(object obj)
        {
            return obj != null && !IsValueType(obj.GetType());

        }
        public static bool IsObjectTarget(Type type)
        {
            return type == typeof(object) || !IsValueTarget(type) && !(type==typeof(void));
        }
        public static bool IsObjectTargetObject(object obj)
        {
            return obj == null ? false : IsObjectTarget(obj.GetType());
        }
        /// <summary>
        ///  The type can be the direct target of a JSON value
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsJSONValueTarget(Type type)
        {
            return IsValueTarget(type) || type == typeof(byte[]);
        }

        /// <summary>
        /// Returns true if the object is a primitive numeric type, e.g. exluding string & char
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNumericType(object obj)
        {
            Type t = GetUnderlyingType(obj.GetType());
            return IsNumericBaseType(t);
        }
        public static bool IsNumericType(Type type)
        {
            Type t = GetUnderlyingType(type);
            return IsNumericBaseType(t);
        }
        /// <summary>
        /// The type is a type that we can reasonably expect to map a list to: 
        /// an array, or a generic ICollection type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsListTargetType(Type target)
        {
            bool isValid = target.IsArray;
            // if it's enumerable and generic, then it's a list of some kind
            if (!isValid && target.IsGenericType)
            {
                Type[] genTypeArgs = target.GetGenericArguments();

                // Any list type should inherit ICollection<T> contravariantly, this should tell us if
                // it's some generic collection type.

                if (genTypeArgs.Length > 1)
                {
                    return false;
        }
                isValid = typeof(IEnumerable<>).MakeGenericType(genTypeArgs)
                    .IsAssignableFrom(target);

            }
            return isValid;
        }
        public static bool IsListTargetType<T>()
        {
            return IsListTargetType(typeof(T));

        }
       
        /// <summary>
        /// Try to get an instantiable type that can be assigned to type (for list types only right now)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="equivalentType"></param>
        /// <returns></returns>
        public static bool TryGetInstantiableType(Type type, out Type equivalentType)
        {
            if (IsListTargetType(type))
            {
                Type[] genericArgs;
                //Type vt = value.GetType();
                if (type.IsGenericType)
                {
                    genericArgs = type.GetGenericArguments();
                }
                else
                {
                    genericArgs = new Type[1] { typeof(object) };
                }
                equivalentType = typeof(List<>).MakeGenericType(genericArgs);
                return true;
            }
            else
            {
                equivalentType = null;
                return false;
            }

        }
        public static bool TryGetInstantiableType<T>(out Type equivalentType)
        {
            return TryGetInstantiableType(typeof(T),out equivalentType);
        }
        public static bool IsGenericEnumerableType(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }
            Type generic = type.GetGenericTypeDefinition();
            return typeof(IEnumerable).IsAssignableFrom(type)
                || generic.IsAssignableFrom(typeof(IEnumerable<>));
        }

        /// <summary>
        /// Try to convert from one type to another. This will only try to map IConvertible types, use TryParse to attempt
        /// to map incompatible types with possibly compatible values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool TryChangeType(object value,  Type type, out object converted)
        {
            bool result;
            try
            {
                converted = ChangeType(value, type);
                result = true;
            }
            catch
            {
                converted = null;
                result = false;
            }
            return result;
        }
        #endregion

        #region private methods

        private static bool IsNumericBaseType(Type type)
        {
            return type.IsValueType && !(type == typeof(string) || type == typeof(char) || type == typeof(bool) || type == typeof(DateTime));
        }

        /// <summary>
        /// Returns an Object with the specified Type and whose value is equivalent to the specified object.
        /// </summary>
        /// <param name="value">An Object that implements the IConvertible interface.</param>
        /// <param name="conversionType">The Type to which value is to be converted.</param>
        /// <returns>An object whose Type is conversionType (or conversionType's underlying type if conversionType
        /// is Nullable&lt;&gt;) and whose value is equivalent to value. -or- a null reference, if value is a null
        /// reference and conversionType is not a value type.</returns>
        /// <remarks>
        /// This method exists as a workaround to System.Convert.ChangeType(Object, Type) which does not handle
        /// nullables as of version 2.0 (2.0.50727.42) of the .NET Framework. The idea is that this method will
        /// be deleted once Convert.ChangeType is updated in a future version of the .NET Framework to handle
        /// nullable types, so we want this to behave as closely to Convert.ChangeType as possible.
        /// This method was written by Peter Johnson at:
        /// http://aspalliance.com/author.aspx?uId=1026.
        /// </remarks>
        public static object ChangeType(object value, Type conversionType)
        {
            // Note: This if block was taken from Convert.ChangeType as is, and is needed here since we're
            // checking properties on conversionType below.
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            } // end if

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType

            if (conversionType.IsGenericType &&
              conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                // It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
                // InvalidCastException (per http://weblogs.asp.net/pjohnson/archive/2006/02/07/437631.aspx),
                // determine what the underlying type is
                // If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
                // have a type--so just return null
                // Note: We only do this check if we're converting to a nullable type, since doing it outside
                // would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
                // value is null and conversionType is a value type.
                if (value == null)
                {
                    return null;
                } // end if

                // It's a nullable type, and not null, so that means it can be converted to its underlying type,
                // so overwrite the passed-in conversion type with this underlying type
                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            } // end if

            // including enums as part of changeType since they should map directly to iConvertible things
            if (conversionType.IsEnum)
            {
                return System.Convert.ChangeType(Enum.Parse(conversionType, value.ToString()), conversionType);
            }
            else
            {
                // Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
                // nullable type), pass the call on to Convert.ChangeType
                return System.Convert.ChangeType(value, conversionType);
            }
        }
        private static bool TryParseNumber(string value, out object number, Type T)
        {
            double val;
            number = 0;
            if (double.TryParse(value, out val))
            {
                if (T == typeof(int))
                {
                    number = System.Convert.ToInt32(Math.Round(val));
                }
                else if (T == typeof(long))
                {
                    number = System.Convert.ToInt64(Math.Round(val));
                }
                else if (T == typeof(double))
                {
                    number = System.Convert.ToDouble(val);
                }
                else if (T == typeof(decimal))
                {
                    number = System.Convert.ToDecimal(val);
                }
                else if (T == typeof(float))
                {
                    number = System.Convert.ToSingle(val);
                }
                else
                {
                    throw new Exception("Unhandled type for TryParseNumber: " + T.GetType().ToString());
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        
        #endregion

    }
}
