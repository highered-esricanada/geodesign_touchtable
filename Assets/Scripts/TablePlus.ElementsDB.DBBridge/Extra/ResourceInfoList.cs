using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using TablePlus.Common.Base;
using System.Xml.Serialization;
using System.Collections;

namespace TablePlus.ElementsDB.DBBridge.Extra
{
    [Serializable]
    [XmlRoot("ResourceAdditionalInfo")]
    public class ResourceAdditionalInfo
    {
        public string Key = "null";
        public string Value = "null";
    }

    [Serializable]
    [XmlRoot("ResourceInfo")]
    public class ResourceInfo
    {
        public string Key = "null";
        public DateTime SourceModifiedDate;
        public DateTime TargetModifiedDate;
        [XmlElement(Type = typeof(ResourceAdditionalInfo), ElementName = "ResourceAdditionalInfo")]
        public ArrayList AdditionalInfo = new ArrayList();

        public string TryGeyAdditionalInfo(string key)
        {
            foreach (object o in AdditionalInfo)
            { 
                if ((o as ResourceAdditionalInfo).Key == key)
                    return (o as ResourceAdditionalInfo).Value;
            }
            return null;
        }
    }

    [XmlRoot("ResourceInfoList")]
    public class ResourceInfoList
    {
        public string Key = "null";
        [XmlElement(Type = typeof(ResourceInfo), ElementName = "ResourceInfo")]
        public ArrayList Info = new ArrayList();
        [XmlElement(Type = typeof(ResourceAdditionalInfo), ElementName = "ResourceAdditionalInfo")]
        public ArrayList AdditionalInfo = new ArrayList();

        public ResourceInfo TryGeyInfo(string key)
        {
            foreach (object o in Info)
            { 
                if ((o as ResourceInfo).Key == key)
                    return (o as ResourceInfo);
            }

            return null;
        }

        public string TryGeyAdditionalInfo(string key)
        {
            foreach (object o in AdditionalInfo)
            { 
                if ((o as ResourceAdditionalInfo).Key == key)
                    return (o as ResourceAdditionalInfo).Value;
            }
            return null;
        }

        public static ResourceInfoList LoadList(string file)
        {
            ResourceInfoList infoList = null;

            if (File.Exists(file))
            {
                string resContent = null;
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    StreamReader sr = new StreamReader(fs);
                    resContent = sr.ReadToEnd();
                }
                infoList = XmlObjectSerializer.Deserialize<ResourceInfoList>(resContent);
            }

            return infoList;
        }

        public static ResourceInfoList LoadList(string root, string targetType, string targetID)
        {
            string resObj = string.Format(@"{0}\{1}\{2}\info.xml", root, targetType, targetID);
            return LoadList(resObj);
        }

        // does reading & writing at the same time causing conflicts?
        public void SaveList(string file)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                string json = XmlObjectSerializer.Serialize(this);
                sw.Write(json);
                sw.Close();
            }
        }

        public void SaveList(string root, string targetType, string targetID)
        {
            string resObj = string.Format(@"{0}\{1}\{2}\info.xml", root, targetType, targetID);
            SaveList(resObj);
        }
    }
}