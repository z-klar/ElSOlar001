using System;
using System.Collections;
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

        String[] ColNames = { "MainTime", "ItemTime", "Voltage", "Current" , "Setpoint", "Iterations", "Step"};
        Type[] types = { typeof(DateTime), typeof(DateTime), typeof(double), typeof(double), typeof(int), typeof(int), typeof(int) };
        DataTable dtProfileItems;
        /// <summary>
        /// 
        /// </summary>
        private void InitProfileTable()
        {
            dtProfileItems = new DataTable();
            DataColumn col;
            int i = 0;

            foreach (String name in ColNames)
            {
                col = new DataColumn();
                col.ColumnName = name;
                col.DataType = types[i++];
                dtProfileItems.Columns.Add(col);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ReadProfile()
        {
            int chan;
            if (cbLoadProfileChan.SelectedIndex == 0) chan = 0;
            else chan = 1;
            ReadProfileInt(chan, false);
        }
        /// <summary>
        /// 
        /// </summary>
        int SetPoint;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chan"></param>
        /// <param name="loguj"></param>
        /// <returns></returns>
        private DataTable ReadProfileInt(int chan, bool loguj)
        {
            int step, LastVolt, ires;
            double  voltage, current;
            DateTime mainTime, itemTime;

            InitProfileTable();
            SetPoint = 1700;
            mainTime = DateTime.Now;
            itemTime = DateTime.Now;
            DisableChannel(chan, false);
            SendSetpointInt(SetPoint, loguj);
            voltage = GetRealVoltage(chan, loguj);
            step = 20;
            dtProfileItems.Rows.Add(mainTime, itemTime, voltage, 0, SetPoint, 0, step);
            dgv1.DataSource = dtProfileItems;
            AdjustDataGrid();
            if (voltage < 13) return(dtProfileItems);

            ires = 0;
            LastVolt = (int)voltage;
            while(LastVolt >= 12)
            {
                ires += IncLoad(chan, LastVolt, step);
                voltage = GetRealVoltage(chan, loguj);
                if((int)voltage != LastVolt)
                {
                    LastVolt = (int)voltage;
                    current = GetAverageRealCurrent(2, 50, loguj);
                    itemTime = DateTime.Now;
                    if (ires < 20) step = ires;
                    dtProfileItems.Rows.Add(mainTime, itemTime, voltage, current, SetPoint, ires, step);
                    ires = 0;
                }
            }
            dgv1.DataSource = dtProfileItems;
            AdjustDataGrid();
            DisableChannel(chan, true);
            // SP < 700 => There is Out voltage !!!!!!!!!!
            SendSetpointInt(800, loguj);
            return (dtProfileItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chan"></param>
        /// <param name="LastVolt"></param>
        private int IncLoad(int chan, int LastVolt, int step)
        {
            double volt;
            int nPoc = 0;
            while(nPoc < 50)
            {
                nPoc++;
                SetPoint += step;
                SendSetpointInt(SetPoint, false);
                volt = GetRealVoltage(chan, false);
                Thread.Sleep(50);
                if ((int)volt != LastVolt) break;
            }
            return (nPoc);
        }

        private void AdjustDataGrid()
        {
            dgv1.Columns[0].DefaultCellStyle.Format = "hh:mm:ss.fff";
            dgv1.Columns[0].Width = 150;
            dgv1.Columns[1].DefaultCellStyle.Format = "hh:mm:ss.fff";
            dgv1.Columns[1].Width = 150;
            dgv1.Columns[2].DefaultCellStyle.Format = "F2";     // Voltage
            dgv1.Columns[3].DefaultCellStyle.Format = "F3";     // Current
            dgv1.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        int LPLastHour = -1, LPLastMinute = -1;
        bool wasRecord = false;
        /// <summary>
        /// 
        /// </summary>
        private void ArcLoadProfile()
        {
            DateTime ted = DateTime.Now;
            int hour = ted.Hour;
            if (hour != LPLastHour)
            {
                LPLastHour = hour;
                int res = CalibrateCurrent();
                LogujDisk(String.Format("Current calibrated - Offset={0}", res));
            }
            
            int minute = ted.Minute;
            if ((minute % 5) == 0)
            {
                if (!wasRecord)
                {
                    wasRecord = true;
                    LPLastMinute = minute;
                    DataTable dtPom = ReadProfileInt(0, false);
                    StoreProfile(dtPom);
                }
            }
            else
            {
                wasRecord = false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtIn"></param>
        private void StoreProfile(DataTable dtIn)
        {
            DateTime ted = DateTime.Now;
            String sData;
            String sFileName = String.Format("E:\\Tests\\Solar01\\PROFILE_{0:D4}_{1:D2}_{2:D2}.txt", ted.Year, ted.Month, ted.Day);
            StreamWriter sw = null;
            try
            {
                FileStream fs = File.Open(sFileName, FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs, System.Text.Encoding.ASCII);

                foreach (DataRow row in dtIn.Rows)
                {
                    sData = String.Format("{0:s}; {1:s}; {2:F2}; {3:F3}; {4}; {5}; {6}",
                                     Convert.ToDateTime(row.ItemArray[0]), Convert.ToDateTime(row.ItemArray[1]),
                                     Convert.ToDouble(row.ItemArray[2]), Convert.ToDouble(row.ItemArray[3]),
                                     Convert.ToInt32(row.ItemArray[4]), Convert.ToInt32(row.ItemArray[5]),
                                     Convert.ToInt32(row.ItemArray[6]));
                    sw.WriteLine(sData);
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                LogujDisk(ex.Message);
            }

        }
    }
}
