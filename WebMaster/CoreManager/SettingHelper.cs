using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMaster.DataManager;
using WebMaster.HtmlManager;

namespace WebMaster.CoreManager
{
   public class SettingHelper
    {
        public static void initialization()
        {
            DeFine.DefWebUrl = CMSHelper.GetDeFineValue("DEFURL");
            DeFine.DefWebRequestUrl= CMSHelper.GetDeFineValue("WebRequestURL");
            DeFine.WallMaxConnect =DataHelper.StrToInt(CMSHelper.GetDeFineValue("WallMaxConnect"));
        }
    }
}
