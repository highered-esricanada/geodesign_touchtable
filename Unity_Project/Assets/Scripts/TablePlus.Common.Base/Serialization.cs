using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;
using System.Reflection;

namespace TablePlus.Common.Base
{
    /// <summary>
    /// Serialization to Json (textual) representation
    /// </summary>
    public class JsonObjectSerializer
    {
        public static string Serialize(object obj)
        {
            try
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                StringWriter sw = new StringWriter();
                JsonTextWriter jtw = new JsonTextWriter(sw);
                jtw.Formatting = Newtonsoft.Json.Formatting.Indented;

                serializer.Serialize(jtw, obj);

                sw.Flush();
                return sw.ToString();
            }
            catch
            {
                return "";
            }
        }

        public static T Deserialize<T>(string json)
        {
            try
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                JsonTextReader jtr = new JsonTextReader(new StringReader(json));
                return serializer.Deserialize<T>(jtr);
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Serialization to XML (textual) representation
    /// </summary>
    public class XmlObjectSerializer
    {
        public static string Serialize<T>(T t)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (MemoryStream ms = new MemoryStream())
                {
                    TextWriter tw = new StreamWriter(ms);
                    serializer.Serialize(tw, t);
                    string s = Encoding.Default.GetString(ms.ToArray());
                    ms.Close();
                    return s;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static T Deserialize<T>(string xml)
        {
            try
            {
                XmlSerializer ser;
                ser = new XmlSerializer(typeof(T));
                StringReader stringReader;
                stringReader = new StringReader(xml);
                XmlTextReader xmlReader;
                xmlReader = new XmlTextReader(stringReader);
                T obj;
                obj = (T)ser.Deserialize(xmlReader);
                xmlReader.Close();
                stringReader.Close();
                return obj;
            }
            catch
            {
                return default(T);
            }
        }
    }

    /// <summary>
    /// Alternative serialization to XML (textual) representation
    /// Must use DataContract convention
    /// </summary>
    public class XmlDataObjectSerialization
    {
        public static string Serialize<T>(T t)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            string xmlString;
            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter writer = new XmlTextWriter(sw))
                {
                    writer.Formatting = System.Xml.Formatting.Indented; // indent the Xml so it's human readable
                    serializer.WriteObject(writer, t);
                    writer.Flush();
                    xmlString = sw.ToString();
                }
            }

            return xmlString;
        }

