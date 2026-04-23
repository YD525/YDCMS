using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMaster.UserManager;

namespace WebMaster.HtmlManager
{
    public class EchartHelper
    {

        public static VisitChart GetVisitChart(string Year,string Month)
        {
            VisitChart NVisitChart = new VisitChart();
            NVisitChart.title.text = "Front-end access statistics";
            NVisitChart.title.subtext= "Front-end visitor report";
            string SqlOder = "select * from VisitHistory Where UPDataDay Like '" + Year + "_" + Month + "%';";
            DataTable NTable = SqlServerHelper.ExecuteDataTable(SqlOder);
            List<TypeValue> AllVisit = new List<TypeValue>();

            ChartLegend NChartLegend = new ChartLegend();
            for (int i = 0; i < NTable.Rows.Count; i++)
            {
                string UserName = DataHelper.ObjToStr(NTable.Rows[i]["UserName"]);
                string UPDataDay = DataHelper.ObjToStr(NTable.Rows[i]["UPDataDay"]);
                string FirstVisitUrl = DataHelper.ObjToStr(NTable.Rows[i]["FirstVisitUrl"]);
                string LastVisitUrl = DataHelper.ObjToStr(NTable.Rows[i]["LastVisitUrl"]);
                string ClickCount = DataHelper.ObjToStr(NTable.Rows[i]["ClickCount"]);
                TypeValue NTypeValue = new TypeValue();
                if (LastVisitUrl.Contains("/"))
                {
                    LastVisitUrl = LastVisitUrl.Substring(LastVisitUrl.LastIndexOf("/")+1);
                }
                NChartLegend.data.Add(LastVisitUrl);
                NTypeValue.name = UserName;
                NTypeValue.value = ClickCount;
                AllVisit.Add(NTypeValue);
            }
            NVisitChart.legend = NChartLegend;
            NVisitChart.series.data = AllVisit;

            return NVisitChart;
        }
    }

    public class TypeValue
    {
        public string name = "";
        public string value = "";
    }
    public class ChartSetting
    {
        public bool show = true;
    }

    public class ChartTitle
    {
        public string text = "";
        public string subtext = "";
        public string x = "center";//标题居中
    }

    public class ChartToolTip
    {
        public string trigger = "item";
        public string formatter = "{a} <br/>{b} : {c} ({d}%)";
    }

    public class ChartLegend
    {
        public string orient = "vertical";
        public string left = "left";
        public List<string> data = new List<string>();
    }
    public class ItemStyle
    {
        public Emphasis emphasis = new Emphasis();
    }
    public class Emphasis
    {
        public int shadowBlur = 10;
        public int shadowOffsetX = 0;
        public string shadowColor = "rgba(0, 0, 0, 0.5)";
    }
    public class ChartSeries
    {
        public string name = "";
        public string type = "pie";
        public string radius = "55%";
        public List<string> center = new List<string>() { "50%", "60%" };
        public List<TypeValue> data = new List<TypeValue>();
        public ItemStyle itemStyle = new ItemStyle();
    }
    public class VisitChart
    {
        public ChartSetting aria = new ChartSetting();
        public ChartTitle title = new ChartTitle();
        public ChartToolTip tooltip = new ChartToolTip();
        public ChartLegend legend = new ChartLegend();
        public ChartSeries series = new ChartSeries();
    }
}
