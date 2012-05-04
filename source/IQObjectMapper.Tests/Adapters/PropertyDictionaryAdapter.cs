using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using IQObjectMapper;
using IQObjectMapper.Adapters;
using Microsoft.CSharp.RuntimeBinder;

namespace IQObjectMapper.Tests
{
    [TestClass]
    public class PropertyDictionaryAdapter_
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
            var dict = new PropertyDictionaryAdapter(testObj);

            Assert.AreEqual("quick brown fox", dict["StringProp"]);
            Assert.AreEqual(4, ((int[])dict["IntArray"])[2]);
            Assert.AreEqual(3.14, dict["DoubleProp"]);
            Assert.AreEqual("b", ((IList<string>)dict["StringList"])[1]);
            
        }
        [TestMethod]
        public void Updating()
        {
            var testObj = new TypedObject();
            var dict = new PropertyDictionaryAdapter(testObj);

            Assert.AreEqual(0d, testObj.DoubleProp);
            dict["doubleprop"] = 3.14;
            Assert.AreEqual(3.14, testObj.DoubleProp);

            Assert.Throws<InvalidCastException>(() =>
            {
                dict["doubleprop"] = "abc";
            }, "Can't assign bad data type");            
        }

        [TestMethod]
        public void Add()
        {
            var testObj = new TypedObject();
            var dict = new PropertyDictionaryAdapter(testObj);

            Assert.AreEqual(13, dict.Count);
            
            dict["stringfield"] = "New string data";
            Assert.AreEqual(13, dict.Count);
            Assert.AreEqual("New string data", dict["Stringfield"]);
            Assert.AreEqual("New string data", testObj.StringField);

            dict["MyNewProp"] = "added data";
            Assert.AreEqual(14, dict.Count);
            Assert.AreEqual("added data", dict["mynewprop"]);

        }

        [TestMethod]
        public void Clear()
        {
            var testObj = new TypedObject();
            var dict = new PropertyDictionaryAdapter(testObj);
            testObj.StringField = "changed";

            Assert.AreEqual("changed", dict["stringfield"],"Propert of underlying object affected by a change to dict");

            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            dict["stringfield"] = "string data";
            
            Assert.AreEqual("changed", testObj.StringField,"Same-named property no longer affects object after a clear");
            Assert.AreEqual("string data", dict["stringfield"], "Same-named property no longer affects object after a clear");

        }
        [TestMethod]
        public void ContainsKey()
        {
            var testObj = new TypedObject();
            var dict = new PropertyDictionaryAdapter(testObj);

            Assert.IsTrue(dict.ContainsKey("stringprop"));
            Assert.IsFalse(dict.ContainsKey("random"));

            dict["random"] = "12adkla";
            Assert.IsTrue(dict.ContainsKey("random"));
            Assert.IsTrue(dict.Keys.Contains("random"));
        }



        [TestMethod]
        public void Parse()
        {
            var testObj = new TypedObject();
            var dict = new PropertyDictionaryAdapter(testObj);
            dict.Options.ParseValues = true;

            dict["doubleprop"] = "3.14";
            Assert.AreEqual(3.14d, testObj.DoubleProp);
            dict["boolprop"] = "false";
            Assert.AreEqual(false, testObj.BoolProp);

        }

        [TestMethod]
        public void Alter()
        {
            //var dict2 = new Dictionary<string, object>();
            //var x = dict2["abc"];

            var testObj = new TypedObject();
            var dict = new PropertyDictionaryAdapter(testObj);
            dict.Options.CanAlterProperties = false;

            Assert.Throws<InvalidOperationException>(() =>
            {
                dict["newprop"] = "newdata";
            }, "Can't add a prop");

            Assert.Throws<InvalidOperationException>(() =>
            {
                dict.Remove("Stringprop");
            }, "Can't add a prop");
            
            // These are OK to do
            dict["stringprop"] = "stringdata";


            dict.Options.CanAlterProperties = true;

            dict["newprop"] = "newdata";
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
            Assert.IsTrue(dict.Contains(new KeyValuePair<string,object>("IntArray",intArray)));
            Assert.IsFalse(dict.Contains(new KeyValuePair<string,object>("IntArray",new int[] {1,2,3})));
        }
    }
}
