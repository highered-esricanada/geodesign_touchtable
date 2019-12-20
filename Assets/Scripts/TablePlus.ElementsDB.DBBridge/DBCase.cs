using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using TablePlus.Common.Base;

namespace TablePlus.ElementsDB.DBBridge
{
    [Serializable]
    public class DBCase : DBObject
    {
        public List<DBCaseResource> ResourceList { get; protected set; }
        public List<DBLanduse> LanduseList { get; protected set; }

        public string _CaseId { get; protected set; }
        public string _CaseNumber { get; protected set; }
        public string _CaseName { get; protected set; }
        public string _Caption { get; protected set; }

        public DBCase(string xml)
            : base(xml)
        {
            ResourceList = new List<DBCaseResource>();
            LanduseList = new List<DBLanduse>();
            _CaseId = "0";
            _CaseNumber = "00000-00";
            _CaseName = "(null)";
            _Caption = "(null)";
        }

        public void ParseCase()
        {
            Properties.Clear();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XML);

            XmlNodeList cases = doc.GetElementsByTagName("case");
            CaseParseAssert(cases.Count > 0, "No cases found");

            XmlNode thisCase = cases[0];
            LoadObjectProperties(thisCase);

            XmlNodeList resources = doc.GetElementsByTagName("resource");
            foreach (XmlNode n in resources)
            {
                DBCaseResource cr = new DBCaseResource();
                cr.Load(n);
                ResourceList.Add(cr);
            }

            XmlNodeList landuses = doc.GetElementsByTagName("landuse");
            foreach (XmlNode n in landuses)
            {
                DBLanduse l = new DBLanduse();
                l.Load(n);
                LanduseList.Add(l);
            }

        }

        protected void CaseParseAssert(bool condition, string error)
        {
            Misc.Assert(condition, "Case parsing error: " + error);
        }
    }

    [Serializable]
    public class DBCaseResource : DBObject
    {
        public string _MediaID { get; protected set; }
        public string _MediaType { get; protected set; }
        public string _MediaTitle { get; protected set; }
        public string _MediaDescription { get; protected set; }
        public string _PathWeb { get; protected set; }
        public string _SizeKb { get; protected set; }
        public string _Extension { get; protected set; }

        public DBCaseResource()
        {
            _MediaID = "0";
            _MediaType = "0";
            _MediaTitle = "0";
            _MediaDescription = "0";
            _PathWeb = "0";
            _SizeKb = "0";
            _Extension = "0";
        }

        public void Load(XmlNode parentNode)
        {
            base.LoadObjectProperties(parentNode);
        }
    }

    [Serializable]
    public class DBLanduse : DBObject
    {
        public string _Type { get; protected set; }
        public string _Forms { get; protected set; }

        public DBLanduse()
        {
            _Type = "(unknown)";
            _Forms = "(unknown)";
        }

        public void Load(XmlNode parentNode)
        {
            base.LoadObjectProperties(parentNode);
        }
    }
}
