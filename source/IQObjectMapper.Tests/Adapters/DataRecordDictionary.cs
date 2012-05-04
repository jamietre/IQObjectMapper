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
    public class DataRecordDictionary__
    {
        protected MapOptions caseOpts = new MapOptions {CaseSensitive=false};

        protected string TestQuery
        {
            get
            {
                return "select 1 as IntProp, 'quick brown fox' as StringProp, GETDATE() as DateTimeProp, 1.23 as MismatchedDouble UNION "
                + "select 5 as IntProp, 'lazy dogs' as StringProp, GETDATE() as DateTimeProp, 2.2 as MismatchedDouble";
            }
        }

        [TestMethod]
        public void Basic()
        {

            string sql = TestQuery;

            using (IDataReader reader = TestConfig.RunSql(sql))
            {
                IDictionary<string,object> map;
                Assert.Throws<InvalidOperationException>(() =>
                {

                    map = new DataRecordDictionary(reader,caseOpts);
                }, "Fails when not on a record");

                reader.Read();
                map = new DataRecordDictionary(reader,caseOpts);
                Assert.AreEqual(1, map["intprop"]);
                Assert.AreEqual("quick brown fox", map["stringprop"]);

                reader.Read();
                map = new DataRecordDictionary(reader, caseOpts);
                Assert.AreEqual(5, map["intprop"]);
                Assert.AreEqual("lazy dogs", map["stringprop"]);

            }



        }
        
        
    }
}
