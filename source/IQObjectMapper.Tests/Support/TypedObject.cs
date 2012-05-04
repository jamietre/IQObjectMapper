using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQObjectMapper.Tests
{
    public class TypedObject : ITypedObject
    {
        public virtual string StringProp { get; set; }
        public int IntProp { get; set; }
        public int[] IntArray { get; set; }
        public double DoubleProp { get; set; }
        public IList<string> StringList { get; set; }
        public byte[] ByteArray { get; set; }
        public IEnumerable<object> ObjectEnumerable { get; set; }
        public dynamic DynamicObjectProp { get; set; }
        public object ObjectProp { get; set; }
        public DateTime DateTimeProp { get; set; }
        public virtual bool BoolProp { get; set; }

        public string StringField;
        public double DoubleField;
    }

    public class TypedObject_Derived : TypedObject
    {
        public string DerivedProperty { get; set; }
        public string DerivedField;
        public override string StringProp
        {
            get
            {
                return "derived_value";
            }
            set
            {
                base.StringProp = value;
            }
        }
        
    }
    public class TypedObject_Parameters: TypedObject
    {
        public TypedObject_Parameters(string value)
        {
            StringProp = value;
        }
    }
    public interface ITypedObject
    {
        string StringProp { get; set; }
        int[] IntArray { get; set; }

    }
}

