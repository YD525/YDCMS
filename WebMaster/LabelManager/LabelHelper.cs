using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Shapes;

namespace WebMaster.LabelManager
{
    public class LabelHelper
    {
        public static string CurrentHtmlAction = "";
        public static void Initialization(string HtmlAction)
        {
            CurrentHtmlAction = HtmlAction;
        }
        public static string ProcessingLabelValue(string LockerLine)
        {
            return LockerLine;
        }
      
    }
}