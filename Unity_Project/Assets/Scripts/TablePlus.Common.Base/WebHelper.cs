using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace TablePlus.Common.Base
{
    public class WebHelper
    {
        public static DateTime LastModifiedDate(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            DateTime last = DateTime.Now;
            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            {
                last = res.LastModified;
                res.Close();
            }
            return last;
        }

        
    }
}
