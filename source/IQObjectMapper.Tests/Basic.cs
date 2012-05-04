using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using System.Data;
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

            Assert.AreEqual(12, ci.Count);


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

            Assert.AreEqual(14, ci.Count);

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

     
    }
}
