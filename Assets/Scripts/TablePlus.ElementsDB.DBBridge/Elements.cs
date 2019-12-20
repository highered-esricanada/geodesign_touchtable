using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;

namespace TablePlus.ElementsDB.DBBridge
{
    public class Elements
    {
        protected const string ApiKey = "65d0b8de459a256c8f6e26dea83a081d";
        public const string ApiLocation = "https://elementsdb.sala.ubc.ca/api/";
        public const string WebServer = "https://elementsdb.sala.ubc.ca";
        

        public static string InvokeFunction(string func, Dictionary<string, string> args)
        {
            List<string> arglist = new List<string>();
            foreach (KeyValuePair<string, string> p in args)
            {
                arglist.Add(p.Key + "=" + p.Value);
            }
            arglist.Add("api_key=" + ApiKey);

            string arguments = string.Join("&", arglist.ToArray());

            string url = ApiLocation + func + ".php?" + arguments;

            WebResponse objResponse;
            WebRequest objRequest = System.Net.HttpWebRequest.Create(url);
            string strResult = null;

            objResponse = objRequest.GetResponse();

            using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
            {
                strResult = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
            }

            return strResult;
        }

        public static DBCase GetCaseByID(string id)
        { 
            Dictionary<string, string> args = new Dictionary<string,string>();
            args.Add("caseId", id);
            string response = InvokeFunction("getCaseById", args);
            DBCase c = new DBCase(response);
            c.ParseCase();

            return c;  
        }

        public static List<DBCase> ListAllCasesSummary()
        {
            List<DBCase> result = new List<DBCase>();

            DBCaseSet dbset = new DBCaseSet();
            dbset.LoadCasesSummary();

            foreach (DBCase c in dbset.CaseSummaryList)
                result.Add(c);

            return result;
        }

        public static List<DBCaseSet> ListAllCaseSetsSummary()
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("currentPage", "1");
            List<DBCaseSet> result = new List<DBCaseSet>();

            int counter = 1;
            while (true)
            {
                string response = InvokeFunction("listCaseSet", args);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);
                XmlNodeList list = doc.GetElementsByTagName("case_set");
                if (list.Count == 0)
                    break;
                foreach (XmlNode node in list)
                {
                    string xml = node.OuterXml;
                    DBCaseSet c = new DBCaseSet(xml);
                    c.ParseCaseSet();
                    result.Add(c);
                }
                counter++;
                args["currentPage"] = counter.ToString();
            }

            return result;
        }

    }
}
