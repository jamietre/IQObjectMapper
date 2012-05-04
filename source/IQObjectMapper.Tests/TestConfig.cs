using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQObjectMapper;


namespace IQObjectMapper.Tests
{
    [TestClass]
    public class TestConfig
    {
        public static IDbConnection GetConnection()
        {
            var conn = new SqlConnection(_ConnectionString);
            conn.Open();
            return conn;
        }
        public static IDataReader RunSql(string sql)
        {
            var conn = GetConnection();
            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                return cmd.ExecuteReader(CommandBehavior.Default);
            }
        }
        private static string _ConnectionString;

        [AssemblyInitialize]
        public static void TestRunSetup(TestContext context)
        {
            ObjectMapper.DefaultOptions.DynamicObjectType = typeof(JsObject);

            switch (System.Environment.MachineName)
            {
                case "DRLAP11001":
                    _ConnectionString = "Data Source=VMSQLMD01;Initial Catalog=tempdb;Integrated Security=True";
                    break;
                case "LENOVO3":
                    _ConnectionString = "Data Source=LENOVO3\\SQLEXPRESS;Initial Catalog=tempdb;Integrated Security=True";
                    break;
                default:
                    _ConnectionString = "Data Source=localhost;Initial Catalog=tempdb;Integrated Security=True";
                    break;
            }
        }
        public static string HexStr(string str)
        {
            return HexStr(System.Text.Encoding.ASCII.GetBytes(str));
        }
        public static string HexStr(byte[] p)
        {

            char[] c = new char[p.Length * 2 + 2];

            byte b;

            c[0] = '0'; c[1] = 'x';

            for (int y = 0, x = 2; y < p.Length; ++y, ++x)
            {

                b = ((byte)(p[y] >> 4));

                c[x] = (char)(b > 9 ? b + 0x37 : b + 0x30);

                b = ((byte)(p[y] & 0xF));

                c[++x] = (char)(b > 9 ? b + 0x37 : b + 0x30);

            }

            return new string(c);

        }
        public static TypedObject GetTypedObject()
        {
            return new TypedObject
            {
                StringProp = "quick brown fox",
                IntArray = new int[] { 1, 2, 4, 8 },
                DoubleProp = 3.14,
                StringList = new List<string> { "a", "b", "c" },
                StringField = "field"

            };

        }
        public static object GetAnonObject()
        {
            return new
            {
                StringProp = "quick brown fox",
                IntArray = new int[] { 1, 2, 4, 8 },
                DoubleProp = 3.14,
                StringList = new List<string> { "a", "b", "c" }
            };

        }

    }
}
