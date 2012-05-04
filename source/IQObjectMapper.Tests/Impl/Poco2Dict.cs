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
    public class Poco2Dict_
    {
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
        [TestMethod]
        public void Simple()
        {
            TypedObject test = GetTestObject();

            var mapper = new Poco2Dict();
            mapper.Options.CaseSensitive  =false;

            var target = ObjectMapper.ToDictionary(test);
            
            Assert.AreEqual(true, target["boolprop"]);
            Assert.AreEqual(1.2, target["doubleprop"]);
            Assert.AreSame(test.ObjectEnumerable,target["objectenumerable"]);


            target = ObjectMapper.ToDictionary(test, true);
            Assert.AreNotSame(test.ObjectEnumerable, ((IEnumerable<object>)target["objectenumerable"]).ElementAt(1));
            Assert.AreEqual(1, ((IEnumerable<object>)target["objectenumerable"]).ElementAt(1));
            
            //Assert.AreEqual((double)1.0, target.DoubleProp);
        }
    
        [TestMethod]
        public void Objects()
        {
            //ObjectMapper.MapOptions.DynamicObjectType = typeof(JsObject);

            //var dict = Dict();
            //dict["objectprop"] = new
            //{
            //    prop1 = "value1",
            //    prop2 = 2.5
            //};

            //dict["objectenumerable"] = new List<string>(new string[] { "a", "b" });

            //var target = ObjectMapper.Map<TypedObject>(dict);

            //Assert.AreEqual(typeof(JsObject), target.ObjectProp.GetType());
            //Assert.AreEqual("value1", target.ObjectProp.prop1);
            //Assert.AreEqual(2.5, target.ObjectProp.prop2);
            //Assert.AreEqual(target.ObjectEnumerable.GetType(), typeof(List<object>));
            //Assert.AreEqual("b", (target.ObjectEnumerable.ElementAt(1)));

            //// map a subobject
            //var innerDict = Dict();
            //innerDict["subprop1"] = "subvalue1";
            //innerDict["subprop2"] = 2;

            //var innerInnerDict = Dict();
            //innerDict["subprop3"] = innerInnerDict;
            //innerInnerDict["falseval"] = false;

            //dict["objectprop"] = innerDict;


            //target = ObjectMapper.Map<TypedObject>(dict);
            //Assert.AreEqual(typeof(JsObject), target.ObjectProp.GetType());
            //Assert.AreEqual(2, target.ObjectProp.subprop2);
            //Assert.AreEqual(false, target.ObjectProp.subprop3.falseval);

        }


    }
}
