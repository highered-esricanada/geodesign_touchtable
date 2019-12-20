using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows;

namespace TablePlus.ElementsDB.DBBridge.Extra.ResourceHandlers
{
    public class DaeGenerator : ScriptedHandler
    {
        protected string LayerRuby;
        protected string SubName;

        public DaeGenerator(string layerRuby, string subName)
            : base(GetDaeScript(layerRuby), "")
        {
            LayerRuby = layerRuby;
            SubName = subName;
        }

        protected static string GetDaeScript(string layerRuby)
        {
            try
            {
                string text = File.ReadAllText(@"scripts\DaeGenerator.rb");
                text = text.Replace("{%LAYER_RUBY%}", layerRuby);

                return text;
            }
            catch (Exception e)
            {
                //MessageBox.Show("Cannot load script: scripts\\DaeGenerator.rb\n" + e.Message);
                return "";
            }
        }

        public override string ResourceKeyword
        {
            get { return SubName + "-dae"; }
        }

        public override string DefaultExtension
        {
            get { return "dae"; }
        }

        public override DBCaseResource GetSourceResource(List<DBCaseResource> resources)
        {
            foreach (DBCaseResource r in resources)
            {
                if (Path.GetExtension(r._PathWeb).ToLower() == ".skp" &&
                    (!r._MediaTitle.ToLower().Contains("massing")))
                    return r;
            }

            return null;
        }

    }

    public class MassingDaeGenerator : DaeGenerator
    {
        protected static string LoadLayerScript()
        {
            try
            {
                string text = File.ReadAllText(@"scripts\MassingDaeLayers.rb");
                return text;
            }
            catch (Exception e)
            {
                //MessageBox.Show("Cannot load script: scripts\\MassingDaeLayers.rb\n" + e.Message);
                return "";
            }
        }

        public MassingDaeGenerator()
            : base(LoadLayerScript(), "massing")
        { }
    }

    public class HDDaeGenerator : DaeGenerator
    {
        protected static string LoadLayerScript()
        {
            try
            {
                string text = File.ReadAllText(@"scripts\HDDaeLayers.rb");
                return text;
            }
            catch (Exception e)
            {
                //MessageBox.Show("Cannot load script: scripts\\HDDaeLayers.rb\n" + e.Message);
                return "";
            }
        }

        public HDDaeGenerator()
            : base(LoadLayerScript(), "hd")
        { }
    }

}
