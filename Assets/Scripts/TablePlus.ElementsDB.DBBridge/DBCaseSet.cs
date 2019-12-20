using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TablePlus.Common.Base;

namespace TablePlus.ElementsDB.DBBridge
{
    [Serializable]
    public class DBCaseSet : DBObject
    {
        public List<DBCase> CaseSummaryList { get; protected set; }

        public string _CaseSetId { get; protected set; }
        public string _UserId { get; protected set; }
        public string _CaseSetName { get; protected set; }
        public string _CaseSetDescription { get; protected set; }
        public string _CasesTotal { get; protected set; }
        public string _Author { get; protected set; }
        public string _Thumbnail { get; protected set; }

        public DBCaseSet()
        {
            CaseSummaryList = new List<DBCase>();
            _CaseSetId = "0";
            _CaseSetName = "(all)";
            _UserId = "0";
            _CaseSetDescription = "(null)";
            _CasesTotal = "0";
            _Author = "(null)";
            _Thumbnail = "(null)";
        }

        public DBCaseSet(string xml)
            : base(xml)
        {
            CaseSummaryList = new List<DBCase>();
        }

        public void ParseCaseSet()
        {
            Properties.Clear();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XML);

            XmlNodeList caseSets = doc.GetElementsByTagName("case_set");
            CaseSetParseAssert(caseSets.Count > 0, "No case sets found");

            XmlNode thisCaseSet = caseSets[0];
            LoadObjectProperties(thisCaseSet);
        }

        public void LoadCasesSummary()
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            CaseSummaryList.Clear();

            string func = "listCase";

            if (_CaseSetId != "0")
            {
                args.Add("caseSetId", _CaseSetId);
                func = "getCaseSetById";
            }

            args.Add("currentPage", "1");
            

            int counter = 1;
            while (true)
            {
                string response = Elements.InvokeFunction(func, args);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);
                XmlNodeList list = doc.GetElementsByTagName("case");
                if (list.Count == 0)
                    break;
                foreach (XmlNode node in list)
                {
                    string xml = node.OuterXml;
                    DBCase c = new DBCase(xml);
                    c.ParseCase();
                    CaseSummaryList.Add(c);
                }
                counter++;
                args["currentPage"] = counter.ToString();
                if (_CaseSetId != "0")
                    break; 
                // DEBUG: The Jan 2011 version of elementsDB does not support pagination for case sets
                // However, the specification asserts so, this is a bug.
            }
        }

        protected void CaseSetParseAssert(bool condition, string error)
        {
            Misc.Assert(condition, "Case set parsing error: " + error);
        }

    }
}
