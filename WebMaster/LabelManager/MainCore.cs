using WebMaster.SQLManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebMaster.LabelManager
{
    public class MainCore
    {

 
        /// <summary>
        /// html核心编译功能 Null为阻止添加行
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="HtmlAction"></param>
        public static string InputLine(string Line,string HtmlAction)
        {
            if (!SignHelper.ActionDifferent(HtmlAction))
            {
                LabelHelper.Initialization(HtmlAction);
            }
            string NextLine = LabelHelper.ProcessingLabelValue(Line);
            List<string> AllLine = SignHelper.ProcessingString(NextLine);
            if (AllLine.Count > 0)
            {
                return DataHelper.ListToSrt(AllLine);
            }
            else
            {
                return null;
            }
        }

       
    }
}