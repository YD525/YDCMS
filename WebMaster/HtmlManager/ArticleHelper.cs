using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMaster.HtmlManager
{
   public class ArticleHelper
    {
        /// <summary>
        /// 获取所有文章栏目并关联自子父级
        /// </summary>
        /// <returns></returns>
        public static ArticleColumn GetAllArticleColumn()
        {
            ArticleColumn NArticleColumn = new ArticleColumn();
            List<ArticleItem> AllArticleItem = new List<ArticleItem>();
            DataTable NDataTable = SqlServerHelper.ExecuteDataTable("Select * From nav Where ColumnType='DefNav' And Parent=''");
            for(int i=0;i<NDataTable.Rows.Count;i++)
            {
                ArticleItem NArticleItem = new ArticleItem();
                NArticleItem.ID = int.Parse(DataHelper.ObjToStr(NDataTable.Rows[i]["ID"]));
                NArticleItem.ColumnName = DataHelper.ObjToStr(NDataTable.Rows[i]["ColumnName"]);
                NArticleItem.ColumnType= DataHelper.ObjToStr(NDataTable.Rows[i]["ColumnType"]);
                NArticleItem.LockerTemplate= DataHelper.ObjToStr(NDataTable.Rows[i]["LockerTemplate"]);
                NArticleItem.ColumnHref = DataHelper.ObjToStr(NDataTable.Rows[i]["ColumnHref"]);
                NArticleItem.ColumnOrder= DataHelper.ObjToStr(NDataTable.Rows[i]["ColumnOrder"]);
                DataTable Lv2 = SqlServerHelper.ExecuteDataTable("Select * From nav Where ColumnType='Lv2Nav'");
                for (int ir = 0; ir < Lv2.Rows.Count; ir++)
                {
                    if (DataHelper.ObjToStr(Lv2.Rows[ir]["Parent"]).Contains("(" + NArticleItem.ColumnName.ToString() + ")"))
                    {
                        NArticleItem.ParentColumn.Add(new Lv2ArticleColumn(int.Parse(DataHelper.ObjToStr(Lv2.Rows[ir]["ID"])),DataHelper.ObjToStr(Lv2.Rows[ir]["ColumnName"])));
                    }
                }
                AllArticleItem.Add(NArticleItem);
            }
            NArticleColumn.ArticleColumnList = AllArticleItem;
            DataTable StandTable = SqlServerHelper.ExecuteDataTable("Select * From nav Where ColumnType='Lv2Nav' And Parent=''");
            for (int irr = 0; irr < StandTable.Rows.Count; irr++)
            {
                NArticleColumn.StandColumnList.Add(new StandArticleColumn(int.Parse(DataHelper.ObjToStr(StandTable.Rows[irr]["ID"])), DataHelper.ObjToStr(StandTable.Rows[irr]["ColumnName"])));
            }
            return NArticleColumn;
        }
        public static Article QuickGetArticle(int ID)
        {
            DataTable NDataTable = SqlServerHelper.ExecuteDataTable("Select * From product Where ProductID=" + ID);
            if (NDataTable.Rows.Count > 0)
            {
                Article NArticle = new Article();
                NArticle.ProductID =int.Parse(DataHelper.ObjToStr(NDataTable.Rows[0]["ProductID"]));
                NArticle.ProductOrder=DataHelper.StrToInt(DataHelper.ObjToStr(NDataTable.Rows[0]["ProductOrder"]));
                NArticle.ProductName = DataHelper.ObjToStr(NDataTable.Rows[0]["ProductName"]);
                NArticle.ProductDescribe = DataHelper.ObjToStr(NDataTable.Rows[0]["ProductDescribe"]);
                NArticle.ProductHref = DataHelper.ObjToStr(NDataTable.Rows[0]["ProductHref"]);
                NArticle.ProductSeoDescribe = DataHelper.ObjToStr(NDataTable.Rows[0]["ProductSeoDescribe"]);
                NArticle.ProductSeoTittle = DataHelper.ObjToStr(NDataTable.Rows[0]["ProductSeoTittle"]);
                NArticle.GetSet = DataHelper.ObjToStr(NDataTable.Rows[0]["GetSet"]);
                NArticle.ProductTittle = DataHelper.ObjToStr(NDataTable.Rows[0]["ProductTittle"]);
                NArticle.ProductShortTittle = DataHelper.ObjToStr(NDataTable.Rows[0]["ProductShortTittle"]);
                NArticle.AllImageURL = DataHelper.ObjToStr(NDataTable.Rows[0]["AllImageURL"]);
                NArticle.Type = DataHelper.ObjToStr(NDataTable.Rows[0]["Type"]);
                NArticle.ProductText =PIN.Decrypt(DataHelper.ObjToStr(NDataTable.Rows[0]["ProductText"]));
                if (NArticle.ProductText == "ERROR") NArticle.ProductText = "";
                NArticle.IsStatic = DataHelper.ObjToStr(NDataTable.Rows[0]["IsStatic"]);
                NArticle.Createpath = DataHelper.ObjToStr(NDataTable.Rows[0]["Createpath"]);
                NArticle.Creattime = DataHelper.ObjToStr(NDataTable.Rows[0]["Creattime"]);
                NArticle.Creat = DataHelper.ObjToStr(NDataTable.Rows[0]["Creat"]);
                return NArticle;
            }
            else
            {
                return new Article();
            }
        }


      
    }

    public class Article
    {
        public int ProductID = 0;
        public int ProductOrder = 0;
        public string ProductName = "";//文章的名称
        public string ProductDescribe = "";//文章的描述
        public string ProductHref = "";//文章的指向
        public string ProductSeoDescribe = "";//文章SEO描述
        public string ProductSeoTittle = "";//文章SEO标题
        public string GetSet = "";//文章扩展字段
        public string ProductTittle = "";//文章的标题
        public string ProductShortTittle = "";//文章的简略标题
        public string AllImageURL = "";//文章的缩略图
        public string Type = "";//文章的类型
        public string ProductText = "";//文章的正文
        public string IsStatic = "";//文章是否属于静态文章
        public string Creat = "";//文章是否被引擎创建
        public string Createpath = "";//文章的创建路径
        public string Creattime = "";//文章的创建时间
    }
    public class Lv2ArticleColumn
    {
        public int ID = 0;
        public string ColumnName = "";
        public Lv2ArticleColumn(int ID,string ColumnName)
        {
            this.ID = ID;
            this.ColumnName = ColumnName;
        }
    }

      public class StandArticleColumn
     {
        public int ID = 0;
        public string ColumnName = "";
        public StandArticleColumn(int ID,string ColumnName)
        {
            this.ID = ID;
            this.ColumnName = ColumnName;
        }
    }
    public class ArticleItem
    {
        public int ID = 0;
        public string ColumnName = "";
        public string ColumnType = "";
        public string LockerTemplate = "";
        public string ColumnHref = "";
        public string ColumnOrder = "";
        public List<Lv2ArticleColumn> ParentColumn = new List<Lv2ArticleColumn>();
    }
    public class ArticleColumn
    {
        public List<ArticleItem> ArticleColumnList = new List<ArticleItem>();
        public List<StandArticleColumn> StandColumnList = new List<StandArticleColumn>();
    }
}
