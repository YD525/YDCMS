using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using WebMaster.DataManager;
using WebMaster.HtmlManager;

namespace WebMaster.BlockChainManager
{
   public class BlockChainHelper
    {
        /// <summary>
        /// 查询并校队区块链返回所在区域任何返回0皆为错误
        /// </summary>
        /// <returns></returns>
        public static int FindBlock(Block Item,string BlockID)
        {
            string LockerHash = Item.Hash;
            foreach (var Get in ReadAllBlockByHead(ReadBlockHead(BlockID)))
            {
                if (Get.PrevHash == LockerHash)
                {
                    return Get.BlockIndex - 1;
                }
                if (Get.Hash == LockerHash)
                {
                    return Get.BlockIndex;
                }
            }
            return 0;
        }

        /// <summary>
        /// 存储区块链
        /// </summary>
        /// <param name="AllBlock"></param>
        public static int SaveToDB(List<Block> AllBlock)
        {
            if (AllBlock.Count > 0)
            {
                SqlServerHelper.ExecuteNonQuery("DELETE BlockList Where BlockID='" + AllBlock[0].BlockID + "';");//删除老的区块链
                int SucessCount = 0;
                foreach (var Get in AllBlock)
                {
                    string SqlOder = "INSERT INTO BlockList(BlockID,BlockIndex,TimeStamp,BPM,BlockStream,Hash,PrevHash,Access) VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}');";
                    int state = SqlServerHelper.ExecuteNonQuery(string.Format(SqlOder, Get.BlockID, Get.BlockIndex, Get.TimeStamp, Get.BPM, PIN.Encrypt(JsonHelper.DataFormatToJson(Get.BlockStream)), Get.Hash, Get.PrevHash, Get.Access));
                    if (state == 0 == false)
                    {
                        //Sucess
                        SucessCount++;
                    }
                }
                return SucessCount;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 创建一个新的区块链
        /// </summary>
        public static List<Block> CreatBlock(string ID, byte[] NStream,int BlockSize, Accessibility Access)
        {
            List<Block> AllBlock = new List<Block>();//一整套区块链

            List<byte[]> NFileList = DeStream(NStream, BlockSize);
            for (int i = 0; i < NFileList.Count; i++)
            {
                Block NBlock = new Block();
                NBlock.BlockIndex = i;
                NBlock.BlockID = ID;
                NBlock.BlockStream = NFileList[i];
                NBlock.TimeStamp = TimeHelper.DateTimeToStamp(DateTime.Now);
                NBlock.BPM = NFileList[i].Length;
                NBlock.Access = Access;

                if (i == 0)
                {
                    NBlock.PrevHash = "";
                }
                else
                {
                    NBlock.PrevHash = AllBlock[i-1].Hash;
                }
                NBlock.Hash = GetHashCode(NBlock);
                AllBlock.Add(NBlock);
            }
            return AllBlock;
        }

        /// <summary>
        /// 获取区块链可视化值用于分享
        /// </summary>
        /// <returns></returns>
        public static string GetBlockKey(Block Item)
        {
            string GetCode = JsonHelper.DataFormatToJson(Item);
            return PIN.Encrypt(GetCode);
        }

        /// <summary>
        /// 块合并
        /// </summary>
        /// <param name="AllByte"></param>
        /// <returns></returns>
        public static byte[] MergeBytes(List<byte[]> AllByte)
        {
            int MaxCount = 0;
            foreach (var Get in AllByte)
            {
                if(Get==null==false)
                MaxCount += Get.Length;
            }
            byte[] ReturnByte = new byte[MaxCount];
            int Offset = 0;
            for (int i = 0; i < AllByte.Count; i++)
            {
                if(AllByte[i]==null==false)
                for (int ir = 0; ir < AllByte[i].Length; ir++)
                {
                    ReturnByte[Offset] = AllByte[i][ir];
                    Offset++;
                }
            }
            return ReturnByte;
        }

        /// <summary>
        /// 拆分块
        /// </summary>
        /// <param name="superbyte"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static List<byte[]> DeStream(byte[] superbyte, int size)
        {
            List<byte[]> result = new List<byte[]>();
            int length = superbyte.Length;
            int count = length / size;
            int r = length % size;
            for (int i = 0; i < count; i++)
            {
                byte[] newbyte = new byte[size];
                newbyte = superbyte.Skip(size * i).Take(size).ToArray();
                result.Add(newbyte);
            }
            if (r != 0)
            {
                byte[] newbyte = new byte[r];
                newbyte = superbyte.Skip(length - r).Take(r).ToArray();
                result.Add(newbyte);
            }
            return result;
        }
        /// <summary>
        /// 获取区块链头
        /// </summary>
        public static Block ReadBlockHead(string BlockID)
        {
            DataTable NTable = SqlServerHelper.ExecuteDataTable("Select * From BlockList Where BlockID ='"+ BlockID + "' And PrevHash=''");
            if (NTable.Rows.Count > 0)
            {
                //取链头
                Block NBlock = new Block();
                NBlock.BlockIndex = DataHelper.StrToInt(DataHelper.ObjToStr(NTable.Rows[0]["BlockIndex"]));
                NBlock.BlockID = DataHelper.ObjToStr(NTable.Rows[0]["BlockID"]);
                NBlock.BlockStream = JsonConvert.DeserializeObject<byte[]>(PIN.Decrypt(DataHelper.ObjToStr(NTable.Rows[0]["BlockStream"])));
                NBlock.TimeStamp = DataHelper.ObjToStr(NTable.Rows[0]["TimeStamp"]);
                NBlock.BPM = DataHelper.StrToInt(DataHelper.ObjToStr(NTable.Rows[0]["BPM"]));
                string GetType = DataHelper.ObjToStr(NTable.Rows[0]["Access"]);
                if (GetType == "Public")
                {
                    NBlock.Access = Accessibility.Public;
                }
                else
                {
                    NBlock.Access = Accessibility.Private;
                }
                NBlock.PrevHash = DataHelper.ObjToStr(NTable.Rows[0]["PrevHash"]);
                NBlock.Hash = DataHelper.ObjToStr(NTable.Rows[0]["Hash"]);

                return NBlock;
            }
            else
            {
                NTable = SqlServerHelper.ExecuteDataTable("Select * From BlockList Where Hash ='" + BlockID + "' And PrevHash=''");
                if (NTable.Rows.Count > 0)
                {
                    Block NBlock = new Block();
                    NBlock.BlockIndex = DataHelper.StrToInt(DataHelper.ObjToStr(NTable.Rows[0]["BlockIndex"]));
                    NBlock.BlockID = DataHelper.ObjToStr(NTable.Rows[0]["BlockID"]);
                    NBlock.BlockStream = JsonConvert.DeserializeObject<byte[]>(PIN.Decrypt(DataHelper.ObjToStr(NTable.Rows[0]["BlockStream"])));
                    NBlock.TimeStamp = DataHelper.ObjToStr(NTable.Rows[0]["TimeStamp"]);
                    NBlock.BPM = DataHelper.StrToInt(DataHelper.ObjToStr(NTable.Rows[0]["BPM"]));
                    string GetType = DataHelper.ObjToStr(NTable.Rows[0]["Access"]);
                    if (GetType == "Public")
                    {
                        NBlock.Access = Accessibility.Public;
                    }
                    else
                    {
                        NBlock.Access = Accessibility.Private;
                    }
                    NBlock.PrevHash = DataHelper.ObjToStr(NTable.Rows[0]["PrevHash"]);
                    NBlock.Hash = DataHelper.ObjToStr(NTable.Rows[0]["Hash"]);

                    return NBlock;
                }
                else
                {
                    return new Block();
                }
            }
            return new Block();
        }

        /// <summary>
        /// 根据链头取出一整条区块链
        /// </summary>
        /// <param name="Head"></param>
        /// <returns></returns>
        public static List<Block> ReadAllBlockByHead(Block Head)
        {
            List<Block> AllBlock = new List<Block>();
            AllBlock.Add(Head);
            string Hash ="";
            Hash = Head.Hash;
            TryAgain:
            DataTable NTable = SqlServerHelper.ExecuteDataTable("Select Top 1 * From BlockList Where PrevHash='"+ Hash + "'");
            if (NTable.Rows.Count > 0)
            {
                Block NBlock = new Block();
                NBlock.BlockIndex = DataHelper.StrToInt(DataHelper.ObjToStr(NTable.Rows[0]["BlockIndex"]));
                NBlock.BlockID = DataHelper.ObjToStr(NTable.Rows[0]["BlockID"]);
                NBlock.BlockStream = JsonConvert.DeserializeObject<byte[]>(PIN.Decrypt(DataHelper.ObjToStr(NTable.Rows[0]["BlockStream"])));
                NBlock.TimeStamp = DataHelper.ObjToStr(NTable.Rows[0]["TimeStamp"]);
                NBlock.BPM = DataHelper.StrToInt(DataHelper.ObjToStr(NTable.Rows[0]["BPM"]));
                string GetType = DataHelper.ObjToStr(NTable.Rows[0]["Access"]);
                if (GetType == "Public")
                {
                    NBlock.Access = Accessibility.Public;
                }
                else
                {
                    NBlock.Access = Accessibility.Private;
                }
                NBlock.PrevHash = DataHelper.ObjToStr(NTable.Rows[0]["PrevHash"]);
                NBlock.Hash = DataHelper.ObjToStr(NTable.Rows[0]["Hash"]);
                AllBlock.Add(NBlock);
                Hash = NBlock.Hash;
                goto TryAgain;
            }
            else
            {
                return AllBlock;
            }
        }

        /// <summary>
        /// 存储性区块链输出到文件
        /// </summary>
        /// <param name="List"></param>
        /// <param name="SavePath"></param>
        public static void PutToFile(List<Block> List,string SavePath)
        {

            List<byte[]> AllByte = new List<byte[]>();
            foreach (var Get in List)
            {
                AllByte.Add(Get.BlockStream);
            }
            byte[] GetFile = MergeBytes(AllByte);

            FileStream fs = new FileStream(SavePath, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(GetFile);
            bw.Close();
            fs.Close();

        }

        public static byte[] PutToFile(List<Block> List)
        {

            List<byte[]> AllByte = new List<byte[]>();
            foreach (var Get in List)
            {
                AllByte.Add(Get.BlockStream);
            }
            byte[] GetFile = MergeBytes(AllByte);

            return GetFile;

        }
        /// <summary>
        /// 存储性区块链输出到字符串
        /// </summary>
        /// <param name="List"></param>
        /// <returns></returns>
        public static string PutToString(List<Block> List)
        {
            List<byte[]> AllByte = new List<byte[]>();
            foreach (var Get in List)
            {
                AllByte.Add(Get.BlockStream);
            }
            byte[] GetString = MergeBytes(AllByte);

            return Encoding.UTF8.GetString(GetString);
        }

        /// <summary>
        /// 获取当前块哈希值
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static string GetHashCode(Block Item)
        {
            return Item.GetHashCode().ToString();
        }

    

    }


    public class Block
    {
        /// <summary>
        /// 区块分类ID
        /// </summary>
        public string BlockID = "";
        /// <summary>
        /// 区块位置
        /// </summary>
        public int BlockIndex { get; set; }

        /// <summary>
        /// 区块生成时间戳
        /// </summary>
        public string TimeStamp { get; set; }
        /// <summary>
        /// 心率数值
        /// </summary>
        public int BPM { get; set; }

        /// <summary>
        /// 区块流
        /// </summary>

        public byte[] BlockStream;

        /// <summary>
        /// 区块 SHA-256 散列值
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// 前一个区块 SHA-256 散列值
        /// </summary>
        public string PrevHash { get; set; }

        public Accessibility Access { get; set; }
    }
    public enum Accessibility
    { 
    Public=0,Private=1
    }
}
