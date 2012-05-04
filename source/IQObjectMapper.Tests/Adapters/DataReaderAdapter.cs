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
using IQObjectMapper.Adapters;

namespace IQObjectMapper.Tests
{
    [TestClass]
    public class DataReaderAdapter_
    {

        protected string TestQuery
        {
            get
            {
                return "select 1 as IntProp, 'quick brown fox' as StringProp, GETDATE() as DateTimeProp, 1.23 as MismatchedDouble UNION "
                + "select 5 as IntProp, 'lazy dogs' as StringProp, GETDATE() as DateTimeProp, 2.2 as MismatchedDouble";
            }
        }

        [TestMethod]
        public void KeyValuePairAdapter()
        {

            string sql = TestQuery;

            IDataReader reader = TestConfig.RunSql(sql);
            var map= new DataReaderAdapter(reader);

            int count=0;

            foreach (var item in map)
            {
                switch (count)
                {
                    case 0:
                        Assert.AreEqual("IntProp", item.First().Key, "Row 1 Column 1");
                        Assert.AreEqual(1, item.First().Value, "Row 1 Column 1");
                        Assert.AreEqual("StringProp", item.ElementAt(1).Key, "Row 1 Column 2");
                        Assert.AreEqual("quick brown fox", item.ElementAt(1).Value, "Row 1 Column 2");
                        Assert.AreEqual("DateTimeProp", item.ElementAt(2).Key, "Row 1 Column 3");
                        Assert.IsTrue(item.ElementAt(2).Value.GetType() == typeof(DateTime), "Row 1 Column 3");
                        Assert.AreEqual(1.23, item.ElementAt(3).Value );
                        break;
                    case 1:
                        Assert.AreEqual(5, item.First().Value, "Row 1 Column 1");
                        Assert.AreEqual("lazy dogs", item.ElementAt(1).Value, "Row 1 Column 2");
                        
                        break;

                }
                count++;
            }
            Assert.AreEqual(2, count, "There were 2 rows.");


        }
        [TestMethod]
        public void TypedAdapter()
        {

            string sql = TestQuery;

            IDataReader reader = TestConfig.RunSql(sql);
            var map = new DataReaderAdapter<TypedObject>(reader);

            int count = 0;

            foreach (var item in map)
            {
                switch (count)
                {
                    case 0:
                        Assert.AreEqual(1, item.IntProp, "Row 1 Column 1");
                        Assert.AreEqual("quick brown fox", item.StringProp, "Row 1 Column 2");
                        
                        Assert.IsTrue(item.DateTimeProp > DateTime.Now-TimeSpan.FromMinutes(10) && 
                            item.DateTimeProp < DateTime.Now + TimeSpan.FromMinutes(10), "Row 1 Column 3");
                        break;
                    case 1:
                        Assert.AreEqual(5, item.IntProp, "Row 1 Column 1");
                        Assert.AreEqual("lazy dogs", item.StringProp, "Row 1 Column 2");

                        break;

                }
                count++;
            }
            Assert.AreEqual(2, count, "There were 2 rows.");


        }
    }
}
