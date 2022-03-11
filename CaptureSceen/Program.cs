using System;
using System.IO;

namespace CaptureSceenShot
{
    class Program
    {
        public static string GetVolume()
        {
            try
            {
                string filepath = Path.Combine(@"D:\HSW_TMS\local_config", "CaptureScreen.txt");
                if (File.Exists(filepath))
                {
                    var path = IoUtility.ReadTxtContent(filepath);
                    if (path.IndexOf(':') == -1 && string.IsNullOrEmpty(path) == false)
                        return path;
                    else
                        return "D";
                }
                else
                {
                    IoUtility.WriteText(filepath, "D");
                    return System.Windows.Forms.Application.StartupPath.Substring(0,
                        System.Windows.Forms.Application.StartupPath.IndexOf(':'));
                }
            }
            catch 
            {
                return System.Windows.Forms.Application.StartupPath.Substring(0,
                    System.Windows.Forms.Application.StartupPath.IndexOf(':'));
            }
            
        }
        static void Main(string[] args)
        {
            
            try
            {
                if (args.Length > 0)
                {
                    if (!string.IsNullOrEmpty(args[0].ToString()) && args.Length == 1)
                    {
                        CaptureScreenshot.CaptureScreenshotFrequency = Int32.Parse(args[0]);
                    }
                    else if (!string.IsNullOrEmpty(args[0].ToString()) && args.Length == 2)
                    {
                        CaptureScreenshot.CaptureScreenshotFrequency = Int32.Parse(args[0]);
                        CaptureScreenshot.Quality = Int32.Parse(args[1]);
                    }
                    else
                    {
                        Console.WriteLine("The parameter intput is incorrect .... should be  [Para1:Interval, Para2:Quality(1-100)]: e.g. CaptureSceenShot.exe 3 100 ");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The parameter input is incorrect .... should be : 3 10 " + ex.ToString());
            }
            try
            {
                Console.WriteLine("Starting the screen Capture......");
                Console.WriteLine(string.Format("Please Check the captured screenshots in {0}:\\TestReport......\n\n", GetVolume()));
                Console.WriteLine("*********************************************************************************************\n");
                Console.WriteLine("Function: 连续截取单屏或者多屏的全屏截图\n");
                Console.WriteLine("Parameters: 参数一：Interval截屏间隔时间， 参数二:图像质量Quality (取值：1-100)\n");
                Console.WriteLine("Sample_A: 没有参数，间隔一秒钟截取，并且图像质量按PrintScreen 图像质量的50%处理\n");
                Console.WriteLine("Sample_B: 接一个参数 [e.g.CaptureScreenShot.exe 3] 间隔三秒截屏一次，图像质量默认按50%处理\n");
                Console.WriteLine("Sample_C: 接两个参数 [e.g.CaptureScreenShot.exe 3 100] 间隔三秒钟截屏一次，并且图像质量无损\n");
                Console.WriteLine("配置文件路径: D:\\HSW_TMS\\local_config\\CaptureScreen.txt  通过修改盘符可以改变存储路径\n");
                Console.WriteLine("*********************************************************************************************\n");

                StartSystemMonitor();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.Read();
        }

        /// <summary>
        /// Function: StartSystemMonitor
        /// Summary: Start system monitor and generate the test report
        /// In: None
        /// Out: void
        /// </summary>
        public static void StartSystemMonitor()
        {
            var screenPath = CaptureScreenshot.ScreenShotPath + TraceLog.GetCurrentTime;
            CaptureScreenshot.CaptureScreenStart(@screenPath);
        }
    }
}
