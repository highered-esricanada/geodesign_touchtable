using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TablePlus.ElementsDB.DBBridge.Extra.ResourceHandlers;
using System.IO;

namespace TablePlus.ElementsDB.DBBridge.Extra
{
    /// <summary>
    /// Summary description for ResourceHandler
    /// </summary>
    public class ResourceHandler
    {
        protected Dictionary<string, BaseResourceHandler> InternalHandlers;

        protected static ResourceHandler InternalInstance;
        public static ResourceHandler Instance
        {
            get
            {
                if (InternalInstance == null)
                    InternalInstance = new ResourceHandler();

                return InternalInstance;
            }
        }

        public ResourceHandler()
        {
            InternalHandlers = new Dictionary<string, BaseResourceHandler>();
        }

        public Dictionary<string, BaseResourceHandler> Handlers
        {
            get { return InternalHandlers; }
        }

        public void RegisterHandler(BaseResourceHandler handler)
        {
            if (!InternalHandlers.ContainsKey(handler.ResourceKeyword))
                InternalHandlers.Add(handler.ResourceKeyword, handler);
        }

        public void UnregisterHandler(string type)
        {
            if (InternalHandlers.ContainsKey(type))
                InternalHandlers.Remove(type);
        }

        // should handle overwrite more reasonably!!!
        public void HandleCase(DBObject theObject,
            List<DBCaseResource> resourceList, 
            string root, string targetType, string targetID,
            bool overwrite)
        {
            ResourceInfoList ril = null;
            string resListName = string.Format(@"{0}\{1}\{2}\info.xml", root, targetType, targetID);

            if (File.Exists(resListName))
                ril = ResourceInfoList.LoadList(resListName);
            else
                ril = new ResourceInfoList();
            
            //foreach (BaseResourceHandler h in InternalHandlers.Values)

            for (int i=0; i<InternalHandlers.Values.Count; i++)
            {
                BaseResourceHandler h = InternalHandlers.Values.ElementAt(i);

                HandleCaseResult hcr = h.HandleCase(theObject, resourceList, root, targetType, targetID, overwrite);
                if (hcr.Succeed == false)
                {
                    System.Console.Out.WriteLine("Fail Warning: " + targetID + " " + hcr.ResultFile);
                    continue;
                }
                ResourceInfo ri = new ResourceInfo();
                string file = Path.GetFileName(hcr.ResultFile);
                ri.SourceModifiedDate = hcr.SourceTimestamp;
                ri.TargetModifiedDate = DateTime.Now;
                ri.Key = file;
                foreach (KeyValuePair<string, string> info in hcr.PrivateAdditionalInfo)
                {
                    ResourceAdditionalInfo ral = new ResourceAdditionalInfo();
                    ral.Key = info.Key;
                    ral.Value = info.Value;
                    ri.AdditionalInfo.Add(ral);
                }
                if (ril.TryGeyInfo(ri.Key) != null)
                    ril.Info.Remove(ril.TryGeyInfo(ri.Key));
                ril.Info.Add(ri);
                foreach (KeyValuePair<string, string> info in hcr.AdditionalInfo)
                {
                    ResourceAdditionalInfo ral = new ResourceAdditionalInfo();
                    ral.Key = info.Key;
                    ral.Value = info.Value;
                    // avoid adding duplicated info
                    if (ril.TryGeyAdditionalInfo(ral.Key) == null)
                        ril.AdditionalInfo.Add(ral);
                }
            }
            ril.Key = targetID;
            ril.SaveList(root, targetType, targetID);
        }

    }
}