using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQObjectMapper.Impl
{
    /// <summary>
    /// Used to represent an undefined return value
    /// </summary>
    public class UndefinedObject
    {
        private static bool Singleton;
        private static object Locked = new Object();
        public UndefinedObject()
        {
            lock(Locked) {
                if (Singleton)
                {
                    throw new Exception("There can be only one instance of the UndefinedObject.");
                }
                Singleton = true;
            }
        }
        public static UndefinedObject Value = new UndefinedObject();
    }
}
