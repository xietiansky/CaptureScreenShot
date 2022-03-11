// -----------------------------------------------------------------------
// <copyright file="CaptureScreenClass.cs" company="Microsoft">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace CaptureSceenShot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    /// <summary>
    ///     The win 32 stuff.
    /// </summary>
    internal class Win32Stuff
    {
        #region Constants

        /// <summary>
        ///     The curso r_ showing.
        /// </summary>
        public const int CURSOR_SHOWING = 0x00000001;

        /// <summary>
        ///     The s m_ cxscreen.
        /// </summary>
        public const int SM_CXSCREEN = 0;

        /// <summary>
        ///     The s m_ cyscreen.
        /// </summary>
        public const int SM_CYSCREEN = 1;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The copy icon.
        /// </summary>
        /// <param name="hIcon">
        /// The h icon.
        /// </param>
        /// <returns>
        /// The <see cref="IntPtr"/>.
        /// </returns>
        [DllImport("user32.dll", EntryPoint = "CopyIcon")]
        public static extern IntPtr CopyIcon(IntPtr hIcon);

        /// <summary>
        /// The get cursor info.
        /// </summary>
        /// <param name="pci">
        /// The pci.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

        /// <summary>
        /// The get dc.
        /// </summary>
        /// <param name="ptr">
        /// The ptr.
        /// </param>
        /// <returns>
        /// The <see cref="IntPtr"/>.
        /// </returns>
        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        /// <summary>
        ///     The get desktop window.
        /// </summary>
        /// <returns>
        ///     The <see cref="IntPtr" />.
        /// </returns>
        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        /// <summary>
        /// The get icon info.
        /// </summary>
        /// <param name="hIcon">
        /// The h icon.
        /// </param>
        /// <param name="piconinfo">
        /// The piconinfo.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [DllImport("user32.dll", EntryPoint = "GetIconInfo")]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        /// <summary>
        /// The get system metrics.
        /// </summary>
        /// <param name="abc">
        /// The abc.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int abc);

        /// <summary>
        /// The get window dc.
        /// </summary>
        /// <param name="ptr">
        /// The ptr.
        /// </param>
        /// <returns>
        /// The <see cref="IntPtr"/>.
        /// </returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowDC")]
        public static extern IntPtr GetWindowDC(int ptr);

        /// <summary>
        /// The release dc.
        /// </summary>
        /// <param name="hWnd">
        /// The h wnd.
        /// </param>
        /// <param name="hDc">
        /// The h dc.
        /// </param>
        /// <returns>
        /// The <see cref="IntPtr"/>.
        /// </returns>
        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("User32.dll", EntryPoint = "DestroyIcon")]
        public static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll")]

        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("GDI32.dll")]

        public static extern bool DeleteObject(IntPtr objectHandle);
        #endregion

        /// <summary>
        ///     The cursorinfo.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            /// <summary>
            ///     The cb size.
            /// </summary>
            public int cbSize; // Specifies the size, in bytes, of the structure. 

            /// <summary>
            ///     The flags.
            /// </summary>
            public int flags; // Specifies the cursor state. This parameter can be one of the following values:

            /// <summary>
            ///     The h cursor.
            /// </summary>
            public IntPtr hCursor; // Handle to the cursor. 

            /// <summary>
            ///     The pt screen pos.
            /// </summary>
            public POINT ptScreenPos; // A POINT structure that receives the screen coordinates of the cursor. 
        }

        /// <summary>
        ///     The iconinfo.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ICONINFO
        {
            /// <summary>
            ///     The f icon.
            /// </summary>
            public bool fIcon;

            // Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies 

            /// <summary>
            ///     The x hotspot.
            /// </summary>
            public int xHotspot;

            // Specifies the x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot 

            /// <summary>
            ///     The y hotspot.
            /// </summary>
            public int yHotspot;

            // Specifies the y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot 

            /// <summary>
            ///     The hbm mask.
            /// </summary>
            public IntPtr hbmMask;

            // (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon, 

            /// <summary>
            ///     The hbm color.
            /// </summary>
            public IntPtr hbmColor; // (HBITMAP) Handle to the icon color bitmap. This member can be optional if this 
        }

        /// <summary>
        ///     The point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>
            ///     The x.
            /// </summary>
            public int x;

            /// <summary>
            ///     The y.
            /// </summary>
            public int y;
        }
    }

    /// <summary>
    ///     The gdi stuff.
    /// </summary>

    public class CaptureScreenshot
    {
        static IntPtr hicon = IntPtr.Zero;
        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hCursor;
            public Point ptScreenPos;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);
        private const int CURSOR_SHOWING = 0x00000001;

        /// <summary>
        /// 将鼠标指针形状绘制到屏幕截图上
        /// </summary>
        /// <param name="g"></param>
        /// <param name="sc"></param>
        public static void DrawCursorImageToMultipleScreenImage(ref Graphics g, Screen sc)
        {
            Screen screen = Screen.AllScreens.FirstOrDefault(s => s.Bounds.Contains(Cursor.Position));
            if (screen == null)
            {
                Console.WriteLine("Cannot get the mouse position...");
                return;
            }

            if (screen.Bounds.X == sc.Bounds.X && screen.Bounds.Y == sc.Bounds.Y)
            {
                try
                {
                    int x = 1920;
                    int y = 1200;
                    Bitmap bm = CaptureCursor(ref x, ref y);
                    if (bm != null)
                        g.DrawImage(bm, new Point(x, y));
                    else
                    {
                        CURSORINFO vCursorInfo;
                        vCursorInfo.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
                        GetCursorInfo(out vCursorInfo);
                        if ((vCursorInfo.flags & CURSOR_SHOWING) != CURSOR_SHOWING) return;
                        Cursor vCursor = new Cursor(vCursorInfo.hCursor);

                        Rectangle vRectangle = new Rectangle(new Point(vCursorInfo.ptScreenPos.X - vCursor.HotSpot.X, vCursorInfo.ptScreenPos.Y - vCursor.HotSpot.Y), vCursor.Size);

                        vCursor.Draw(g, vRectangle);
                        vCursor = null;
                    }
                    if (bm != null) bm.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }
        /// <summary>
        /// The capture cursor.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <returns>
        /// The <see cref="Bitmap"/>.
        /// </returns>
        public static Bitmap CaptureCursor(ref int x, ref int y)
        {

            try
            {
                var cursorInfo = new Win32Stuff.CURSORINFO();
                cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
                if (!Win32Stuff.GetCursorInfo(out cursorInfo))
                {
                    Console.WriteLine("I am null 1");
                    IoUtility.WriteText(Path.Combine(ScreenShotErrPath, "ScreenShotError.txt"), TraceLog.GetCurrentTime + "I am null 1");
                    return null;
                }
                if (cursorInfo.hCursor == IntPtr.Zero)
                    return null;
                hicon = Win32Stuff.CopyIcon(cursorInfo.hCursor);
                if (hicon == IntPtr.Zero)
                {
                    Console.WriteLine("I am null 2");
                    cursorInfo.hCursor = IntPtr.Zero;
                    IoUtility.WriteText(Path.Combine(ScreenShotErrPath, "ScreenShotError.txt"), TraceLog.GetCurrentTime + "I am null 2");
                    return null;
                }
                Win32Stuff.ICONINFO iconInfo;
                if (!Win32Stuff.GetIconInfo(hicon, out iconInfo))
                {
                    Console.WriteLine("I am null 3");
                    IoUtility.WriteText(Path.Combine(ScreenShotErrPath, "ScreenShotError.txt"), TraceLog.GetCurrentTime + "I am null 3");
                    return null;
                }
                x = cursorInfo.ptScreenPos.x - ((int)iconInfo.xHotspot);
                y = cursorInfo.ptScreenPos.y - ((int)iconInfo.yHotspot);

                using (Icon icon = Icon.FromHandle(hicon))
                {
                    Win32Stuff.DeleteObject(iconInfo.hbmColor);
                    Win32Stuff.DeleteObject(iconInfo.hbmMask);
                    return icon.ToBitmap();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Win32Stuff.DestroyIcon(hicon);
            }
            return null;
        }
        /// <summary>
        /// 将鼠标指针形状绘制到屏幕截图上
        /// </summary>
        /// <param name="g"></param>
        public static void DrawCursorImageToScreenImage(ref Graphics g)
        {
            int x = 1920;
            int y = 1200;
            Bitmap bmp = CaptureCursor(ref x, ref y);
            g.DrawImage(bmp, new Point(x, y));
            StreamWriter sw = new StreamWriter(@"C:\ConsoleOutput.txt");
            Console.SetOut(sw);

            Console.WriteLine("The x {0} y {1}", x, y);
            sw.Flush();
            sw.Close();


        }
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
           UIntPtr dwExtraInfo);//该函数合成一次击键事件  


        const int KEYEVENTF_KEYUP = 0x2;//若指定该值，该键将被释放；若未指定该值，该键将被按下  

        public static void keydown(Keys k)
        {//按下  
            keybd_event((byte)k, 0, 0, UIntPtr.Zero);
        }

        public static void keyup(Keys k)
        {//释放  
            keybd_event((byte)k, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public static void PrintScreen()
        {//模拟PrintScreen  
            keydown(Keys.PrintScreen);
            Application.DoEvents();
            keyup(Keys.PrintScreen);
            Application.DoEvents();
        }

        public static void AltPrintScreen()
        {//模拟Alt+PrintScreen  
            keydown(Keys.Alt);
            keydown(Keys.PrintScreen);
            Application.DoEvents();
            keyup(Keys.PrintScreen);
            keyup(Keys.Alt);
            Application.DoEvents();

        }

        /// <summary>
        /// 从剪贴板获取图片
        /// </summary>
        /// <returns></returns>
        [STAThread]
        public static Image GetScreenImage()
        {
            Image img = null;

            try
            {

                var newobject = Clipboard.GetDataObject();

                if (newobject != null && newobject.GetDataPresent(DataFormats.Bitmap))
                {
                    img = (Bitmap)newobject.GetData(DataFormats.Bitmap);
                }

                return img;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void CaptureFailedScreen(string testCaseName)
        {
            // Save the screenshot to the specified path that the user has chosen.
            try
            {
                if (!Directory.Exists(ScreenShotErrPath))
                    Directory.CreateDirectory(ScreenShotErrPath);
                string path = Path.Combine(ScreenShotErrPath, testCaseName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                AltPrintScreen();

                //创建一个和屏幕一样大的
                Image myImage = GetScreenImage();
                if (myImage != null)
                {
                    Graphics g = Graphics.FromImage(myImage);


                    DrawCursorImageToScreenImage(ref g);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(90, Color.Gray)), Screen.PrimaryScreen.Bounds);
                    myImage.Save(path + "\\" + TraceLog.GetCurrentTime + "_FailedScreenshot.png");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public static void CaptureScreenStart(string screenPath)
        {
            ScreenSavePath = screenPath;
            _mThreadTrace = new Thread(CaptureScreenProcesses);
            _mThreadTrace.IsBackground = true;
            _mThreadTrace.SetApartmentState(ApartmentState.STA);
            _mThreadTrace.Start();
        }

        public static void CaptureScreenEnd()
        {
            lock (iLock)
            {
                if (_mThreadTrace.IsAlive)
                    _mThreadTrace.Abort();

            }
        }
        public static void CaptureScreenProcesses()
        {
            //Write the file header
            while (true)
            {
                try
                {
                    CaptureScreen(Quality);

                    Thread.Sleep(CaptureScreenshotFrequency * 1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(DateTime.Now.ToString() + ": " + e.ToString());
                    continue;
                }
            }//while(true)
        }

        /// <summary>
        /// Capture a screenshot and save to specified path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pictureName"></param>
        public void CaptureScreenToDestination(string filePath, string pictureName)
        {
            // Save the screenshot to the specified path that the user has chosen.
            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                // printScreen();

                //创建一个和屏幕一样大的
                Image myImage = GetScreenImage();
                if (myImage != null)
                {
                    Graphics g = Graphics.FromImage(myImage);
                    g.CopyFromScreen(0, 0, 0, 0, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
                    DrawCursorImageToScreenImage(ref g);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(90, Color.Gray)), Screen.PrimaryScreen.Bounds);
                    myImage.Save(filePath + "\\" + TraceLog.GetCurrentTime + pictureName);
                    myImage.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        //参数：
        //  string dir 指定的文件夹
        //  string ext 文件类型的扩展名，如".txt" , “.exe"

        private static int GetFileCount(string dir, string ext)
        {
            int count = 0;
            DirectoryInfo d = new DirectoryInfo(dir);
            foreach (FileInfo fi in d.GetFiles())
            {
                if (fi.Extension.ToUpper() == ext.ToUpper())
                {
                    count++;
                }
            }
            return count;
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }
        public static string RemoveNotNumber(string key)
        {
            key = new string((from c in key

                              where char.IsWhiteSpace(c) || char.IsLetterOrDigit(c)

                              select c

                ).ToArray());
            return key;
        }
        /// <summary>
        /// 合并图片
        /// </summary>
        /// <param name="firstImage">第一张图片</param>
        /// <param name="lastImage">第二张图片</param>
        /// <returns></returns>
        public static Image MergerImage(Image firstImage, Image lastImage)
        {
            int width = firstImage.Width + lastImage.Width;
            int height = firstImage.Height > lastImage.Height ? firstImage.Height : lastImage.Height;
            ////创建要显示的图片对象,根据参数的个数设置宽度
            Bitmap backgroudImg = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(backgroudImg);
            ////清除画布,背景设置为白色
            g.Clear(System.Drawing.Color.White);
            g.DrawImageUnscaled(firstImage, 0, 0);
            g.DrawImageUnscaled(lastImage, firstImage.Width, 0);
            g.Dispose();
            return backgroudImg;
        }

        public static Image GetScreenImage(Screen sc)
        {
            try
            {
                Image img = null;
                Graphics gc = null;
                string strSplit1 = RemoveNotNumber(sc.DeviceName);
                //printScreen();
                int iWidth = sc.Bounds.Width;
                //屏幕高
                int iHeight = sc.Bounds.Height;
                //按照屏幕宽高创建位图
                img = new Bitmap(iWidth, iHeight);
                //从一个继承自Image类的对象中创建Graphics对象
                gc = Graphics.FromImage(img);
                //抓屏并拷贝到myimage里
                gc.CopyFromScreen(new Point(sc.Bounds.X, sc.Bounds.Y), new Point(0, 0), new Size(iWidth, iHeight));
                //创建一个和屏幕一样大的
                DrawCursorImageToMultipleScreenImage(ref gc, sc);
                gc.FillRectangle(new SolidBrush(Color.FromArgb(90, Color.Gray)), sc.Bounds);
                return img;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        [STAThread]
        public static void CaptureScreen(int quality)
        {
            EncoderParameter qualityParam = null;
            ImageCodecInfo jpegCodec = null;
            EncoderParameters encoderParams = null;
            Image img = null;
            // Save the screenshot to the specified path that the user has chosen.
            Dictionary<string, Screen> screens = new Dictionary<string, Screen> { };
            try
            {
                if (quality < 0 || quality > 100)
                    throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");

                // Encoder parameter for image quality 
                qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                // JPEG image codec 
                jpegCodec = GetEncoderInfo("image/jpeg");
                encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;
                Screen[] allscreen = Screen.AllScreens;
                int screencount = allscreen.Length;

                if (!Directory.Exists(ScreenSavePath))
                {
                    Directory.CreateDirectory(ScreenSavePath);
                }

                foreach (Screen sc in allscreen)
                {
                    screens.Add(RemoveNotNumber(sc.DeviceName), sc);
                }
                Dictionary<string, Screen> dic1Asc = screens.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

                if (GetFileCount(ScreenSavePath, ".jpg") <= 100)
                {
                    if (screencount == 1)
                    {
                        img = GetScreenImage(allscreen[0]);
                        if (!Directory.Exists(ScreenSavePath))
                            Directory.CreateDirectory(ScreenSavePath);
                        if (Directory.Exists(ScreenSavePath))
                            img.Save(Path.Combine(ScreenSavePath, TraceLog.GetCurrentTime + "screenshot.jpg"), jpegCodec,
                                encoderParams);
                    }
                    else
                    {
                        string[] keys = dic1Asc.Keys.ToArray();
                        Screen[] scrs = dic1Asc.Values.ToArray();

                        Image temp = MergerImage(GetScreenImage(scrs[0]), GetScreenImage(scrs[1]));
                        for (int i = 2; i < screencount; i++)
                        {
                            temp = MergerImage(temp, GetScreenImage(scrs[i]));
                        }
                        if (!Directory.Exists(ScreenSavePath))
                            Directory.CreateDirectory(ScreenSavePath);
                        if (Directory.Exists(ScreenSavePath))
                            temp.Save(Path.Combine(ScreenSavePath, TraceLog.GetCurrentTime + "screenshot.jpg"),
                                jpegCodec, encoderParams);
                    }
                }
                else
                {
                    ScreenSavePath = Path.Combine(CaptureScreenshot.ScreenShotPath + TraceLog.GetCurrentTime);
                }

            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
            qualityParam = null;
            jpegCodec = null;
            encoderParams = null;
            img = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private Program ps = new Program();
        public static string ScreenShotErrPath = string.Format("{0}:\\TestReport\\Errors\\", Program.GetVolume());
        public static string ScreenShotPath = string.Format("{0}:\\TestReport\\ScreenShot", Program.GetVolume());
        public static string ScreenSavePath = String.Empty;
        private static Thread _mThreadTrace;
        private static object iLock = new object();
        public static int Quality = 50;
        public static int CaptureScreenshotFrequency = 1;
    }
}


