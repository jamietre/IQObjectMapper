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
using IQObjectMapper.Impl;

namespace IQObjectMapper.Tests.Impl
{
    [TestClass]
    public class DataTypes
    {
        
        [TestMethod] 
        public void Binary() {
            string sql = String.Format("select {0} as ByteArray",TestConfig.HexStr("TestBinaryData"));

            var item= new DataReaderAdapter<TypedObject>(TestConfig.RunSql(sql)).First();

            Assert.AreEqual(System.Text.Encoding.ASCII.GetString(item.ByteArray),"TestBinaryData");
        }

        [TestMethod]
        public void Boolean()
        {
            string sql = String.Format("select cast(1 as bit) as boolProp");
            var item = new DataReaderAdapter<TypedObject>(TestConfig.RunSql(sql)).First();

            Assert.AreEqual(true,item.BoolProp);

            sql = String.Format("select cast(0 as bit) as boolProp");
            item = new DataReaderAdapter<TypedObject>(TestConfig.RunSql(sql)).First();
            Assert.AreEqual(false, item.BoolProp);

            sql = String.Format("select 'false' as boolProp");
            var parse = new MapOptions { ParseValues = true };
            item = new DataReaderAdapter<TypedObject>(TestConfig.RunSql(sql),parse ).First();

            Assert.AreEqual(false, item.BoolProp);

            sql = String.Format("select 'yes' as boolProp");
            item = new DataReaderAdapter<TypedObject>(TestConfig.RunSql(sql),parse).First();
            Assert.AreEqual(true, item.BoolProp);
        }

    }
}
