using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CaptureSceenShot
{
    class IoUtility
    {
        /// <summary>
        /// Write a text
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        public static void WriteText(string filePath, string content)
        {
            FileInfo fi = new FileInfo(filePath);
            var di = fi.Directory;
            if (di != null && !di.Exists)
                di.Create();
            if (!fi.Exists)
            {
                //创建文件
                fi.Create().Close();
            }

            StreamWriter sw = File.AppendText(filePath);
            sw.WriteLine(content);
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// 读取txt文件内容
        /// </summary>
        /// <param name="filePath">文件地址</param>
        public static string ReadTxtContent(string filePath)
        {
            StreamReader sr = new StreamReader(filePath, Encoding.Default);
            string content;
            while ((content = sr.ReadLine()) != null)
            {
                return content.ToString();
            }
            return string.Empty;
        }
    }
}
