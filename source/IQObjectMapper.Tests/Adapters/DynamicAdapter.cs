using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using IQObjectMapper;
using IQObjectMapper.Adapters;

namespace IQObjectMapper.Tests
{
    [TestClass]
    public class DynamicADapter_
    {

        [TestMethod]
        public void WrapperBasic()
        {
            var testObj = new TypedObject
            {
                StringProp="quick brown fox",
                IntArray =new int[] { 1, 2, 4, 8 },
                DoubleProp = 3.14,
                StringList =  new List<string> { "a","b","c" }
            
            };
            dynamic dict = new DynamicAdapter(testObj);

            Assert.AreEqual("quick brown fox", dict.StringProp);
            Assert.AreEqual(4, ((int[])dict.IntArray)[2]);
            Assert.AreEqual(3.14, dict.DoubleProp);
            Assert.AreEqual("b", ((IList<string>)dict.StringList)[1]);
            
        }
        [TestMethod]
        public void Updating()
        {
            var testObj = new TypedObject();
            dynamic dict = new DynamicAdapter(testObj);

            Assert.AreEqual(0d, testObj.DoubleProp);
            dict.doubleprop = 3.14;
            Assert.AreEqual(3.14, testObj.DoubleProp);

            Assert.Throws<InvalidCastException>(() =>
            {
                dict.doubleprop = "abc";
            }, "Can't assign bad data type");            
        }

        [TestMethod]
        public void Add()
        {
            var testObj = new TypedObject();
            var dict = new DynamicAdapter(testObj);
            dict.Options.CanAlterProperties = true;

            dynamic dyn = dict;
            Assert.AreEqual(12, dict.Count);

            dyn.stringfield = "New string data";
            Assert.AreEqual(12, dict.Count);
            Assert.AreEqual("New string data", dyn.Stringfield);
            Assert.AreEqual("New string data", testObj.StringField);

            dyn.MyNewProp = "added data";
            Assert.AreEqual(13, dict.Count);
            Assert.AreEqual("added data", dyn.mynewprop);

        }


        [TestMethod]
        public void Alter()
        {
            var testObj = new TypedObject();
            var dict = new DynamicAdapter(testObj);
            dynamic dyn = dict;
            
            dict.Options.CanAlterProperties = false;

            Assert.Throws<RuntimeBinderException>(() =>
            {
                dyn.newprop = "newdata";
            }, "Can't add a prop");

            Assert.Throws<Exception>(() =>
            {
                dict.Remove("Stringprop");
            }, "Can't add a prop");
            
            // These are OK to do
            dyn.stringprop = "stringdata";


            dict.Options.CanAlterProperties = true;

            dyn.newprop = "newdata";
            Assert.AreEqual("newdata",dict["Newprop"]);
            
            Assert.IsTrue(dict.ContainsKey("stringprop"));
            dict.Remove("Stringprop");
            Assert.IsFalse(dict.ContainsKey("stringprop"));
        }

        [TestMethod]
        public void Contains()
        {
            var testObj = new TypedObject();
            var dict = new PropertyDictionaryAdapter(testObj);
           
            var intArray = new int[] {1,2,3};
            testObj.IntArray = intArray;
            Assert.IsTrue(dict.Contains(new KeyValuePair<string,object>("intarray",intArray)));
            Assert.IsFalse(dict.Contains(new KeyValuePair<string,object>("intarray",new int[] {1,2,3})));
        }
    }
}
