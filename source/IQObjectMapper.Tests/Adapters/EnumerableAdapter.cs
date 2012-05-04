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
    public class EnumerableAdapter_
    {

        [TestMethod]
        public void WrapperBasic()
        {
            var el1 = KeyValueBuilder.KeyValueDict<string, object>()
                .Add("stringProp", "quick brown fox")
                .Add("IntArray", new List<int> { 1, 2, 4, 8 })
                .Add("DoubleProp", 3.14);
            
            var el2 = KeyValueBuilder.KeyValueDict<string, object>()
                .Add("stringProp", "row2")
                .Add("IntArray", new List<int> { 20,30 })
                .Add("StringList", new string[] { "a","b","c" });
            
            var list = new List<IEnumerable<KeyValuePair<string,object>>> { el1, el2 };

            var wrap = new TypedAdapter<TypedObject>(list);

            int row=0;
            foreach (var item in wrap) {
                switch(row) {
                    case 1:
                        Assert.AreEqual("quick brown fox", item.StringProp);
                        Assert.AreEqual(4, item.IntArray[2]);
                        Assert.AreEqual(3.14, item.DoubleProp);
                        Assert.AreEqual(null, item.StringList);
                        break;
                    case 2:
                        Assert.AreEqual("row2", item.StringProp);
                        Assert.AreEqual(20, item.IntArray[0]);
                        Assert.AreEqual(2, item.IntArray.Length);
                        Assert.AreEqual("b", item.StringList[2]);

                        break;
                }

            }
        }

    }
}
