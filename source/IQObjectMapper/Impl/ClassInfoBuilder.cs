using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace IQObjectMapper.Impl
{
    public interface IClassInfoBuilder
    {
        IClassInfo MapClass(Type type);
        IClassInfo ComposeClass(IClassInfo classInfo, IClassInfoData data);
    }
    /// <summary>
    /// Generates ClassInfo data from POCOs
    /// </summary>
    public class ClassInfoBuilder<T>: IClassInfoBuilder 
        where T: ClassInfo, new() 
    {
        public ClassInfoBuilder(IMapOptions options, Type delegateGenericType)
        {
            Options = MapOptions.From(options);
            DelegateGenericType = delegateGenericType;
        }
        protected Type DelegateGenericType { get; set; }
        protected Type Type { get; set; }
        protected MapOptions Options;
        /// <summary>
        /// Exclude properties by default
        /// </summary>
        public bool ExcludeByDefault { get; protected set; }

        private IDictionary<string, DelegateInfo> _TempDelegateInfo;
        protected IDictionary<string, DelegateInfo> TempDelegateInfo
        {
            get
            {
                if (_TempDelegateInfo == null)
                {
                    _TempDelegateInfo = new Dictionary<string, DelegateInfo>(Options.CaseSensitive ?
                        StringComparer.Ordinal :
                        StringComparer.OrdinalIgnoreCase);
                }
                return _TempDelegateInfo;
            }
        }
        private ClassInfoData _Data;
        protected ClassInfoData Data
        {
            get
            {
                if (_Data == null)
                {
                    _Data = new ClassInfoData(Type,Options);
                }
                return _Data;
            }
        }
        public virtual IClassInfo ComposeClass(IClassInfo classInfo, IClassInfoData data)
        {
            T cInfo = new T();
            cInfo.Type = classInfo.Type;
            cInfo.Data = data;
            return cInfo;
        }
        /// <summary>
        /// Main entry point: map a class to ClassInfo
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tableName"></param>
        /// <param name="mappedFields"></param>
        public virtual IClassInfo MapClass(Type type)
        {
            T cInfo = new T();
            Type = type;
            cInfo.Type = Type;

            GetClassMetadata();
            GetClassPropertiesMethods();
            MapFromTemporaryFields();

            if (Data.Count == 0)
            {
                throw new Exception("There were no databound fields in the object.");
            }
            cInfo.Data = Data;

            TempDelegateInfo.Clear();
            return cInfo;

        }
        #region private methods

        /// <summary>
        /// Override to do stuff like add attribute data
        /// </summary>
        protected virtual void GetClassMetadata()
        {

        }

        /// <summary>
        /// Override to process attributes during reflection. It can update delInfo based on property values.
        /// </summary>
        protected virtual void GetPropertyMetadata(MemberInfo member, object[] attributes, DelegateInfo delInfo)
        {

        }
        /// <summary>
        /// Override to process members
        /// </summary>
        /// <param name="?"></param>
        /// <param name="?"></param>
        protected virtual void GetMethodMetadata(MethodInfo member, object[] attributes)
        {

        }


        //protected virtual TemporaryField CreateTemporaryField()
        //{
        //    return new TemporaryField();
        //}

        protected virtual void GetClassPropertiesMethods()
        {

            // It's a regular object. It cannot be extended, but set any same-named properties.
            IEnumerable<MemberInfo> members = Type.GetMembers(BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance |
                            BindingFlags.Static);

            foreach (var member in members)
            {

                DelegateInfo delInfo = null;

                if (member is PropertyInfo)
                {
                    delInfo = CreateForProperty((PropertyInfo)member);
                }
                else if (member is FieldInfo)
                {
                    delInfo = CreateForField((FieldInfo)member);
                }
                else if (member is MethodInfo)
                {
                    if (Types.IsAnonymousType(Type))
                    {
                        delInfo = CreateForAnon((MethodInfo)member);
                    }
                    else
                    {
                        // for methods of non-anonymous types, determine if it's a constructor
                        MethodInfo method = (MethodInfo)member;
                        object[] attributes = (object[])member.GetCustomAttributes(true);
                        GetMethodMetadata(method, attributes);
                    }
                }
                if (delInfo != null)
                {
                    string name = delInfo.Name;
                    object[] attributes = (object[])member.GetCustomAttributes(true);

                    GetPropertyMetadata(member, attributes, delInfo);

                    if (delInfo.Include != false)
                    {
                        if (!TempDelegateInfo.ContainsKey(name))
                        {
                            TempDelegateInfo[name] = delInfo;
                        }
                    }
                    else
                    {
                        TempDelegateInfo.Remove(name);
                    }
                }
            }
        }

        protected DelegateInfo CreateForProperty(PropertyInfo pi)
        {
            DelegateInfo dInfo = null;

            // Skip properties with no indexers
            if (pi.GetIndexParameters().Length == 0)
            {
                string name = pi.Name;

                // Use the fieldmap info if it exists already as a starting point - but override with any attribute data
                if (!DeclaredDelegateExists(name)) 
                {
                   dInfo = GetDelegateInfo(name,
                        pi.GetGetMethod(true),
                        pi.GetSetMethod(true),
                        pi.PropertyType);
                    
                    // Always update name from property, even if field was already mapped from constructor
                    dInfo.HasPublicGetter = pi.GetGetMethod() != null;
                    dInfo.IsProperty = true;
                    dInfo.IsField = false;
                    dInfo.IsDeclared = pi.DeclaringType == Type;
                    dInfo.IsReadOnly = !pi.CanRead;
                }
            }
            return dInfo;
        }
        protected DelegateInfo CreateForAnon(MethodInfo mi)
        {
            string name = mi.Name;
            DelegateInfo dInfo=null;
            if (name.Length >= 4 && name.StartsWith("get_"))
            {
                name = name.Substring(4);
                if (!DeclaredDelegateExists(name))
                {
                    dInfo= GetDelegateInfo(name, mi, null, mi.ReturnType);
                    dInfo.Name = name;

                    dInfo.HasPublicGetter = true;
                    dInfo.IsProperty = true;
                    dInfo.IsField = false;
                    dInfo.IsDeclared = true;
                    dInfo.IsReadOnly = false;
                }
            }
            return dInfo;

        }
        protected DelegateInfo CreateForField(FieldInfo fi)
        {
            string name = fi.Name;

            if (DeclaredDelegateExists(name) 
                || name.EndsWith("__BackingField"))
            {
                return null;
            }
            // see http://www.codeproject.com/Articles/14560/Fast-Dynamic-Property-Field-Accessors
            // have not done extensive comparison but there is no way to create a typed delegate using
            // delegate.createdelegate for fields, and this is indeed fast so i see no reason not to use it

            DynamicMethod getDm = new DynamicMethod("Get" + name, fi.FieldType, new Type[] { Type }, Type);
            ILGenerator il = getDm.GetILGenerator();
            // Load the instance of the object (argument 0) onto the stack
            il.Emit(OpCodes.Ldarg_0);
            // Load the value of the object's field (fi) onto the stack
            il.Emit(OpCodes.Ldfld, fi);
            // return the value on the top of the stack
            il.Emit(OpCodes.Ret);

            DynamicMethod setDm = new DynamicMethod("Set" + name, typeof(void), new Type[] { Type, fi.FieldType }, Type);
            il = setDm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);       // Load instance of the object onto eval stack
            il.Emit(OpCodes.Ldarg_1);       // Load 2nd arg, i.e., value
            il.Emit(OpCodes.Stfld, fi);    // Store value into field
            il.Emit(OpCodes.Ret);

            // orig code
            Type getInvokeType = typeof(Func<,>).MakeGenericType(new Type[] { Type, fi.FieldType });
            Type setInvokeType = typeof(Action<,>).MakeGenericType(new Type[] { Type, fi.FieldType });
            Type dInfoType = DelegateGenericType.MakeGenericType(Type, fi.FieldType);

            Delegate getDelegate = getDm.CreateDelegate(getInvokeType);
            Delegate setDelegate = setDm.CreateDelegate(setInvokeType);

            DelegateInfo dInfo = (DelegateInfo)Activator.CreateInstance(dInfoType, getDelegate, setDelegate);

            dInfo.Name = fi.Name;

            // Always update name from property, even if field was already mapped from constructor
            dInfo.Name = fi.Name;
            dInfo.HasPublicGetter = fi.IsPublic;
            dInfo.IsProperty = false;
            dInfo.IsField = true;
            dInfo.IsDeclared = fi.DeclaringType == Type;
            dInfo.IsReadOnly = false;
            return dInfo;

        }
        /// <summary>
        /// If there's an existing delegate by the same name, remove it. Return whether or not there's a decared one.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        protected bool DeclaredDelegateExists(string name)
        {
            DelegateInfo delInfo;
            if (TempDelegateInfo.TryGetValue(name, out delInfo))
            {
                if (!delInfo.IsDeclared)
                {
                    TempDelegateInfo.Remove(name);
                }
                return delInfo.IsDeclared;
            }
            return false;
        }
        /// <summary>
        /// Take all the info we've gathered form constructors, parameters, attributes, etc. and build the map
        /// </summary>
        protected virtual void MapFromTemporaryFields()
        {
            int index=0;

            foreach (var item in TempDelegateInfo.Values)
            {

                bool include =!ExcludeByDefault
                    || item.Include==true;
                bool exclude = item.Include == false;
                
                if (include && !exclude)
                {
                    item.Index = index++;
                    Data[item.Name] = item;

                }

            }
            _TempDelegateInfo = null;

        }
        protected DelegateInfo GetDelegateInfo(string name, MethodInfo getMethodInfo, MethodInfo setMethodInfo, Type returnType)
        {
            Type getInvokeType = typeof(Func<,>).MakeGenericType(new Type[] { Type, returnType });
            Delegate getInvokeMethod = Delegate.CreateDelegate(getInvokeType, getMethodInfo);
            Delegate setInvokeMethod = null;

            if (setMethodInfo != null)
            {
                var setInvokeType = typeof(Action<,>).MakeGenericType(new Type[] { Type, returnType });
                setInvokeMethod = Delegate.CreateDelegate(setInvokeType, setMethodInfo);
            }


            Type dInfoType = DelegateGenericType.MakeGenericType(Type, returnType);
            DelegateInfo dInfo = (DelegateInfo)Activator.CreateInstance(dInfoType, getInvokeMethod, setInvokeMethod);
            dInfo.Name = name;
            return dInfo;

        }
        #endregion
    }
}
