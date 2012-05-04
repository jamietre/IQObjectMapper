using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using System.ComponentModel;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using IQObjectMapper;
using IQObjectMapper.Adapters;

namespace IQObjectMapper.Tests
{
    [TestClass]
    public class DynamicAdapter_
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
            Assert.AreEqual(13, dict.Count);

            dyn.stringfield = "New string data";
            Assert.AreEqual(13, dict.Count);
            Assert.AreEqual("New string data", dyn.Stringfield);
            Assert.AreEqual("New string data", testObj.StringField);

            dyn.MyNewProp = "added data";
            Assert.AreEqual(14, dict.Count);
            Assert.AreEqual("added data", dyn.mynewprop);

        }
        
        /// <summary>
        /// This test ensures null handling is correct. Null values should work same as any other
        /// </summary>
        [TestMethod]
        public void NullValues()
        {
            var testObj = new TypedObject();
            var dict = new DynamicAdapter(testObj);
            dynamic dyn = dict;

            dict.Options.CanAlterProperties = true;

            int count = dict.Count;

            testObj.ObjectProp = null;
            dyn.NewProp = null;
            Assert.AreEqual(count + 1, dict.Count, "New property added");

            Assert.IsNull(dict["objectprop"]);
            Assert.IsNull(dict["newprop"]);
            Assert.AreEqual(Undefined.Value,dict["noprop"]);

            testObj.ObjectProp = 1;
            dict["newprop"] = 2;
            Assert.AreEqual(1,dict["objectprop"]);
            Assert.AreEqual(2,dict["newprop"]);

        }

        [TestMethod]
        public void Alter()
        {
            var testObj = new TypedObject();
            var dict = new DynamicAdapter(testObj, new MapOptions { 
                CanAlterProperties = false
            });
            
            dynamic dyn = dict;
            

            Assert.Throws<InvalidOperationException>(() =>
            {
                dyn.newprop = "newdata";
            }, "Can't add a prop");

            Assert.Throws<InvalidOperationException>(() =>
            {
                dict.Remove("Stringprop");
            }, "Can't rmove a prop");
            
            // we can update with just CanAlterProprerties=false but IsReadOnly=true

            dyn.stringprop = "stringdata";

            dict.Options.CanAlterProperties = true;

            dyn.newprop = "newdata";
            Assert.AreEqual("newdata",dict["Newprop"]);
            
            Assert.IsTrue(dict.ContainsKey("stringprop"));
            dict.Remove("Stringprop");
            Assert.IsFalse(dict.ContainsKey("stringprop"));
        }
        [TestMethod]
        public void Remove()
        {
            var testObj = new TypedObject();
            var dict = new DynamicAdapter(testObj);
            dynamic dyn=dict;

            int count = dict.Count;
            dyn.newprop = "newdata";
            Assert.AreEqual(count + 1, dict.Count, "Added a property");

            Assert.IsTrue(dict.Remove("stringprop"));
            Assert.AreEqual(count, dict.Count, "Removed a property");

            dict["doubleprop"] = 5.67;
            Assert.IsFalse(dict.Remove(new KeyValuePair<string,object>("doubleprop",1.23)),
                "Can't remove KVP with diff value");
            Assert.IsTrue(dict.ContainsKey("DoubleProp"));

            Assert.IsTrue(dict.Remove(new KeyValuePair<string,object>("doubleprop",5.67)),
                "Removed KVP with wrong case");
            Assert.IsFalse(dict.ContainsKey("DoubleProp"));

            Assert.AreEqual(count - 1, dict.Count, "Count is correct at end");
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

        [TestMethod]
        public void Options()
        {
            TypedObject test = GetTestObject();

            dynamic target = new DynamicAdapter(test,new MapOptions { 
                CaseSensitive = true, 
                CanAlterProperties = false, 
                CanAccessMissingProperties = false 
            });

            // todo move this test to poco2dict
            //Assert.AreEqual(ObjectMapper.DefaultOptions.DynamicObjectType, target.GetType(), "Default dynamic type is created");

            Assert.AreEqual(true, target.BoolProp);

            Assert.Throws<RuntimeBinderException>(() =>
            {
                var t = target.boolProp;
            });
            Assert.Throws<InvalidOperationException>(() =>
            {
                target.newProp = "test";
            });

            // change to allow accessing missing

            target = new DynamicAdapter(test, new MapOptions
            {
                CaseSensitive = true,
                CanAlterProperties = false,
                CanAccessMissingProperties = true
            });
            
            Assert.AreEqual(Undefined.Value, target.boolprop);

            // most permissive
            target = new DynamicAdapter(test, new MapOptions
            {
                CaseSensitive = false,
                UndefinedValue = null 
            });

            Assert.AreEqual(1.2, target.doubleprop);

            target.newProp = "test";
            Assert.AreEqual("test", target.newprop, "Created property & accessed it with different case");
            Assert.AreEqual(null, target.missingprop, "The custom undefined value was used for a missing property.");

        }

 

        protected TypedObject GetTestObject()
        {
            return new TypedObject
            {
                BoolProp = true,
                DoubleProp = 1.2,
                IntArray = new int[] { 1, 3, 5 },
                ObjectEnumerable = new object[]
                {
                    false,
                    1,
                    new TypedObject()
                }
            };

        }
    }
}
