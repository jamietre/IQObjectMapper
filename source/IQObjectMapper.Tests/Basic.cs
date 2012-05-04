using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using System.Data;
using System.Web.Script.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using IQObjectMapper;
using IQObjectMapper.Impl;
using IQObjectMapper.Adapters;

namespace IQObjectMapper.Tests
{
    [TestClass]
    public class Basic_
    {
       

        [TestMethod]
        public void Properties()
        {
            

            var ci = ObjectMapper.MapperCache.GetClassInfo(typeof(TypedObject), MapOptions.Default());

            Assert.AreEqual(13, ci.Count);


            var fi = ci["doubleprop"];
            Assert.IsNotNull(fi);
            Assert.IsFalse(fi.IsReadOnly);
            Assert.IsTrue(fi.IsProperty);
            Assert.IsFalse(fi.IsField);
            Assert.IsFalse(fi.IsPrivate);
            Assert.IsTrue(fi.IsDeclared);



        }
        [TestMethod]
        public void Derived()
        {
            var opts = MapOptions.Default();

            var ci = ObjectMapper.MapperCache.GetClassInfo(typeof(TypedObject_Derived),opts);

            Assert.AreEqual(15, ci.Count);

            var fi = ci["stringprop"];
            Assert.IsTrue(fi.IsDeclared);
            Assert.AreEqual("derived_value", fi.GetValue(new TypedObject_Derived()));
            
            fi = ci["intprop"];
            Assert.IsFalse(fi.IsDeclared);

            opts.DeclaredOnly = true;
            ci = ObjectMapper.MapperCache.GetClassInfo(typeof(TypedObject_Derived), opts);

            Assert.AreEqual(3, ci.Count);
        }
        [TestMethod]
        public void Case()
        {
            var opts = MapOptions.Default();
            opts.CaseSensitive = true;

            var ci = ObjectMapper.MapperCache.GetClassInfo(typeof(TypedObject), opts);
            
            Assert.IsFalse(ci.ContainsField("stringprop"));

            opts.CaseSensitive = false;
            ci = ObjectMapper.MapperCache.GetClassInfo(typeof(TypedObject), opts);
            Assert.IsTrue(ci.ContainsField("stringprop"));
            var fi = ci["stringprop"];
            Assert.IsNotNull(fi);

        }

        class MyObject
        {
            public String name = "Dan";
            public int age = 88;
            public  object[] children = new object[] { "a", 1, false };
        }
            
        [TestMethod]
        public void JSON()
        {
            var obj = new MyObject();
            
            var dict = ObjectMapper.ToDictionary(obj );

            foreach (var info in ObjectMapper.GetClassInfo<MyObject>())
            {
                if (info.Type.IsClass)
                {
                    dict[info.Name] = null;
                }
            }

            JavaScriptSerializer ser = new JavaScriptSerializer();
            string json = ser.Serialize(ObjectMapper.ToNew<MyObject>(dict));

            Assert.AreEqual("{\"name\":null,\"age\":88,\"children\":null}", json);


            // another way

            var parsed = ObjectMapper.Map(obj,(del,value) => {
                if (value !=null && value.GetType().IsClass) {
                    return null;
                } else {
                    return value;
                }
            });

            json = ser.Serialize(ObjectMapper.ToNew<MyObject>(dict));
            Assert.AreEqual("{\"name\":null,\"age\":88,\"children\":null}", json);

        }

     
    }
}
