using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQObjectMapper
{
    /// <summary>
    /// Used to represent an undefined return value (distinct from "null" which could be an intended value)
    /// This class is a self-refencing singleton. The static property exposes the only allowed instance 
    /// of itself, like to System.DBNull.Value. The locking object and flag ensure only one instance can be
    /// created.
    /// </summary>
    public class Undefined
    {
        private static bool Singleton;
        private static object Locked = new Object();
        public Undefined()
        {
            lock(Locked) {
                if (Singleton)
                {
                    throw new Exception("There can be only one instance of the UndefinedObject.");
                }
                Singleton = true;
            }
        }
        public static Undefined Value = new Undefined();
    }
}
