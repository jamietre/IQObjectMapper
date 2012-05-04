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
using IQObjectMapper.Impl;
using IQObjectMapper.Adapters;

namespace IQObjectMapper.Tests
{
    //[Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
    [TestClass]
    public class Performance_
    {
        /// <summary>
        /// quick and dirty performance check
        /// </summary>
        string results;
        MapOptions options;

        [TestMethod]
        public void Perform()
        {
            results="";
            options = MapOptions.Default();

            Perform("No optimizing",FullReflection, Pure);

            IClassInfo ci = ObjectMapper.MapperCache.GetClassInfo(typeof(TypedObject), options);

            var partialReflect = new Action<int, TypedObject>((i, obj) =>
            {
                var fi = ci["stringprop"];
                string newVal = "val" + i;
                fi.SetValue(obj, newVal);
                var value = (string)fi.GetValue(obj);
            });

            Perform("Local ClassInfo",partialReflect, Pure);

            IDelegateInfo mfi = ci["stringprop"];
            IDelegateInfo mfiDbl = ci["doubleprop"];

            var onlyReflect = new Action<int, TypedObject>((i, obj) =>
            {
                
                string newVal = "val" + i;
                mfi.SetValue(obj, newVal);
                mfiDbl.SetValue(obj, i * 1.5);
                string value = (string)mfi.GetValue(obj);
                double dbl = (double)mfiDbl.GetValue(obj);
            });

            Perform("Local Delegates", onlyReflect, Pure);

            // This makes no difference - the delegates are already cast as types when created

            //var onlyReflectT = new Action<int, TypedObject>((i, obj) =>
            //{

            //    string newVal = "val" + i;
            //    mfi.SetValueT(obj, newVal);
            //    mfiDbl.SetValueT(obj, i*1.5);
            //    string value = mfi.GetValueT<TypedObject,string>(obj);
            //    double dbl = mfiDbl.GetValueT<TypedObject, double>(obj);
            //});

            //Perform("Strongly typed local delegates vs. Pure", onlyReflect, Pure);

            mfi = ci["stringfield"];
            mfiDbl = ci["doublefield"];

            var onlyReflectFields = new Action<int, TypedObject>((i, obj) =>
            {

                string newVal = "val" + i;
                mfi.SetValue(obj, newVal);
                mfiDbl.SetValue(obj, i * 1.5);
                string value = (string)mfi.GetValue(obj);
                double dbl = (double)mfiDbl.GetValue(obj);
            });

            Perform("Local Delegates -- Fields vs Pure", onlyReflect, PureFields);

            var dict = new PropertyDictionaryAdapter(new TypedObject());

            var dictWrapper = new Action<int, TypedObject>((i, obj) =>
            {

                string newVal = "val" + i;
                dict["stringvalue"] = newVal;
                dict["doublevalue"] = i * 1.5;
                string value = (string)dict["stringvalue"];
                double dbl = (double)dict["doublevalue"];
            });

            
            Perform("Dictionary wrapper - properties vs. pure ", dictWrapper, PureFields);

            var realDict = new Dictionary<string, object>();
            
            var realDictWrapper = new Action<int, TypedObject>((i, obj) =>
            {

                string newVal = "val" + i;
                realDict["stringvalue"] = newVal;
                realDict["doublevalue"] = i * 1.5;
                string value = (string)realDict["stringvalue"];
                double dbl = (double)realDict["doublevalue"];
            });

            Perform("Dictionary wrapper - properties vs. real dictionary ", dictWrapper, realDictWrapper);

            dynamic expando = new ExpandoObject();

            var expandoTest = new Action<int, TypedObject>((i, obj) =>
            {

                string newVal = "val" + i;
                expando.stringvalue = newVal;
                expando.doublevalue = i * 1.5;
                string value = expando.stringvalue;
                double dbl = expando.doublevalue;
            });

            Perform("Expando vs. pure", expandoTest, Pure);

            dynamic dynWrapper = new DynamicAdapter(new TypedObject());

            var dynWapperTest = new Action<int, TypedObject>((i, obj) =>
            {

                string newVal = "val" + i;
                dynWrapper.stringvalue = newVal;
                dynWrapper.doublevalue = i * 1.5;
                string value = dynWrapper.stringvalue;
                double dbl = dynWrapper.doublevalue;
            });

            Perform("DynamicAdapter vs. Pure", dynWapperTest, Pure);
            Perform("DynamicAdapter vs. Expando", dynWapperTest, expandoTest);

            Assert.Fail(results);
        }


        protected void FullReflection(int i, TypedObject obj)
        {
            IClassInfo ci;
            ci = ObjectMapper.MapperCache.GetClassInfo(typeof(TypedObject), options);
            string newVal = "val" + i;
            
            ci["stringprop"].SetValue(obj, newVal);
            ci["doubleprop"].SetValue(obj, i * 1.5);

            string value = (string)ci["stringprop"].GetValue(obj);
            double dbl = (double)ci["doubleprop"].GetValue(obj);

        }
        protected void Pure(int i, TypedObject obj)
        {
            string newVal = "val" + i;
            obj.StringProp = newVal;
            obj.DoubleProp = i * 1.5;
            string value = obj.StringProp;
            double dbl = obj.DoubleProp;
        }

        protected void PureFields(int i, TypedObject obj)
        {
            string newVal = "val" + i;
            obj.StringField = newVal;
            obj.DoubleField = i * 1.5;
            string value = obj.StringField;
            double dbl = obj.DoubleField;
        }

        protected void Perform(string name, Action<int, TypedObject> reflection, Action<int, TypedObject> pure)
        {
            DateTime start = DateTime.Now;
            int iterations = 1000000;
            

            TypedObject obj = new TypedObject();

            for (int i = 0; i < iterations; i++)
            {
                reflection(i,obj);    
            }

            DateTime afterReflection = DateTime.Now;
            for (int i = 0; i < iterations; i++)
            {
                pure(i, obj);
            }

            DateTime Done = DateTime.Now;

            TimeSpan time1 = afterReflection - start;
            TimeSpan time2 = Done - afterReflection;

            results += name + " (Per 1000) Time 1: " + time1.TotalMilliseconds / iterations * 1000 + ", Time 2: " + time2.TotalMilliseconds / iterations * 1000  
                + ", Ratio: " + time1.TotalMilliseconds / time2.TotalMilliseconds + System.Environment.NewLine;
        }
    }
}
