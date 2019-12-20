using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using TablePlus.ElementsDB.DBBridge;

namespace TablePlus.ElementsDB.DBBridge.Extra.ResourceHandlers
{
    /// <summary>
    /// Summary description for BaseResourceHandler
    /// </summary>
    public abstract class BaseResourceHandler
    {
        public abstract string ResourceKeyword { get; }
        public abstract string DefaultExtension { get; }

        public abstract HandleCaseResult HandleCase(DBObject theObject,
            List<DBCaseResource> resourceList,
            string root, string targetType, string targetID, bool overwrite);
    }

    public class HandleCaseResult
    {
        public bool Succeed { get; set; }
        public string AcceptedSource { get; set; }
        public string ResultFile { get; set; }
        public DateTime SourceTimestamp { get; set; }
        public Dictionary<string, string> AdditionalInfo { get; protected set; }
        public Dictionary<string, string> PrivateAdditionalInfo { get; protected set; }

        public HandleCaseResult()
        {
            AdditionalInfo = new Dictionary<string, string>();
            PrivateAdditionalInfo = new Dictionary<string, string>();
        }
    }
}