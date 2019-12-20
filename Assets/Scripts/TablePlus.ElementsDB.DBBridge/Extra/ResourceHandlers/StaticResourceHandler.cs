using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using TablePlus.Common.Base;

namespace TablePlus.ElementsDB.DBBridge.Extra.ResourceHandlers
{
    public class StaticResourceHandler : BaseResourceHandler
    {
        public string SubName { get; protected set; }
        public string Extension { get; protected set; }
        public string ResKeyWords { get; protected set; }

        protected string ResourceExtension;

        public StaticResourceHandler(string subName, string extension, string resKeyWords)
        {
            SubName = subName;
            Extension = extension;
            ResKeyWords = resKeyWords;
        }

        public override string ResourceKeyword
        {
            get { return "static-" + SubName; }
        }

        public override string DefaultExtension
        {
            get { return Extension; }
        }

        public DBCaseResource GetSourceResource(List<DBCaseResource> resources)
        {
            foreach (DBCaseResource r in resources)
            {
                if ((Path.GetExtension(r._PathWeb).ToLower() == "." + Extension.ToLower() || Extension == "") &&
                    (r._MediaTitle.ToLower().Contains(ResKeyWords.ToLower())))
                {
                    ResourceExtension = Path.GetExtension(r._PathWeb).Substring(1);
                    return r;
                }
            }

            return null;
        }

        public override HandleCaseResult HandleCase(DBObject theObject, List<DBCaseResource> resourceList, string root, 
            string targetType, string targetID, bool overwrite)
        {
            DBCaseResource cr = GetSourceResource(resourceList);
            HandleCaseResult hcr = new HandleCaseResult();
            hcr.Succeed = false;
            if (cr == null)
                return hcr;

            string resObj = string.Format(@"{0}\{1}\{2}\{3}.{4}",
                root, targetType, targetID, ResourceKeyword, ResourceExtension);

            if (File.Exists(resObj) && !overwrite)
            {
                hcr.Succeed = false;
                return hcr;
            }

            string toDown = Elements.WebServer + cr._PathWeb;
            if (File.Exists(resObj))
                File.Delete(resObj);

            Directory.CreateDirectory(Path.GetDirectoryName(resObj));

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(toDown, resObj);
            }

            hcr.AcceptedSource = Elements.WebServer + cr._PathWeb;
            hcr.ResultFile = resObj;
            hcr.SourceTimestamp = WebHelper.LastModifiedDate(hcr.AcceptedSource);
            hcr.Succeed = true;

            return hcr;
        }
    }

    public class KeyPhotoHandler : StaticResourceHandler
    {
        public KeyPhotoHandler()
            : base("keyphoto", "", "key")
        { 
        
        }
    }

    public class IsoModelHandler : StaticResourceHandler
    {
        public IsoModelHandler()
            : base("iso-model", "", "3d model view")
        {

        }
    }

}
