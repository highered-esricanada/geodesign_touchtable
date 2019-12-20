using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using TablePlus.Common.Base;
using System.Reflection;
using System.Windows;

namespace TablePlus.ElementsDB.DBBridge.Extra.ResourceHandlers
{
    public abstract class ScriptedHandler : BaseResourceHandler
    {
        public string Script { get; protected set; }
        public string Functions { get; protected set; }

        protected string ProcessedScript;

        public ScriptedHandler(string script, string functions)
        {
            Script = script;
            Functions = functions;
            ProcessedScript = WrapScript(script, functions);
        }

        protected string WrapScript(string script, string function)
        {
            try
            {
                string text = File.ReadAllText(@"scripts\ScriptedHandler.rb");
                text = text.Replace("{%CODE_BODY%}", script);
                text = text.Replace("{%FUNCTIONS%}", function);
                return text;
            }
            catch (Exception e)
            { 
                //MessageBox.Show("Cannot load script: scripts\\ScriptedHandler.rb\n" + e.Message);
                return "";
            }

        }

        protected string DownloadSourceTemp(DBCaseResource cr,string toSave)
        {
            string toDown = Elements.WebServer + cr._PathWeb;

            Directory.CreateDirectory(Path.GetDirectoryName(toSave));

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(toDown, toSave);
            }

            return toSave;
        }

        public abstract DBCaseResource GetSourceResource(List<DBCaseResource> resources);

        protected virtual bool PostScriptProcessing(DBObject theObject, HandleCaseResult hcr)
        {
            return true;
        }

        public override HandleCaseResult HandleCase(DBObject theObject, 
            List<DBCaseResource> resourceList, 
            string root, string targetType, string targetID, bool overwrite)
        {
            DBCaseResource cr = GetSourceResource(resourceList);
            HandleCaseResult hcr = new HandleCaseResult();
            hcr.Succeed = false;
            if (cr == null)
                return hcr;

            string resObj = string.Format(@"{0}\{1}\{2}\{3}.{4}", 
                root, targetType, targetID, ResourceKeyword, "skp");

            if (File.Exists(resObj) && !overwrite)
            {
                hcr.Succeed = false;
                return hcr;
            }

            string source = DownloadSourceTemp(cr, resObj);

            hcr.Succeed = true;

            return hcr;
        }
    }
}
