using System;
using System.Text;
using System.Collections;
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

namespace IQObjectMapper.Tests.Impl
{
    [TestClass]
    public class Object2Object_
    {
       
        [TestMethod]
        public void ArraysLists()
        {
            var o2o = new Object2Object();
            var intArr = new int[] { 1, 2, 3 };

            var list = o2o.Map(intArr, typeof(IList<int>));
            Assert.AreEqual(list.GetType(),typeof(int[]),"int[] is directly assignable to IList<int>, should not change");

            HashSet<int> hs = new HashSet<int>(intArr);
            list = o2o.Map(hs, typeof(IList<int>));
            Assert.AreEqual(list.GetType(), typeof(List<int>), "A generic enumerable type becomes List<t>");

            var col = new ArrayList();
            foreach (var item in intArr)
            {
                col.Add(item);
            }
            list = o2o.Map(col, typeof(ICollection<int>));
            Assert.AreEqual(list.GetType(), typeof(List<int>), "Works from nongeneric sources");

            var objList = new List<object> {"a",1,false};

            Assert.Throws<InvalidCastException>(() =>
            {
                list = o2o.Map(objList, typeof(ICollection<int>));
            }, "Can't map incompatible lists");

            var stringList = o2o.Map<string[]>(objList);

            Assert.AreEqual(new string[] {"a","1","False"},stringList,"Automatic type conversion for array elements");

        }
        [TestMethod]
        public void Parse()
        {
            var o2o = new Object2Object();
            o2o.Options.ParseValues = false;

            Assert.AreEqual(typeof(int),o2o.Map(1.0d,typeof(int)).GetType());
            Assert.Throws<InvalidCastException>(()=> {
                o2o.Map<int>("1.0");
            });

            // we do more extensive testign of Parse under Types. This is mostly to ensure the option flag works.

            o2o.Options.ParseValues = true;
            Assert.AreEqual(1, o2o.Map<int>("1.0"),"Parsing option enabled");
            Assert.AreEqual(false, o2o.Map("no",typeof(bool)), "Parsing option enabled");
        }
     
    }
}
