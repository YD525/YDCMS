using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMaster.DataManager
{
    public class DeFine
    {
        public static string DBIPAddress = PIN.AESEncrypt("xxx.com");
        public static string DBName = "xxx";
        public static int DBPort = 3306;
        public static string DBUSName = PIN.Encrypt("xxx");
        public static string DBUSPassword = PIN.Encrypt("xxx");
        public static string TemplatesPath = AppDomain.CurrentDomain.BaseDirectory + @"HtmlTemplate\";
        public static string SystemFilePath = AppDomain.CurrentDomain.BaseDirectory + @"DataFile\";
        public static string UPLoadFile = AppDomain.CurrentDomain.BaseDirectory + @"FileList\";
        public static string DefCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static string DefWebRequestUrl = "AutoGet";
        public static string DefWebUrl = "AutoGet";
        public static string ArticleSavePath = AppDomain.CurrentDomain.BaseDirectory + @"ArticleList\";
        public static string PluginPath = AppDomain.CurrentDomain.BaseDirectory + @"pulgins\";
        public static int WallMaxConnect = 0;
        public static string GetRootURI()
        {
            return DefWebUrl;
            string AppPath = "";
            HttpContext HttpCurrent = HttpContext.Current;
            HttpRequest Req;
            if (HttpCurrent != null)
            {
                Req = HttpCurrent.Request;

                string UrlAuthority = Req.Url.GetLeftPart(UriPartial.Authority);
                if (Req.ApplicationPath == null || Req.ApplicationPath == "/")
                    //直接安装在   Web   站点   
                    AppPath = UrlAuthority;
                else
                    //安装在虚拟子目录下   
                    AppPath = UrlAuthority + Req.ApplicationPath;
            }
            return AppPath;
        }
    }


}
