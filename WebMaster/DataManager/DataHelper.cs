using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMaster.HtmlManager;

namespace WebMaster
{
    public class DataHelper
    {
        public static List<atclsinfile> allcodeinfile = new List<atclsinfile>();
        public static string ObjToStr(object obj)
        {
            if (obj == null) return "";
            return obj.ToString();
        }
        public static List<string> readfile(string filepath)
        {
            try {
                StreamReader rd = new StreamReader(filepath, Encoding.UTF8);

                StringBuilder sb = new StringBuilder();
                while (!rd.EndOfStream)
                {
                    string dqstr = rd.ReadLine();
                    sb = sb.Append(dqstr + "\r\n");
                }

                rd.Close();
                rd.Dispose();
                return StrToList(sb.ToString());
            } 
            catch
            {
                return new List<string>();
            }
        }
        public static byte[] StrToStream(string Str)
        {
            return Encoding.UTF8.GetBytes(Str);
        }
       

        public static byte[] GetBytesByFilePath(string strFile)
        {
            byte[] photo_byte = null;
            using (FileStream fs =
            new FileStream(strFile, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    photo_byte = br.ReadBytes((int)fs.Length);
                }
            }
            return photo_byte;
        }

        public byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // 设置当前流的位置为流的开始 
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        public Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
        public static void writefile(string targetpath, string text, Encoding encodingtype)
        {
            try
            {
                StreamWriter FileWriter = new StreamWriter(targetpath, false, encodingtype);
                FileWriter.Write(text);
                FileWriter.Close();
                FileWriter.Dispose();
                GC.Collect();
            }
            catch { }
        }
        public static void writefileappend(string targetpath, string text, Encoding encodingtype)
        {
            try
            {
                StreamWriter FileWriter = new StreamWriter(targetpath, true, encodingtype);
                FileWriter.Write(text);
                FileWriter.Close();
                FileWriter.Dispose();
                GC.Collect();
            }
            catch { }
        }
        public static bool creatfile(string targetpath)
        {
            bool iserror = false;
            if (File.Exists(targetpath) == false)
            {
                File.Create(targetpath);
            }
            else
            {
                iserror = true;
            }
            return iserror;
        }

        //重构streamreader
        public static System.Text.Encoding GetFileEncodeType(string filename)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
            Byte[] buffer = br.ReadBytes(2);
            if (buffer[0] >= 0xEF)
            {
                if (buffer[0] == 0xEF && buffer[1] == 0xBB)
                {
                    fs.Close();
                    br.Close();
                    return System.Text.Encoding.UTF8;
                }
                else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                {
                    fs.Close();
                    br.Close();
                    return System.Text.Encoding.BigEndianUnicode;
                }
                else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                {
                    fs.Close();
                    br.Close();
                    return System.Text.Encoding.Unicode;
                }
                else
                {
                    fs.Close();
                    br.Close();
                    return System.Text.Encoding.Default;
                }
            }
            if (buffer[0] == 0x3c)//utf-8无bom格式
            {
                fs.Close();
                br.Close();
                return System.Text.Encoding.UTF8;
            }

            else
            {
                fs.Close();
                br.Close();
                return System.Text.Encoding.Default;
            }
            fs.Close();
            br.Close();
        }

