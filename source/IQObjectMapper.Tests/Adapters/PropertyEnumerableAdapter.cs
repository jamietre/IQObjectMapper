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

namespace IQObjectMapper.Tests
{
    [TestClass]
    public class PropertyEnumerableAdapter_
    {

        [TestMethod]
        public void TypedObject()
        {
            var testObj = TestConfig.GetTypedObject();
            var wrap = new PropertyKVPAdapter(testObj);
            TestTheObject(wrap);
        }

        [TestMethod]
        public void AnonObj()
        {
            var testObj = TestConfig.GetAnonObject();
            var wrap = new PropertyKVPAdapter(testObj);
            TestTheObject(wrap);
        }

        [TestMethod]
        public void Deep()
        {
            var testObj = TestConfig.GetTypedObject();
            testObj.ObjectProp = TestConfig.GetAnonObject();

            var wrap = new PropertyKVPAdapter(testObj);
            wrap.Deep = true;
            TestTheObject(wrap);

            var obj = wrap.Where(item => item.Key == "ObjectProp").First().Value;
            
            Assert.IsFalse(ReferenceEquals(obj,testObj.ObjectProp),"The deep-copied object must not equal the original one");
            Assert.IsTrue(obj is IEnumerable<KeyValuePair<string, object>>);

            var kvp = (IEnumerable<KeyValuePair<string,object>>)obj;
            Assert.AreEqual("quick brown fox",
                (string)kvp.Where(item => item.Key == "StringProp").First().Value);

            wrap = new PropertyKVPAdapter(testObj,false, new MapOptions { CaseSensitive = false });

            obj = wrap.Where(item => item.Key == "ObjectProp").First().Value;
            Assert.IsTrue(ReferenceEquals(obj,testObj.ObjectProp),"The non-deep-copied object mustequal the original one");


        }

        protected int TestTheObject(PropertyKVPAdapter wrap)
        {
            int count = 0;
            
            wrap.Options.IncludeFields = true;
            wrap.Options.CaseSensitive = false;
            
            Assert.AreEqual("quick brown fox", 
                (string)wrap.Where(item => item.Key == "StringProp").First().Value);
            count++;

            int[] arr = (int[])wrap.Where(item => item.Key == "IntArray").First().Value;
            Assert.AreEqual(2,arr[1]);
            count++;

            if (wrap.Where(item => item.Key == "StringField").Count() > 0)
            {
                Assert.AreEqual("field",
                    (string)wrap.Where(item => item.Key == "StringField").First().Value);
                count++;
            }
            return count;

        }
    }
}
