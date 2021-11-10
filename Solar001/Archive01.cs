using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Solar001
{


    public partial class frmMain : Form
    {
        int LastHour = -1;
        int LastRecMinute = 0;

        /// <summary>
        /// 
        /// </summary>
        private void ArcChan0Per1()
        {
            DateTime ted = DateTime.Now;
            int hour = ted.Hour;
            if(hour != LastHour) {
                LastHour = hour;
                int res = CalibrateCurrent();
                LogujDisk(String.Format("Current calibrated - Offset={0}", res));
            }
            
            int minute = ted.Minute;
            if(minute != LastRecMinute)
            {
                LastRecMinute = minute;
                DoArchiving001();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void DoArchiving001()
        {
            VoltageResult res = GetLoadCharsInt(0, 12, 50, 3, false);
            DateTime ted = DateTime.Now;
            String sFileName = String.Format("E:\\Tests\\Solar01\\{0:D4}_{1:D2}_{2:D2}.txt", ted.Year, ted.Month, ted.Day);
            String sData = String.Format("{0:D2}:{1:D2}:00; {2:F2}; {3:F}; {4:F3}; {5}; {6:F2}",
                                     ted.Hour, ted.Minute, res.Uopen, res.U12Volt, res.I12Volts, res.NoTries, res.Duration);
            StreamWriter sw = null;
            try
            {
                FileStream fs = File.Open(sFileName, FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs, System.Text.Encoding.ASCII);
                sw.WriteLine(sData);
                sw.Close();
            }
            catch(Exception ex) {
                LogujDisk(ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void LogujDisk(String msg)
        {
            DateTime ted = DateTime.Now;
            String sTimestamp = String.Format("{0:D4}{1:D2}{2:D2}T{3:D2}:{4:D2}:{5:D2}",
                                            ted.Year, ted.Month, ted.Day, ted.Hour, ted.Minute, ted.Second);
            String sData = String.Format("{0}: {1}", sTimestamp, msg);
            StreamWriter sw = null;
            try
            {
                FileStream fs = File.Open("E:\\Tests\\Solar01\\solar.log", FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs, System.Text.Encoding.ASCII);
                sw.WriteLine(sData);
                sw.Close();
            }
            catch (Exception ex)
            {
                lbMainLog.Items.Add(ex.Message);
            }
        }
    }
}/// <summary>
