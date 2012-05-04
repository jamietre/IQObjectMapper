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
using IQObjectMapper.Impl;

using IQObjectMapper.Adapters;

namespace IQObjectMapper.Tests
{
    [TestClass]
    public class DelegateEnumerableAdapter_
    {

        [TestMethod]
        public void TypedObject()
        {
            var testObj = TestConfig.GetTypedObject();
            Assert.AreEqual(3, TestTheObject(testObj));
        }
        [TestMethod]
        public void AnonymousObect() 
        {

            var anonObj = TestConfig.GetAnonObject();
            Assert.AreEqual(2, TestTheObject(anonObj));
        }

        protected int TestTheObject(object obj)
        {
            int count = 0;
            //var wrap = new ClassInfoEnumerableAdapter(obj.GetType());
            var opts = ReflectionOptions.Default();
            opts.IncludeFields = true;
            var wrap = ObjectMapper.MapperCache.GetClassInfo(obj.GetType(), opts).Data.Values;

        
            foreach (IDelegateInfo item in wrap)
            {
                switch (item.Name.ToLower())
                {
                    case "stringprop":
                        Assert.AreEqual(typeof(string), item.Type);
                        Assert.AreEqual("quick brown fox", item.GetValue(obj));
                        count++;
                        break;
                    case "intarray":
                        Assert.AreEqual(typeof(int[]), item.Type);
                        int[] val = (int[])item.GetValue(obj);

                        Assert.AreEqual(2, val[1]);
                        count++;
                        break;
                    case "stringfield":
                        Assert.AreEqual(typeof(string), item.Type);
                        Assert.AreEqual("field", item.GetValue(obj));
                        count++;
                        break;
                }

            }
            return count;

        }
    }
}
