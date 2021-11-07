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
        private void SetCurrent()
        {
            int chan, tries;
            double current, tolerance, res;

            if (cbTests01Channel.SelectedIndex > 0) chan = 1;
            else chan = 0;
            if((current = CheckDouble(txTests01Current.Text)) < 0)
            {
                MessageBox.Show("Wrong current format !");
                return;
            }
            if ((tries = CheckInt(txTests01Tries.Text)) < 0)
            {
                MessageBox.Show("Wrong # Tries format !");
                return;
            }
            if ((tolerance = CheckDouble(txTests01Tolerance.Text)) < 0)
            {
                MessageBox.Show("Wrong tolerance format !");
                return;
            }
            res = SetCurrentInt(chan, current, tries, tolerance);
            txTests01Result.Text = String.Format("{0:F3}", res);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private double CheckDouble(String text)
        {
            double val;
            try
            {
                val = Convert.ToDouble(text);
                return (val);
            }
            catch(Exception ex)
            {
                return (-1000);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private int CheckInt(String text)
        {
            int val;
            try
            {
                val = Convert.ToInt32(text);
                return (val);
            }
            catch (Exception ex)
            {
                return (-1000);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chan"></param>
        /// <param name="current"></param>
        /// <param name="tries"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private double SetCurrentInt(int chan, double current, int tries, double tolerance)
        {
            int testno = tries, curr = 0;
            int setpoint = 1700;
            double diff, reldiff;

            DisableChannel(chan, false);
            while(testno >= 0)
            {
                SendSetpointInt(setpoint, true);
                Thread.Sleep(50);
                curr = GetAverageCurrent(3, 40, true);
                reldiff = Diff(current, curr);
                if (reldiff <= tolerance) break;

                testno--;
                diff = curr - current;
                if(diff > 0)
                {
                    setpoint -= 20;
                }
                else
                {
                    setpoint += 20;
                }
            }
            return (curr);
        }

        private double Diff(double target, double value)
        {
            double adiff = Math.Abs(value - target);
            double diff = adiff / (Math.Abs(value) / 100);
            return (diff);
        }

    }
}