        /// <summary>
        /// 复制文件夹
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="aimPath"></param>
        public static void CopyDir(string srcPath, string aimPath)
        {
            try
            {
                // 检查目标目录是否以目录分割字符结束如果不是则添加
                if (aimPath[aimPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                {
                    aimPath += System.IO.Path.DirectorySeparatorChar;
                }
                // 判断目标目录是否存在如果不存在则新建
                if (!System.IO.Directory.Exists(aimPath))
                {
                    System.IO.Directory.CreateDirectory(aimPath);
                }
                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
                // string[] fileList = Directory.GetFiles（srcPath）；
                string[] fileList = System.IO.Directory.GetFileSystemEntries(srcPath);
                // 遍历所有的文件和目录
                foreach (string file in fileList)
                {
                    // 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                    if (System.IO.Directory.Exists(file))
                    {
                        CopyDir(file, aimPath + System.IO.Path.GetFileName(file));
                    }
                    // 否则直接Copy文件
                    else
                    {
                        System.IO.File.Copy(file, aimPath + System.IO.Path.GetFileName(file), true);
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }


        /// <summary>
        /// htmlformatinupt
        /// </summary>
        /// <param name="path"></param>
        /// <param name="strformat"></param>
        /// <returns></returns>
        public static string pathtohtml(string urlpath, string url, string strformat, string deffilepath, string locker)
        {
            if (locker == null == false)
            {
                locker = "图片被引用:" + locker;
            }
            string filename = urlpath.Substring(urlpath.LastIndexOf(@"\") + 1);

            FileInfo newinfo = new FileInfo(deffilepath + urlpath);
            int getsize = Convert.ToInt32(newinfo.Length / 1024);

            string creatid = urlpath + ">" + filename + ">" + getsize.ToString();

            string getpath = urlpath.Replace(@"\", "/");
            getpath = string.Format(strformat.Replace("'", "\""), creatid, url + getpath, getsize + "kb", locker);
            return getpath;
        }


        public static string checkimagelist(List<string> imagelist, int maxsize = 0, int minsize = 0, int py = 0)
        {
            string richtext = "";
            foreach (var str in imagelist)
            {
                System.IO.FileInfo fileInfo = null;
                try
                {
                    fileInfo = new System.IO.FileInfo(str);
                    int size = Convert.ToInt32(fileInfo.Length / 1024);
                    if (size > maxsize)
                    {
                        if (CompressImage(str, str + "_back", true, 90, maxsize))
                        {
                            FileInfo newinfo = new FileInfo(str);
                            richtext += string.Format("位置:{0}图片原尺寸{1}kb被调整为{2}kb", str, size, Convert.ToInt32(newinfo.Length / 1024));
                        }
                    }
                    else
                    if (minsize == 0 == false)
                    {
                        if (size > minsize)
                        {

                            if (size - py <= minsize)
                            {
                                if (CompressImage(str, str + "_back", true, 95, minsize))
                                {
                                    FileInfo newinfo = new FileInfo(str);
                                    richtext += string.Format("位置:{0}图片原尺寸{1}kb被优化为{2}kb", str, size, Convert.ToInt32(newinfo.Length / 1024));
                                }


                            }
                        }

                    }

                }
                catch
                {
                    richtext += "图片压缩失败!:" + str + "\r\n";
                }


            }
            return richtext;
        }


        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="sFile">原图片地址</param>
        /// <param name="dFile">压缩后保存图片地址</param>
        /// <param name="flag">压缩质量（数字越小压缩率越高）1-100</param>
        /// <param name="size">压缩后图片的最大大小</param>
        /// <param name="sfsc">是否是第一次调用</param>
        /// <returns></returns>
        public static bool CompressImage(string sFile, string dFile, bool writesfile, int flag = 90, int size = 300, bool sfsc = true)
        {

            System.Drawing.Image iSource = System.Drawing.Image.FromFile(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            //如果是第一次调用，原始图像的大小小于要压缩的大小，则直接复制文件，并且返回true
            FileInfo firstFileInfo = new FileInfo(sFile);
            if (sfsc == true && firstFileInfo.Length < size * 1024)
            {
                if (File.Exists(dFile))
                {
                    File.Delete(dFile);
                }
                firstFileInfo.CopyTo(dFile);
                return true;
            }

            int dHeight = iSource.Height / 2;
            int dWidth = iSource.Width / 2;
            int sW = 0, sH = 0;
            //按比例缩放
            Size tem_size = new Size(iSource.Width, iSource.Height);
            if (tem_size.Width > dHeight || tem_size.Width > dWidth)
            {
                if ((tem_size.Width * dHeight) > (tem_size.Width * dWidth))
                {
                    sW = dWidth;
                    sH = (dWidth * tem_size.Height) / tem_size.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = (tem_size.Width * dHeight) / tem_size.Height;
                }
            }
            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }

            Bitmap ob = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(ob);

            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            g.Dispose();

            //以下代码为保存图片时，设置压缩质量
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;

            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径
                    FileInfo fi = new FileInfo(dFile);
                    if (fi.Length > 1024 * size)
                    {
                        flag = flag - 10;
                        CompressImage(sFile, dFile, writesfile, flag, size, false);
                    }
                }
                else
                {
                    ob.Save(dFile, tFormat);
                }
                if (writesfile)
                {
                    iSource.Dispose();
                    ob.Dispose();
                    File.Delete(sFile);
                    File.Move(dFile, sFile);

                }
                return true;
            }
            catch
            {
                iSource.Dispose();
                ob.Dispose();
                return false;
            }
            finally
            {
                iSource.Dispose();
                ob.Dispose();
            }
        }
        public static string ListToSrt(List<string> obj)
        {
            string RichText = "";
            if (obj.Count == 1)
            {
                return obj[0];
            }
            else
            {
                RichText = string.Join("\r\n", obj.ToArray());
                return RichText;
            }
         
        }
        public static bool StrToBool(string obj)
        {
            bool NextBool = false;
            bool.TryParse(obj, out NextBool);
            return NextBool;
        }
        public static List<string> StrToList(string obj)
        {
            List<string> AllLine = new List<string>();
            if (obj.Contains("\r\n"))
            {
                AllLine=obj.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else
            {
                AllLine.Add(obj);
            }
            return AllLine;
        }
        public static int StrToInt(string Number)
        {
            int NNumber = 0;
            int.TryParse(Number, out NNumber);
            return NNumber;
        }
        /// <summary>
        /// 删除文件夹下所有内容
        /// </summary>
        /// <param name="strPath"></param>
        public static void DeleteFolder(string strPath)
        {
            //删除这个目录下的所有子目录
            if (Directory.GetDirectories(strPath).Length > 0)
            {
                foreach (string fl in Directory.GetDirectories(strPath))
                {
                    Directory.Delete(fl, true);
                }
            }
            //删除这个目录下的所有文件
            if (Directory.GetFiles(strPath).Length > 0)
            {
                foreach (string f in Directory.GetFiles(strPath))
                {
                    System.IO.File.Delete(f);
                }
            }
            Directory.Delete(strPath, true);
        }
        /// <summary>
        /// 遍历文件夹
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filetype"></param>
        /// <returns></returns>
        public static List<FileInformation> getallfile(string filepath, string replacestr = "", List<string> filetype = null)
        {
            DirectoryAllFiles.FileList.Clear();
            if (replacestr == "") { replacestr = " "; }
            List<FileInformation> list = DirectoryAllFiles.GetAllFiles(new System.IO.DirectoryInfo(filepath), replacestr);
            List<FileInformation> nlist = new List<FileInformation>();

            if (filetype == null == false)
            {
                nlist.AddRange(list);
                foreach (var autoget in list)
                {
                    if (filetype.Contains(autoget.Filetype) == false)
                    {
                        nlist.Remove(autoget);
                    }
                }
                GC.Collect();
                return nlist;
            }
            GC.Collect();
            return list;
        }

        public static List<FileInformation> GetAllFileCode(string filepath, string replacestr = "", List<string> filetype = null)
        {
            DirectoryAllFilesCode.FileList.Clear();
            if (replacestr == "") { replacestr = " "; }
            List<FileInformation> list = DirectoryAllFilesCode.GetAllFiles(new System.IO.DirectoryInfo(filepath), replacestr);
            List<FileInformation> nlist = new List<FileInformation>();

            if (filetype == null == false)
            {
                nlist.AddRange(list);
                foreach (var autoget in list)
                {
                    if (filetype.Contains(autoget.Filetype) == false)
                    {
                        nlist.Remove(autoget);
                    }
                }
                GC.Collect();
                return nlist;
            }
            GC.Collect();
            return list;
        }


    }

    public class DirectoryAllFiles
    {
        public static List<FileInformation> FileList = new List<FileInformation>();
        public static List<FileInformation> GetAllFiles(DirectoryInfo dir, string dqpath)
        {
            FileInfo[] allFile = dir.GetFiles();
            foreach (FileInfo fi in allFile)
            {
                FileList.Add(new FileInformation { FileName = fi.Name, FilePath = fi.FullName.Replace(dqpath, ""), Filetype = fi.Extension });
            }
            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo d in allDir)
            {
                GetAllFiles(d, dqpath);
            }
            return FileList;
        }
    }
    public class DirectoryAllFilesCode
    {
        public static List<FileInformation> FileList = new List<FileInformation>();
        public static List<FileInformation> GetAllFiles(DirectoryInfo dir, string dqpath)
        {
            FileInfo[] allFile = dir.GetFiles();
            foreach (FileInfo fi in allFile)
            {
                List<string> AllCode = new List<string>();
                AllCode = DataHelper.readfile(fi.FullName);
                FileList.Add(new FileInformation { FileName = fi.Name, FilePath = fi.FullName.Replace(dqpath, ""), Filetype = fi.Extension,FileCode = AllCode });
            }
            DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (DirectoryInfo d in allDir)
            {
                GetAllFiles(d, dqpath);
            }
            return FileList;
        }
    }



    public struct atclsinfile
    {
        public string isname { get; set; }
        public string iscode { get; set; }
    }

    public class FileInformation
    {
        public string Filetype { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public List<string> FileCode { get; set; }
    }
}
