using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.IO;
using System.Data;

namespace CaptureSceenShot
{
    public static class TraceLog
    {
        #region Member variables
        private static int m_Period = 0;
        private static string m_LogFilePath = string.Empty;
        private static string _LogFolderPath = string.Empty;
        private const string _Wql = "SELECT CommandLine,Name,ProcessId FROM Win32_Process where Name like \"%Mcsf%\" or Name like \"%QTAgent%\"";  //
        private static DataTable m_TabProcess = new DataTable();
        private static long m_HeadLen = 0;
        private static Thread m_ThreadTrace;
        private static object iLock = new object();
        private static int _FileBlockSize = 6 * 1024 * 1024;
        private const int m_MUTEX_TIMEOUT = 10 * 1000;

        #region For function trance
        private static string m_sNewColumnHeader = string.Empty;
        private static PerformanceCounter m_CpuCounter;
        private static FileStream m_fs;
        #endregion
        #endregion

        #region Exposed function
        public static string LogPath
        {
            get
            {
                return _LogFolderPath;
            }
        }

        public static void LogBegin(string pLogPath, string pCaseName, int pPeriod)
        {
            _LogFolderPath = pLogPath;
            m_LogFilePath = pLogPath + pCaseName + ".log";
            m_Period = pPeriod;
            m_TabProcess.Reset();
            m_TabProcess.Columns.Add("TimeStamp", Type.GetType("System.String"));
            m_TabProcess.Columns.Add("Log", Type.GetType("System.String"));
            m_TabProcess.Columns.Add("CPU_Load", Type.GetType("System.String"));

            //Shared members should be initialized in the main thread
            m_CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            m_CpuCounter.NextValue();

            m_fs = new FileStream(@m_LogFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            m_ThreadTrace = new Thread(GetRunningProcesses);
            m_ThreadTrace.IsBackground = true;
            m_ThreadTrace.SetApartmentState(ApartmentState.MTA);
            m_ThreadTrace.Start();
        }

        public static void LogInfo(string pLogContent)
        {
            if (string.IsNullOrEmpty(pLogContent))
            {
                return;
            }
            LogSingleRow(pLogContent.Replace(",", "，"), true);
            m_fs.Flush(true);
        }

        public static void LogEnd()
        {
            lock (iLock)
            {
                m_ThreadTrace.Abort();

                if (m_fs != null)
                {
                    m_fs.Flush();
                    m_fs.Close();
                    m_fs = null;
                }
            }
        }

        #endregion

        public static void GetRunningProcesses()
        {
            //Write the file header
            foreach (DataColumn dc in m_TabProcess.Columns)
            {
                m_fs.Write(Encoding.Default.GetBytes(dc.ColumnName + ","), 0, dc.ColumnName.ToString().Length + 1);
            }
            m_fs.Write(System.Text.Encoding.Default.GetBytes("\r\n"), 0, 2);
            m_HeadLen = m_fs.Position - 2;

            while (true)
            {
                try
                {
                    LogSingleRow(string.Empty);
                    Thread.Sleep(m_Period);
                }
                catch (Exception e)
                {
                    Console.WriteLine(DateTime.Now.ToString() + ": " + e.ToString());
                    continue;
                }
            }//while(true)
        }
        [STAThread]
        public static void LogSingleRow(string pContent, bool forceFlush = false)
        {
            lock (iLock)
            {
                if (m_fs == null)
                    return;

                char[] cmdSpliteChar = new char[] { ' ' };
                DataRow row;

                string[] Temp;
                string sCmdLine;

                //Record the common columns :TimeStamp, Log content, CPU_Load
                row = m_TabProcess.NewRow();
                row["TimeStamp"] = GetCurrentTime;
                row["Log"] = pContent;
                row["CPU_Load"] = Math.Round(m_CpuCounter.NextValue(), 2).ToString();

                using (ManagementObjectSearcher mgrSearcher = new ManagementObjectSearcher(_Wql))
                {
                    using (ManagementObjectCollection runningProcesses = mgrSearcher.Get())
                    {
                        //Start to record the system info of the every zhenghe process
                        foreach (ManagementObject obj in runningProcesses)
                            using (obj)
                            {
                                try
                                {
                                    //Get the commandLine of process
                                    Temp = obj["CommandLine"].ToString().Split(cmdSpliteChar, 2, StringSplitOptions.RemoveEmptyEntries);

                                    if (!obj["Name"].ToString().Contains("Container"))
                                    {
                                        sCmdLine = obj["Name"].ToString();
                                    }
                                    else
                                    {
                                        sCmdLine = Temp[1];
                                    }

                                    if (!m_TabProcess.Columns.Contains(sCmdLine))
                                    {
                                        m_TabProcess.Columns.Add(sCmdLine, Type.GetType("System.String"));
                                        m_TabProcess.Columns.Add(sCmdLine + "[Mem_Private]", Type.GetType("System.String"));
                                        m_TabProcess.Columns.Add(sCmdLine + "[Mem_Virtual]", Type.GetType("System.String"));
                                        m_TabProcess.Columns.Add(sCmdLine + "[Thread_Count]", Type.GetType("System.String"));
                                        m_TabProcess.Columns.Add(sCmdLine + "[Handle_Count]", Type.GetType("System.String"));
                                        m_TabProcess.Columns.Add(sCmdLine + "[CPU_Usage]", Type.GetType("System.String"));
                                        m_sNewColumnHeader += sCmdLine + "," + sCmdLine + "[Mem_Private]," + sCmdLine + "[Mem_Virtual]," + sCmdLine + "[Thread_Count]," + sCmdLine + "[Handle_Count]," + sCmdLine + "[CPU_Usage],";
                                    }

                                    //Get the process by id for getting the CPU and mem info
                                    using (Process p = Process.GetProcessById(Convert.ToInt32(obj["ProcessId"])))
                                    {
                                        row[sCmdLine] = obj["ProcessId"];
                                        row[sCmdLine + "[Mem_Private]"] = p.PrivateMemorySize64.ToString();
                                        row[sCmdLine + "[Mem_Virtual]"] = p.VirtualMemorySize64.ToString();
                                        row[sCmdLine + "[Thread_Count]"] = p.Threads.Count.ToString();
                                        row[sCmdLine + "[Handle_Count]"] = p.HandleCount.ToString();
                                        row[sCmdLine + "[CPU_Usage]"] = Math.Round((p.TotalProcessorTime - p.PrivilegedProcessorTime).TotalMilliseconds / 1000 / Environment.ProcessorCount * 100, 2).ToString() + "%";
                                    }
                                }//try
                                catch (ThreadAbortException)
                                {
                                    Thread.ResetAbort();
                                }
                                catch
                                {
                                }
                            }
                    }
                }

                m_TabProcess.Rows.Add(row);
                row = null;
                if (m_TabProcess.Rows.Count > 9 || forceFlush)
                {
                    //New process has been created
                    if (!string.IsNullOrEmpty(m_sNewColumnHeader))
                    {
                        int iRestLogLen = (int)(m_fs.Length - m_HeadLen);
                        byte[] RestLog = new byte[iRestLogLen];
                        m_fs.Seek(m_HeadLen, SeekOrigin.Begin);
                        m_fs.Read(RestLog, 0, iRestLogLen);
                        m_fs.Seek(m_HeadLen, SeekOrigin.Begin);
                        string temp = Encoding.Default.GetString(RestLog);
                        m_fs.Write(Encoding.Default.GetBytes(m_sNewColumnHeader), 0, m_sNewColumnHeader.Length);

                        //splite to 6mb for file stream writen
                        string tempBlock = string.Empty;
                        while (!String.IsNullOrEmpty(temp))
                        {
                            if (temp.Length > _FileBlockSize)
                            {
                                tempBlock = temp.Substring(0, _FileBlockSize);
                                temp = temp.Substring(_FileBlockSize, temp.Length - _FileBlockSize);
                                m_fs.Write(Encoding.Default.GetBytes(tempBlock), 0, tempBlock.Length);
                            }
                            else
                            {
                                m_fs.Write(Encoding.Default.GetBytes(temp), 0, temp.Length);
                                temp = string.Empty;
                            }
                        }
                        RestLog = null;
                        tempBlock = string.Empty;
                        temp = string.Empty;
                        m_HeadLen += m_sNewColumnHeader.Length;
                        m_fs.Seek(m_fs.Length, SeekOrigin.Begin);
                    }

                    //Write every log row into file
                    byte[] content;
                    foreach (DataRow dr in m_TabProcess.Rows)
                    {
                        for (int i = 0; i < m_TabProcess.Columns.Count; i++)
                        {
                            content = Encoding.Default.GetBytes(dr.ItemArray[i].ToString() + ",");
                            m_fs.Write(content, 0, content.Length);
                            content = null;
                        }
                        m_fs.Write(Encoding.Default.GetBytes("\r\n"), 0, 2);
                    }
                    m_fs.Flush(true);
                    m_sNewColumnHeader = string.Empty;

                    m_TabProcess.Clear();
                }//if (m_TabProcess.Rows.Count > 9)
                cmdSpliteChar = null;
            }//iLock

            return;
        }
        /// <summary>
        /// 根据获取任务管理器的命令行字符，判断运行的进程中是否存在输入的模块
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static bool IsProcessRun(string appName)
        {
            char[] cmdSpliteChar = new char[] { ' ' };
            string[] Temp;
            string sCmdLine;
            using (ManagementObjectSearcher mgrSearcher = new ManagementObjectSearcher(_Wql))
            {
                using (ManagementObjectCollection runningProcesses = mgrSearcher.Get())
                {
                    foreach (ManagementObject obj in runningProcesses)
                        using (obj)
                        {
                            try
                            {
                                //Get the commandLine of process
                                Temp = obj["CommandLine"].ToString().Split(cmdSpliteChar, 2, StringSplitOptions.RemoveEmptyEntries);

                                if (obj["Name"].ToString().Contains("Container"))
                                {
                                    sCmdLine = Temp[1];
                                    if (sCmdLine.Contains(appName))
                                    {
                                        return true;
                                    }
                                }

                            }//try
                            catch
                            {
                            }
                        }
                }
            }
            return false;
        }

        public static string GetLatestProcessCommunicationProxyName(string appName)
        {
            int index = -1;
            string returnValue = null;
            char[] cmdSpliteChar = new char[] { ' ' };
            string[] Temp;
            string[] cmdSpliterString = new string[] { "\" \"" };
            string sCmdLine;
            using (ManagementObjectSearcher mgrSearcher = new ManagementObjectSearcher(_Wql))
            {
                using (ManagementObjectCollection runningProcesses = mgrSearcher.Get())
                {
                    foreach (ManagementObject obj in runningProcesses)
                        using (obj)
                        {
                            try
                            {
                                //Get the commandLine of process
                                Temp = obj["CommandLine"].ToString().Split(cmdSpliteChar, 2, StringSplitOptions.RemoveEmptyEntries);

                                if (obj["Name"].ToString().Contains("Container"))
                                {
                                    sCmdLine = Temp[1];
                                    if (sCmdLine.Contains(appName))
                                    {
                                        string[] stringTemp = sCmdLine.Split(cmdSpliterString, StringSplitOptions.None);
                                        if (stringTemp != null && stringTemp.Count() >= 2)
                                        {
                                            string comm = stringTemp[1];
                                            int count = comm.Count();
                                            int lastIndex = comm.LastIndexOf("@");
                                            string s = comm.Substring(lastIndex + 1, count - 1 - lastIndex);
                                            int i = Convert.ToInt32(s);
                                            if (i > index)
                                            {
                                                index = i;
                                                returnValue = comm;
                                            }
                                        }
                                    }
                                }

                            }//try
                            catch
                            {
                            }
                        }
                }
            }
            return returnValue;
        }

        public static int GetProcessIDByCommunicationProxy(string comm)
        {
            char[] cmdSpliteChar = new char[] { ' ' };
            string[] Temp;
            string sCmdLine;
            using (ManagementObjectSearcher mgrSearcher = new ManagementObjectSearcher(_Wql))
            {
                using (ManagementObjectCollection runningProcesses = mgrSearcher.Get())
                {
                    foreach (ManagementObject obj in runningProcesses)
                        using (obj)
                        {
                            try
                            {
                                //Get the commandLine of process
                                Temp = obj["CommandLine"].ToString().Split(cmdSpliteChar, 2, StringSplitOptions.RemoveEmptyEntries);

                                if (obj["Name"].ToString().Contains("Container"))
                                {
                                    sCmdLine = Temp[1];
                                    if (sCmdLine.Contains(comm))
                                    {
                                        string pid = obj["ProcessID"].ToString();
                                        return Convert.ToInt32(pid);
                                    }
                                }

                            }//try
                            catch
                            {
                            }
                        }
                }
            }
            return -1;
        }
        public static int GetRunningProcessCountByName(string appName)
        {
            int count = 0;
            char[] cmdSpliteChar = new char[] { ' ' };
            string[] Temp;
            string sCmdLine;
            using (ManagementObjectSearcher mgrSearcher = new ManagementObjectSearcher(_Wql))
            {
                using (ManagementObjectCollection runningProcesses = mgrSearcher.Get())
                {
                    foreach (ManagementObject obj in runningProcesses)
                        using (obj)
                        {
                            try
                            {
                                //Get the commandLine of process
                                Temp = obj["CommandLine"].ToString().Split(cmdSpliteChar, 2, StringSplitOptions.RemoveEmptyEntries);

                                if (obj["Name"].ToString().Contains("Container"))
                                {
                                    sCmdLine = Temp[1];
                                    if (sCmdLine.Contains(appName))
                                    {
                                        count++;
                                    }
                                }

                            }//try
                            catch
                            {
                            }
                        }
                }
            }
            return count;
        }

        public static bool IsZhengheStarted()
        {
            Process[] myProcesses = Process.GetProcesses();
            String[] McsfProcessName = { "McsfContainerBase", "UIH.Mcsf.Core.ContainerBase", "McsfLoggerServer", "UIH.Mcsf.Service.Manager" };

            foreach (Process myProcess in myProcesses)
            {
                foreach (String mpName in McsfProcessName)
                {
                    // Check If Some ZHENGHE Process Contains or Not
                    if (myProcess.ProcessName.Contains(mpName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #region Tool function
        public static string GetCurrentTime
        {
            get
            {
                return DateTime.Now.Year.ToString() + "年" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "月" + DateTime.Now.Day.ToString().PadLeft(2, '0') + "日" + "_" +
                                DateTime.Now.Hour.ToString().PadLeft(2, '0') + "时" + DateTime.Now.Minute.ToString().PadLeft(2, '0') + "分" + DateTime.Now.Second.ToString().PadLeft(2, '0') + "秒" + DateTime.Now.Millisecond.ToString().PadLeft(3, '0') + "毫秒";
            }
        }
        #endregion
    }
}