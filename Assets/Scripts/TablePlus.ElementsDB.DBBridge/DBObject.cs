using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using TablePlus.Common.Base;

namespace TablePlus.ElementsDB.DBBridge
{
    [Serializable]
    public class DBObject
    {
        protected string XML;
        public Dictionary<string, string> Properties { get; protected set; }

        public DBObject(string xml)
        {
            XML = xml;
            Properties = new Dictionary<string, string>();
        }

        public DBObject()
        {
            XML = null;
            Properties = new Dictionary<string, string>();
        }

        protected virtual void LoadObjectProperties(XmlNode parentNode)
        {
            XmlNodeList children = parentNode.ChildNodes;

            foreach (XmlNode node in children)
            {
                TrySetProperty(node);
            }

            XmlAttributeCollection ac = parentNode.Attributes;
            for (int i = 0; i < ac.Count; i++)
            {
                TrySetProperty(ac[i]);
            }
        }

        protected void LoadObjectProperties(string xml)
        { 
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            LoadObjectProperties(doc);
        }

        protected void TrySetProperty(XmlNode node)
        {
            string cap = "_" + Misc.GetCapitalizedName(node.LocalName);
            string value = Misc.GetSimpleContent(node);
            PropertyInfo prop = this.GetType().GetProperty(cap);
            if (prop != null)
            {
                if (prop.CanWrite)
                {
                    prop.SetValue(this, value, null);
                }
            }
            Properties[node.LocalName] = value;
        }
    }
}