        public static T Deserialize<T>(string xml)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (StringReader sr = new StringReader(xml))
            {
                using (XmlTextReader reader = new XmlTextReader(sr))
                {
                    return (T)serializer.ReadObject(reader);
                }
            }
        }
    }

    /// <summary>
    /// Serialization to binary then compress using gzip then convert to BASE64.
    /// Result representation is textual.
    /// </summary>
    public class BinaryObjectSerialization
    {
        public static string Serialize(object x)
        {
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(ms, x);
                bytes = ms.ToArray();
            }
            using (MemoryStream msz = new MemoryStream())
            {
                GZipStream gz = new GZipStream(msz, CompressionMode.Compress, false);
                gz.Write(bytes, 0, bytes.Length);
                gz.Close();
                msz.Close();
                string s = Convert.ToBase64String(msz.ToArray());
                return s;
            }
        }

        public static object Deserialize(string data)
        {
            byte[] y = System.Convert.FromBase64String(data);

            using (MemoryStream ms2 = new MemoryStream(y))
            {
                byte[] buffer = new byte[1024];
                MemoryStream msd = new MemoryStream();
                GZipStream gz = new GZipStream(ms2, CompressionMode.Decompress, false);
                int n = 0;
                do
                {
                    n = gz.Read(buffer, 0, 1024);
                    if (n > 0)
                        msd.Write(buffer, 0, n);
                } while (n > 0);
                BinaryFormatter bformatter = new BinaryFormatter();
                msd.Seek(0, SeekOrigin.Begin);
                object obj = bformatter.Deserialize(msd);
                gz.Close();
                msd.Close();
                return obj;
            }
        }
    }

    /// <summary>
    /// This class serializes objects into CSV tables.
    /// scope-x.scope-y.index.scope-z.variable,val1,val2,...
    /// The object will first be serialized into JSON
    /// {
    ///  scope-x : {
    ///   scope-y : [
    ///    {
    ///     scope-z : {
    ///      variable : val1;
    ///     }
    ///    }
    ///   ]
    ///  }
    /// }
    /// Then JSON are translated into CSV.
    /// </summary>
    public class CSVObjectSerialization
    {
        /// <summary>
        /// Serialize a list of objects into a CSV table.
        /// The objects must align with each other, meaning that each of the objects
        /// must contain similar properties.
        /// </summary>
        /// <param name="xx"></param>
        /// <returns></returns>
        public static string Serialize(params object[] xx)
        {
            try
            {
                string[] headers = null;
                List<string[]> lines = new List<string[]>();
                foreach (object x in xx)
                {
                    string r = InternalSerialize(x);
                    string[] list = r.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (x == xx.First())
                        headers = list[0].Split(',');
                    lines.Add(list[1].Split(','));
                }

                string output = "";

                for (int i = 0; i < headers.Length; i++)
                {
                    string line = headers[i];
                    foreach (string[] obj in lines)
                    {
                        line += "," + obj[i];
                    }
                    output += line + "\n";
                }

                return output;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        protected static string InternalSerialize(object x)
        {

            Dictionary<string, string> dict = new Dictionary<string, string>();

            string s = JsonObjectSerializer.Serialize(x);
            // ensure proper separation
            s = s.Replace("}", " } ");
            s = s.Replace("{", " { ");
            s = s.Replace("]", " ] ");
            s = s.Replace("[", " [ ");
            s = s.Replace(",", " , ");

            bool inq = false;
            bool backslash = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\\')
                    backslash = true;
                if (backslash)
                    continue;
                if (s[i] == '"')
                    inq = !inq;
                if (s[i] == ' ' && inq)
                {
                    if (i < s.Length - 1)
                        s = s.Substring(0, i) + '+' + s.Substring(i + 1);
                    else
                        s = s.Substring(0, i) + '+';
                }
                if (s[i] == ':' && !inq)
                {
                    if (i < s.Length - 1)
                        s = s.Substring(0, i) + ' ' + s.Substring(i + 1);
                    else
                        s = s.Substring(0, i) + ' ';
                }
            }

            string[] segments = s.Split(new char[] {' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            List<string> prefixes = new List<string>();
            string currentName = "";
            int counter = 0;
            bool inarray = false;
            bool arrayHasName = false;
            string lastSegment = "";

            foreach (string str in segments)
            {
                string xkey = "";
                string xval = "";

                try
                {
                    // begin a sub-object
                    if (str == "{")
                    {
                        if (currentName != "")
                        {
                            prefixes.Add(currentName);
                            currentName = "";
                        }
                        else if (inarray == true)
                        {
                            prefixes.Add(counter.ToString());
                            counter++;
                        }
                        continue;
                    }

                    // end a sub-object
                    if (str == "}")
                    {
                        if (lastSegment == "{")
                        { 
                            xkey = string.Join(".", prefixes.ToArray());
                            xval = "{}";
                        }
                        if (prefixes.Count > 0)
                            prefixes.RemoveAt(prefixes.Count - 1);
                        continue;
                    }

                    // begin an array
                    if (str == "[")
                    {
                        arrayHasName = false;
                        if (currentName != "")
                        {
                            prefixes.Add(currentName);
                            currentName = "";
                            arrayHasName = true;
                        }
                        counter = 0;
                        inarray = true;
                        continue;
                    }

                    // end an array
                    if (str == "]")
                    {
                        if (lastSegment == "[")
                        {
                            xkey = string.Join(".", prefixes.ToArray());
                            xval = "[]";
                        }
                        if (currentName != "")
                        {
                            xkey = string.Join(".", prefixes.ToArray()) + "." + counter.ToString();
                            xval = currentName;
                        }
                        if (arrayHasName)
                            if (prefixes.Count > 0)
                                prefixes.RemoveAt(prefixes.Count - 1);
                        counter = 0;
                        inarray = false;
                        continue;
                    }

                    if (str == ":")
                    {
                        continue;
                    }

                    // key/value
                    if (currentName != "")
                    {
                        if (str == ",")
                        {
                            if (inarray)
                            {
                                xkey = string.Join(".", prefixes.ToArray()) + "." + counter.ToString();
                                xval = currentName;
                                counter++;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            xkey = string.Join(".", prefixes.ToArray()) + "." + currentName;
                            if (prefixes.Count == 0)
                                xkey = xkey.Substring(1);
                            xval = str;
                        }
                        currentName = "";
                        continue;
                    }

                    if (str == ",")
                        continue;

                    if (currentName == "")
                    {
                        currentName = str;
                    }
                }
                finally
                {
                    lastSegment = str;

                    if (xkey != "")
                    {
                        xkey = xkey.Replace("\"", "");
                        xkey = xkey.Replace("+", " ");
                        xkey = "\"" + xkey + "\"";
                        xval = xval.Replace("+", " ");

                        Console.WriteLine("Adding: " + xkey);
                        dict.Add(xkey, xval);
                    }
                }
            }

            string header = string.Join(",", dict.Keys.ToArray());
            string value = string.Join(",", dict.Values.ToArray());

            return header + "\n" + value; ;

        }

        public static T[] Deserialize<T>(string csv)
        {
            try
            {
                csv = csv.Replace("\r", "");
                string[] lines = csv.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                List<string[]> parts = new List<string[]>();
                foreach (string l in lines)
                {
                    // ignore blank lines or comment lines
                    if (l.StartsWith("-") || l == ",")
                        continue;
                    parts.Add(l.Split(','));
                }
                if (parts.Count < 2)
                    return default(T[]);
                int objects = parts[0].Length - 1;
                List<T> results = new List<T>();

                for (int i = 0; i < objects; i++)
                {
                    Dictionary<string, string> keyval = new Dictionary<string, string>();
                    for (int k = 0; k < parts.Count; k++)
                    {
                        keyval.Add(parts[k][0], parts[k][i + 1]);
                    }
                    results.Add(InternalDeserialize<T>(keyval));
                }
                return results.ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return default(T[]);
            }
        }

        protected static T InternalDeserialize<T>(Dictionary<string, string> keyval)
        {
            Dictionary<string, object> root = new Dictionary<string, object>();

            foreach (KeyValuePair<string, string> k in keyval)
            {
                string itemKey = k.Key.Replace("\"", "");
                string[] parts =itemKey.Split('.');
                object current = root;
                int counter = 0;
                foreach (string p in parts)
                {
                    counter++;
                    // last one
                    if (counter == parts.Length)
                    {
                        // an empty list;
                        if (k.Value == "[]")
                        {
                            if (current is Dictionary<string, object>)
                                (current as Dictionary<string, object>).Add(p, new List<int>());
                            else if (current is List<object>)
                                (current as List<object>).Add(new List<int>());
                        }
                        // an empty object;
                        else if (k.Value == "{}")
                        {
                            if (current is Dictionary<string, object>)
                                (current as Dictionary<string, object>).Add(p, new Dictionary<string, object>());
                            else if (current is List<object>)
                                (current as List<object>).Add(new Dictionary<string, object>());
                        }
                        // a value
                        else
                        {
                            object value = null;
                            string lowerval = k.Value.ToLower();
                            if (lowerval == "true")
                                value = true;
                            else if (lowerval == "false")
                                value = false;
                            else if (lowerval == "null")
                                value = null;
                            else if (Misc.IsInt(lowerval))
                                value = Int32.Parse(lowerval);
                            else if (Misc.IsDouble(lowerval))
                                value = Double.Parse(lowerval);
                            else
                            {
                                string strval = k.Value;
                                if (strval.StartsWith("\""))
                                    strval = strval.Substring(1);
                                if (strval.EndsWith("\""))
                                    strval = strval.Substring(0, strval.Length - 1);
                                value = strval;
                            }
                            if (current is Dictionary<string, object>)
                                (current as Dictionary<string, object>).Add(p, value);
                            else if (current is List<object>)
                                (current as List<object>).Add(value);
                        }
                    }
                    // in middle, .. key.key ..
                    // current is dict, child is dict
                    else if (Misc.TryParseInt(p, -1) == -1 && Misc.TryParseInt(parts[counter], -1) == -1)
                    {
                        // current must be a dict in this situation...
                        // x.y.0.z.w,5
                        // {
                        //  x: {
                        //   y: [
                        //    {
                        //     z : {
                        //      w : 5;
                        Dictionary<string, object> dcurrent = current as Dictionary<string, object>;
                        if (dcurrent.ContainsKey(p))
                            current = dcurrent[p];
                        else
                        {
                            dcurrent.Add(p, new Dictionary<string, object>());
                            current = dcurrent[p];
                        }
                    }
                    // in middle, .. key.number ...
                    // current is dict, child is list
                    else if (Misc.TryParseInt(p, -1) == -1 && Misc.TryParseInt(parts[counter], -1) >= 0)
                    {
                        Dictionary<string, object> dcurrent = current as Dictionary<string, object>;
                        if (dcurrent.ContainsKey(p))
                            current = dcurrent[p];
                        else
                        { 
                            dcurrent.Add(p, new List<object>());
                            current = dcurrent[p];
                        }
                    }
                    // in middle, .. number.key ..
                    // current is list, child is dict
                    else if (Misc.TryParseInt(p, -1) >= 0 && Misc.TryParseInt(parts[counter], -1) == -1)
                    {
                        int index = Misc.TryParseInt(p, -1);
                        List<object> lcurrent = current as List<object>;
                        if (lcurrent.Count > index)
                            current = lcurrent[index];
                        else
                        {
                            for (int i = lcurrent.Count; i < index + 1; i++)
                            {
                                lcurrent.Add(new Dictionary<string, object>());
                            }
                            current = lcurrent[index];
                        }
                    }
                    // in middle, .. number.number ..
                    // current is list, child is list
                    else if (Misc.TryParseInt(p, -1) >= 0 && Misc.TryParseInt(parts[counter], -1) >= 0)
                    {
                        int index = Misc.TryParseInt(p, -1);
                        List<object> lcurrent = current as List<object>;
                        if (lcurrent.Count > index)
                            current = lcurrent[index];
                        else
                        {
                            for (int i = lcurrent.Count; i < index + 1; i++)
                            {
                                lcurrent.Add(new List<object>());
                            }
                            current = lcurrent[index];
                        }
                    }
                }
            }

            string json = JsonObjectSerializer.Serialize(root);

            return JsonObjectSerializer.Deserialize<T>(json);
        }

    }

}
