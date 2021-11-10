using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        /// <summary>
        /// 
        /// </summary>
        private void mainTimerHandler()
        {
            if(receptionEnded)
            {
                lbMainLog.Items.Add("RX: [" + commResponse + "]");
                receptionEnded = false;
            }
            TimerCounter1++;
            if(TimerCounter1 >= 10)
            {
                TimerCounter1 = 0;
                if (chkAutoVoltage.Checked) UpdateVoltage();
                if (chkAutoCurrent.Checked) UpdateCurrent();
                if (chkAutoCurrentAvg.Checked) UpdateCurrentAvg();
                if (chkRunArc001.Checked) ArcChan0Per1();
            }
        } 
        /// <summary>
        /// 
        /// </summary>
        private void UpdateVoltage()
        {
            int v = GetRawVoltage(1, false);
            txCh0VoltageRaw.Text = String.Format("{0:D5}", v);
            double volt = v / VoltageConversionCoeff[0];
            txCh0VoltageReal.Text = String.Format("{0:F2}", volt);

            v = GetRawVoltage(2, false);
            txCh1VoltageRaw.Text = String.Format("{0:D5}", v);
            volt = v / VoltageConversionCoeff[0];
            txCh1VoltageReal.Text = String.Format("{0:F2}", volt);
        }
        /// <summary>
        /// 
        /// </summary>
        private void UpdateCurrent()
        {
            int v = GetRawVoltage(0, false);
            txCurrentRaw.Text = String.Format("{0:D5}", v);
            double milliVolts = (v - CurrentZeroOffset) * CurrentChanVoltageRatio;
            double amps = milliVolts / CurrentChanAmpereRatio;
            amps = Math.Abs(amps);
            txCurrentReal.Text = String.Format("{0:F3}", amps);
        }
        /// <summary>
        /// 
        /// </summary>
        private void UpdateCurrentAvg()
        {
            int samples, period;
            try { samples = Convert.ToInt32(txCurrentAvgPeriod.Text); }
            catch (Exception ex)
            {
                MessageBox.Show("Wrong SAMPLES format !");
                return;
            }
            try { period = Convert.ToInt32(txCurrentAvgPeriod.Text); }
            catch (Exception ex)
            {
                MessageBox.Show("Wrong PERIOD format !");
                return;
            }
            int v = GetAverageCurrent(samples, period, false);
            txCurrentRaw.Text = String.Format("{0:D5}", v);
            double milliVolts = (v - CurrentZeroOffset) * CurrentChanVoltageRatio;
            double amps = milliVolts / CurrentChanAmpereRatio;
            amps = Math.Abs(amps);
            txCurrentReal.Text = String.Format("{0:F3}", amps);
        }
    }
}
