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

namespace IQObjectMapper.Tests
{
    [TestClass]
    public class Dict2Poco_
    {
        protected IDictionary<string, object> Dict()
        {
            return new Dictionary<string, object>();
        }
        [TestMethod]
        public void Simple()
        {
            var dict = Dict();
            dict["stringprop"] = "stringValue";
            dict["intarray"] = new List<int>(new int[] { 1, 2, 5, 10 });
            dict["stringlist"] = new string[] { "abc", "def", "ghi" };
            dict["doubleprop"] = (int)1;

            var target = new TypedObject();
            ObjectMapper.ToExisting(dict, target);

            Assert.AreEqual("stringValue", target.StringProp);
            Assert.AreEqual(2, target.IntArray[1]);
            Assert.AreEqual("ghi", target.StringList[2]);
            Assert.AreEqual((double)1.0, target.DoubleProp);
        }

        [TestMethod]
        public void Objects()
        {

            var dict = Dict();
            dict["dynamicobjectprop"] = new
            {
                prop1 = "value1",
                prop2 = 2.5
            };

            dict["objectenumerable"] = new List<string>(new string[] { "a", "b" });

            var target = ObjectMapper.ToNew<TypedObject>(dict);

            Assert.AreEqual(typeof(JsObject), target.DynamicObjectProp.GetType());
            Assert.AreEqual("value1", target.DynamicObjectProp.prop1);
            Assert.AreEqual(2.5, target.DynamicObjectProp.prop2);
            Assert.AreEqual(target.ObjectEnumerable.GetType(), typeof(List<object>));
            Assert.AreEqual("b", (target.ObjectEnumerable.ElementAt(1)));

            // map a subobject
            var innerDict = Dict();
            innerDict["subprop1"] = "subvalue1";
            innerDict["subprop2"] = 2;

            var innerInnerDict = Dict();
            innerDict["subprop3"] = innerInnerDict;
            innerInnerDict["falseval"] = false;

            dict["dynamicobjectprop"] = innerDict;


            target = ObjectMapper.ToNew<TypedObject>(dict);
            Assert.AreEqual(typeof(JsObject), target.DynamicObjectProp.GetType());
            Assert.AreEqual(2, target.DynamicObjectProp.subprop2);
            Assert.AreEqual(false, target.DynamicObjectProp.subprop3.falseval);

        }

        [TestMethod]
        public void Options_()
        {
            var opts = MapOptions.Default();

            var dict = Dict();
            dict["objectprop"] = true;
            var target = ObjectMapper.ToNew<TypedObject>(dict);

            //Assert.AreEqual(typeof(ExpandoObject), target.ObjectProp);
            Assert.AreEqual(true, target.ObjectProp);

            dict["stringfield"] = "stringvalue";
            target = ObjectMapper.ToNew<TypedObject>(dict);
            Assert.AreEqual( "stringvalue", target.StringField);

            opts.IncludeFields = true;
            ObjectMapper.MapperCache.ForgetAbout(typeof(TypedObject));
            target = ObjectMapper.ToNew<TypedObject>(dict);
            Assert.AreEqual("stringvalue", target.StringField);


        }

    }
}
