
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Xml;
using System.ComponentModel;
namespace TablePlus.Common.Base
{
    public class ThreadSafeBox<T>
    {
        protected T internalValue;
        protected object LockObject = new object();

        public ThreadSafeBox()
        { }

        public ThreadSafeBox(T val)
        {
            internalValue = val;
        }

        public T Value
        {
            set
            {
                lock (LockObject)
                {
                    internalValue = value;
                }
            }

            get
            {
                lock (LockObject)
                {
                    return internalValue;
                }
            }
        }
    }

    public class PassiveStopWatch
    {
        private DateTime TimeStamp;
        public long DefaultTimeoutMS;

        public PassiveStopWatch()
            : this(1000)
        {

        }

        public PassiveStopWatch(long defaultms)
        {
            DefaultTimeoutMS = defaultms;
            TimeStamp = DateTime.Now;
        }

        public bool Timeout()
        {
            return Timeout(true);
        }

        public bool Timeout(long temporaryTimeoutMS)
        {
            return Timeout(temporaryTimeoutMS, true);
        }

        public bool Timeout(bool resetIfTimeout)
        {
            return Timeout(DefaultTimeoutMS, true);
        }

        public bool Timeout(long temporaryTimeoutMS, bool resetIfTimeout)
        {
            DateTime now = DateTime.Now;

            long ticks = DateTime.Now.Ticks - TimeStamp.Ticks;
            long ms = ticks / TimeSpan.TicksPerMillisecond;

            if (ms < temporaryTimeoutMS)
                return false;
            else
            {
                if (resetIfTimeout)
                    TimeStamp = DateTime.Now;
                return true;
            }
        }
    }

    public class Misc
    {
        protected static List<System.Threading.Timer> DeplayedInvokeTimers = new List<System.Threading.Timer>();

        public static void Assert(bool condition, string error)
        {
            if (!condition)
                throw new Exception(error);
        }

        public static string GetCapitalizedName(string underscore_name)
        {
            string[] parts = underscore_name.Split('_');
            List<string> newparts = new List<string>();
            foreach (string p in parts)
            {
                newparts.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p));
            }

            return string.Join("", newparts.ToArray());
        }

        public static string GetSimpleContent(XmlNode node)
        {
            if (node.Value != null)
                return node.Value;
            else if (node.ChildNodes.Count == 1)
                return node.ChildNodes[0].Value;
            else
                return null;
        }

        public static bool Expired(DateTime last, TimeSpan period)
        {
            DateTime expected = last.Add(period);
            if (expected.CompareTo(DateTime.Now) < 0)
            {
                return true;
            }

            return false;
        }
 
 
        public static int TryParseInt(string text, int def)
        {
            int r = -1;
            if (Int32.TryParse(text, out r))
                return r;
            else
                return def;
        }

        public static double TryParseDouble(string text, double def)
        {
            double r = -1;
            if (Double.TryParse(text, out r))
                return r;
            else
                return def;
        }

        public static bool IsInt(string text)
        {
            int r = -1;
            return Int32.TryParse(text, out r);
        }

        public static bool IsDouble(string text)
        {
            double r = -1;
            return Double.TryParse(text, out r);
        }
    }
}
