
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Dynamic;
using System.Collections;
using System.Collections.Generic;
using IQObjectMapper;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;


namespace IQObjectMapper.Tests
{
    /// <summary>
    ///This is a test class for TypesTest and is intended
    ///to contain all TypesTest Unit Tests
    ///</summary>
    [TestClass]
    public class Types_
    {
        enum TestEnum
        {
            Item1 = 1,
            Item2 = 2
        }
        int intVar = 1;
        bool boolVar = true;
        DateTime? dateTimeNullable = DateTime.Now;
        DateTime dateTime = DateTime.Now;
        string stringVar = "test";
        char charVal = 'a';
        decimal decimalVal = 1.0m;
        decimal? decimalNullable = 1.1m;
        TypedObject classVar = new TypedObject();
        float floatVar = 1.24f;
        byte byteVar = 12;
        byte[] byteArr = new byte[] { 0x10, 0xff, 0x50 };
        List<string> stringList = new List<string>(new string[] { "item1", "item2", "item3" });
        ExpandoObject expando = new ExpandoObject();
        JsObject jsobject = new JsObject();

        /// <summary>
        ///A test for ChangeType
        ///</summary>
        [TestMethod]
        public void ChangeType()
        {
            //object value = null; // TODO: Initialize to an appropriate value
            //Type conversionType = null; // TODO: Initialize to an appropriate value
            //object expected = null; // TODO: Initialize to an appropriate value
            //object actual;
            //actual = Types_Accessor.ChangeType(value, conversionType);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void Parse()
        {
            bool boolResult;
            long longResult;
            DateTime dateResult;

            Assert.IsTrue(Types.TryParse<bool>("true", out boolResult), "Converted a bool");
            Assert.IsTrue(boolResult, "Result was true");
            Assert.IsTrue(Types.TryParse<bool>("Yes", out boolResult), "Converted a bool (yes)");
            Assert.IsTrue(boolResult, "Result was true");
            Assert.IsTrue(Types.TryParse<bool>("0", out boolResult), "Converted a bool (0)");
            Assert.IsFalse(boolResult, "Result was false");

            Assert.IsTrue(Types.TryParse<long>("12345", out longResult), "Converted a long");
            Assert.AreEqual(12345, longResult, "Result was correct");
            Assert.IsFalse(Types.TryParse<long>("blah", out longResult), "Could not convert a string to long");
            Assert.IsTrue(Types.TryParse<long>("12345.5", out longResult), "Could convert float to long");
            // when converting a floating-point string to an integral value, it should be rounded
            Assert.AreEqual(12346, longResult, "Result was correct");
            Assert.IsTrue(Types.TryParse<long>("12345.2", out longResult), "Could convert float to long");
            Assert.AreEqual(12345, longResult, "Result was correct");

            Assert.IsTrue(Types.TryParse<DateTime>("5/22/1969", out dateResult), "Could convert a date");
            Assert.AreEqual(new DateTime(1969, 5, 22), dateResult, "Date esult was correct");

            long? nullableLongResult;
            Assert.IsTrue(Types.TryParse<long?>(null, out nullableLongResult), "Could convert a nullable long");
            Assert.AreEqual(null, nullableLongResult, "Nullable long result was correct");

            Assert.IsTrue(Types.TryParse<long?>("", out nullableLongResult), "Could convert a nullable long");
            Assert.AreEqual(null, nullableLongResult, "Nullable long result was correct");

            object objectResult = Types.Parse("2", typeof(long?));
            Assert.AreEqual(2L, objectResult, "Nullable long result was correct");
            Assert.AreEqual(typeof(long), objectResult.GetType(), "Nullable long result was correct");

            Assert.IsFalse(Types.TryParse<long>(null, out longResult), "Could not convert null to long");
            Assert.IsFalse(Types.TryParse<long>("", out longResult), "Could not convert empty string to long");
            Assert.IsFalse(Types.TryParse<long>("A non-numeric string", out longResult), "Could not convert empty string to long");

            Assert.IsFalse(Types.TryParse(null, out longResult), "Could not convert null to long");
            Assert.IsFalse(Types.TryParse<long>("", out longResult), "Could not convert empty string to long");
            Assert.IsFalse(Types.TryParse<long>("A non-numeric string", out longResult), "Could not convert empty string to long");

            Assert.AreEqual(String.Empty, Types.Parse("", typeof(string)), "String -> string works (bug)");

            Assert.Throws<Exception>(() => { Types.Parse(null, typeof(long)); }, "Could not convert string to long");
            Assert.Throws<Exception>(() => { Types.Parse("", typeof(long)); }, "Could not convert string to long");
            Assert.Throws<Exception>(() => { Types.Parse("A non-numeric string", typeof(long)); }, "Could not convert string to long");

            // Test using obkects

            Assert.IsTrue(Types.TryParse<bool>(true, out boolResult), "Could convert bool");
            Assert.IsTrue(boolResult, "Could convert bool (2)");
            Assert.IsTrue(Types.Parse<bool>(true), "Could convert bool (3)");
            Assert.IsTrue(Types.Parse<bool>(1), "Could convert bool (4)");
            Assert.IsFalse(Types.Parse<bool>(0), "Could convert bool (5)");
            Assert.Throws<Exception>(() => { Types.Parse<bool>(""); }, "Could not convert bool (6)");
            Assert.AreEqual(1, Types.Parse<long>(1.1), "Could convert long");
            Assert.AreEqual(1, Types.Parse<long>(1L), "Could convert long");


            // Test enums

            Assert.AreEqual(TestEnum.Item1, Types.Parse<TestEnum>(1), "Got an enum from an int");
            Assert.AreEqual(TestEnum.Item1, Types.Parse<TestEnum>("1"), "Got an enum from an int");

            // couple tests of the wrapper method

            Assert.IsTrue(Types.Parse<bool>("yes", false), "Converted 'yes'");
            Assert.IsFalse(Types.Parse<bool>("xx", false), "Default was correct for a bad bool");
            Assert.IsTrue(Types.Parse<bool>("xx", true), "Default was correct for a bad bool");


            // DefaultValue
        }


        /// <summary>
        ///A test for DefaultValue
        ///</summary>
        [TestMethod()]
        public void DefaultValue()
        {
            Assert.AreEqual(0, Types.DefaultValue<int>());
            Assert.AreEqual(0m, Types.DefaultValue<decimal>());
            Assert.AreEqual(null, Types.DefaultValue<string>());
            Assert.AreEqual(default(DateTime), Types.DefaultValue<DateTime>());
            Assert.AreEqual(null, Types.DefaultValue<DateTime?>());
            Assert.AreEqual(null, Types.DefaultValue<TypedObject>());
            Assert.AreEqual(null, Types.DefaultValue<JsObject>());
        }
        [TestMethod()]
        public void IsListTargetType()
        {
            Assert.IsFalse(Types.IsListTargetType<string>());
            Assert.IsFalse(Types.IsListTargetType<IEnumerable>());
            Assert.IsTrue(Types.IsListTargetType<IEnumerable<JsObject>>());
            Assert.IsFalse(Types.IsListTargetType<IList>());
            Assert.IsTrue(Types.IsListTargetType<IList<int>>());
            Assert.IsTrue(Types.IsListTargetType<ICollection<int>>());
            Assert.IsFalse(Types.IsListTargetType<object>());


        }
         [TestMethod()]
        public void TryGetInstantiableType()
        {
            Type t;
            Assert.IsFalse( Types.TryGetInstantiableType<IList>(out t));

            Assert.IsTrue(Types.TryGetInstantiableType<IEnumerable<double>>(out t));
            Assert.AreEqual(typeof(List<double>), t);

            Assert.IsTrue(Types.TryGetInstantiableType<IList<double>>(out t));
            Assert.AreEqual(typeof(List<double>),t);

            Assert.IsTrue(Types.TryGetInstantiableType<ICollection<JsObject>>(out t));
            Assert.AreEqual(typeof(List<JsObject>), t);
        }
        /// <summary>
        ///A test for GetFullyQualifiedPath
        ///</summary>
        [TestMethod()]
        public void GetFullyQualifiedPath()
        {
            //MethodInfo methodInfo = null; // TODO: Initialize to an appropriate value
            //string expected = string.Empty; // TODO: Initialize to an appropriate value
            //string actual;
            //actual = Types.GetFullyQualifiedPath(methodInfo);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void IsNumericType()
        {


            Assert.IsTrue(Types.IsNumericType(intVar));
            Assert.IsFalse(Types.IsNumericType(boolVar));
            Assert.IsFalse(Types.IsNumericType(dateTimeNullable));
            Assert.IsFalse(Types.IsNumericType(dateTime));
            Assert.IsFalse(Types.IsNumericType(stringVar));
            Assert.IsFalse(Types.IsNumericType(charVal));
            Assert.IsFalse(Types.IsNumericType(charVal));
            Assert.IsTrue(Types.IsNumericType(decimalVal));
            Assert.IsTrue(Types.IsNumericType(decimalNullable));
            Assert.IsFalse(Types.IsNumericType(classVar));
            Assert.IsTrue(Types.IsNumericType(floatVar));
            Assert.IsTrue(Types.IsNumericType(byteVar));
            Assert.IsFalse(Types.IsNumericType(byteArr));
        }

        [TestMethod]
        public void IsValueTargetObject()
        {
            Assert.IsTrue(Types.IsValueTargetObject(intVar));
            Assert.IsTrue(Types.IsValueTargetObject(boolVar));
            Assert.IsTrue(Types.IsValueTargetObject(dateTimeNullable));
            Assert.IsTrue(Types.IsValueTargetObject(dateTime));
            Assert.IsTrue(Types.IsValueTargetObject(stringVar));
            Assert.IsTrue(Types.IsValueTargetObject(charVal));
            Assert.IsTrue(Types.IsValueTargetObject(charVal));
            Assert.IsTrue(Types.IsValueTargetObject(decimalVal));
            Assert.IsTrue(Types.IsValueTargetObject(decimalNullable));
            Assert.IsTrue(Types.IsValueTargetObject(floatVar));
            Assert.IsTrue(Types.IsValueTargetObject(byteVar));
            Assert.IsTrue(Types.IsValueTargetObject(byteArr));
            Assert.IsFalse(Types.IsValueTargetObject(new object()));
            
            Assert.IsFalse(Types.IsValueTargetObject(expando));
            Assert.IsFalse(Types.IsValueTargetObject(null));
            Assert.IsFalse(Types.IsValueTargetObject(new TypedObject()));
        }
        [TestMethod]
        public void IsValueTarget()
        {
            Assert.IsTrue(Types.IsValueTarget<int>());
            Assert.IsTrue(Types.IsValueTarget<int?>());
            Assert.IsTrue(Types.IsValueTarget<bool>());


            Assert.IsFalse(Types.IsValueTarget(typeof(void)));
            Assert.IsFalse(Types.IsValueTarget<ExpandoObject>());
            Assert.IsFalse(Types.IsValueTarget<TypedObject>());

        }

        [TestMethod]
        public void GetInstanceOf()
        {
            Assert.AreEqual(0, Types.GetInstanceOf<int>());
            Assert.AreEqual(0m, Types.GetInstanceOf<decimal>());
            Assert.AreEqual(null, Types.GetInstanceOf<string>());
            Assert.AreEqual(default(DateTime), Types.GetInstanceOf<DateTime>());
            Assert.AreEqual(null, Types.GetInstanceOf<DateTime?>());
            Assert.IsTrue(Types.GetInstanceOf<TypedObject>().GetType() == typeof(TypedObject));
            Assert.IsTrue(Types.GetInstanceOf<JsObject>().GetType() == typeof(JsObject));

        }


        /// <summary>
        ///A test for GetNullableType
        ///</summary>
        [TestMethod()]
        public void GetNullableType()
        {
            Assert.AreEqual(typeof(int?), Types.GetNullableType(typeof(int)));
            Assert.AreEqual(typeof(string), Types.GetNullableType(typeof(string)));
            Assert.AreEqual(typeof(int?), Types.GetNullableType(typeof(int)));
            Assert.AreEqual(typeof(TypedObject), Types.GetNullableType(typeof(TypedObject)));

        }

        /// <summary>
        ///A test for GetUnderlyingType
        ///</summary>
        [TestMethod()]
        public void GetUnderlyingType()
        {
            Assert.AreEqual(typeof(int), Types.GetUnderlyingType(typeof(int?)));
            Assert.AreEqual(typeof(int), Types.GetUnderlyingType(typeof(int)));
            Assert.AreEqual(typeof(string), Types.GetNullableType(typeof(string)));
            Assert.AreEqual(typeof(TypedObject), Types.GetNullableType(typeof(TypedObject)));
        }

        /// <summary>
        ///A test for IsInsantiableType
        ///</summary>
        [TestMethod()]
        public void IsInsantiableType()
        {
            Assert.IsTrue(Types.IsInstantiableType<int>());
            Assert.IsTrue(Types.IsInstantiableType<bool>());
            Assert.IsTrue(Types.IsInstantiableType<bool>());
            Assert.IsTrue(Types.IsInstantiableType<TypedObject>());
            Assert.IsTrue(Types.IsInstantiableType<ExpandoObject>());
            Assert.IsFalse(Types.IsInstantiableType<TypedObject_Parameters>());
            Assert.IsFalse(Types.IsInstantiableType(typeof(void)));
            Assert.IsFalse(Types.IsInstantiableType(typeof(ITypedObject)));
            Assert.IsFalse(Types.IsInstantiableType(typeof(List<>)));
        }

     
        /// <summary>
        ///A test for IsNullableType
        ///</summary>
        [TestMethod()]
        public void IsNullableType()
        {
            Assert.IsTrue(Types.IsNullableType<int?>());
            Assert.IsTrue(Types.IsNullableType<string>());
            Assert.IsTrue(Types.IsNullableType<DateTime?>());
            
            Assert.IsFalse(Types.IsNullableType<ExpandoObject>());
            Assert.IsFalse(Types.IsNullableType<TypedObject>());
            Assert.IsFalse(Types.IsNullableType<List<int>>());
            Assert.IsFalse(Types.IsNullableType<int[]>());
            

            Assert.IsFalse(Types.IsNullableType<int>());
            Assert.IsFalse(Types.IsNullableType<DateTime>());
            
        }

        /// <summary>
        ///A test for IsRESTParameterType
        ///</summary>
        [TestMethod()]
        public void IsRESTParameterType()
        {
            //Type type = null; // TODO: Initialize to an appropriate value
            //bool expected = false; // TODO: Initialize to an appropriate value
            //bool actual;
            //actual = Types.IsRESTParameterType(type);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for TryConvert
        ///</summary>
        [TestMethod()]
        public void TryConvert()
        {
            //object value = null; // TODO: Initialize to an appropriate value
            //object typedValue = null; // TODO: Initialize to an appropriate value
            //object typedValueExpected = null; // TODO: Initialize to an appropriate value
            //Type type = null; // TODO: Initialize to an appropriate value
            //object defaultValue = null; // TODO: Initialize to an appropriate value
            //bool expected = false; // TODO: Initialize to an appropriate value
            //bool actual;
            //actual = Types.TryConvert(value, out typedValue, type, defaultValue);
            //Assert.AreEqual(typedValueExpected, typedValue);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        
        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
    }
}
