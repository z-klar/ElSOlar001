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
            double current, tolerance;
            AdjustResult ares;

            if (cbTests01Channel.SelectedIndex > 0) chan = 1;
            else chan = 0;
            if ((current = CheckDouble(txTests01Current.Text)) < 0)
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
            ares = SetCurrentInt(chan, current, tries, tolerance, chkTests01Loguj.Checked);
            txTests01Result.Text = String.Format("{0:F3}", ares.Result);
            txTests01CurrIterations.Text = String.Format("{0}", ares.NoIterations);
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
            catch (Exception ex)
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
        private AdjustResult SetCurrentInt(int chan, double current, int tries, double tolerance, bool loguj)
        {
            int testno = tries, curr = 0;
            int setpoint = 1700, step;
            double diff, reldiff, calculatedCurrent = 0;

            DisableChannel(chan, false);
            while (testno >= 0)
            {
                SendSetpointInt(setpoint, loguj);
                Thread.Sleep(50);
                curr = GetAverageCurrent(3, 40, loguj);
                double milliVolts = (curr - CurrentZeroOffset) * CurrentChanVoltageRatio;
                double amps = milliVolts / CurrentChanAmpereRatio;
                calculatedCurrent = Math.Abs(amps);

                reldiff = Diff(current, calculatedCurrent);
                if (reldiff <= tolerance) break;
                if (reldiff > 100) step = 100;
                else if (reldiff > 30) step = 20;
                else step = 5;

                testno--;
                diff = calculatedCurrent - current;
                if (diff > 0)
                {
                    setpoint -= step;
                }
                else
                {
                    setpoint += step;
                }
            }
            DisableChannel(chan, true);
            // SP < 700 => There is Out voltage !!!!!!!!!!
            SendSetpointInt(800, loguj);
            return (new AdjustResult(calculatedCurrent, tries - testno));
        }

        private double Diff(double target, double value)
        {
            double adiff = Math.Abs(value - target);
            double diff = adiff / (Math.Abs(value) / 100);
            return (diff);
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetLoadChars()
        {
            int channel, tries;
            double voltage, tolerance;

            if (cbTest01ChannelVoltage.SelectedIndex == 0) channel = 0;
            else channel = 1;
            try { voltage = Convert.ToDouble(txTests01Voltage.Text); }
            catch (Exception ex)
            {
                MessageBox.Show("Wrong Required Voltage Format !");
                return;
            }
            try { tolerance = Convert.ToDouble(txTests01ToleranceVoltage.Text); }
            catch (Exception ex)
            {
                MessageBox.Show("Wrong Tolerance Format !");
                return;
            }
            try { tries = Convert.ToInt32(txTests01TriesVoltage.Text); }
            catch (Exception ex)
            {
                MessageBox.Show("Wrong # Tries Format !");
                return;
            }
            VoltageResult res = GetLoadCharsInt(channel, voltage, tries, tolerance, chkTest01LogujVoltage.Checked);
            txTests01ResultVoltage.Text = String.Format("{0:F2}", res.U12Volt);
            txTests01ResultCurrent.Text = String.Format("{0:F3}", res.I12Volts);
            txTests01VoltageIterations.Text = String.Format("{0}", res.NoTries);
            txTests01VoltageOpen.Text = String.Format("{0:F2}", res.Uopen);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="reqVoltage"></param>
        /// <param name="tries"></param>
        /// <param name="tolerance"></param>
        /// <param name="loguj"></param>
        /// <returns></returns>
        private VoltageResult GetLoadCharsInt(int channel, double reqVoltage, int tries, double tolerance, bool loguj)
        {
            int testno = tries;
            int setpoint = 1700, step;
            double diff, reldiff, reldiffold, delta, voltage = 0;
            VoltageResult res = new VoltageResult();
            String sPom;

            DisableChannel(channel, false);
            res.Uopen = GetRealVoltage(channel, loguj);
            if (res.Uopen < reqVoltage) return (res);
            reldiffold = Diff(reqVoltage, res.Uopen);
            while (testno >= 0)
            {
                SendSetpointInt(setpoint, loguj);
                Thread.Sleep(50);
                voltage = GetRealVoltage(channel, loguj);
                reldiff = Diff(reqVoltage, voltage);
                if (reldiff <= tolerance)
                {
                    res.U12Volt = voltage;
                    res.I12Volts = GetAverageRealCurrent(3, 50, loguj);
                    res.NoTries = tries - testno;
                    DisableChannel(channel, true);
                    // SP < 700 => There is Out voltage !!!!!!!!!!
                    SendSetpointInt(800, loguj);
                    return (res); ;
                }
                delta = Math.Abs(reldiff - reldiffold);
                reldiffold = reldiff;
                if (delta < 0.3) step = 100;
                else if (delta < 1) step = 24;
                else if (delta < 3) step = 10;
                else if (delta < 5) step = 5;
                else step = 2;

                testno--;
                diff = voltage - reqVoltage;
                if (diff > 0) setpoint += step;
                else  setpoint -= step;
                sPom = String.Format("#{0}: Volt={1:F2},  reldiff={2:F2},  delta={3:F2}  step={4} => SP={5}", 
                                      testno,   voltage,       reldiff,        delta,     step,     setpoint);
                lbMainLog.Items.Add(sPom);
            }
            res.U12Volt = voltage;
            res.I12Volts = GetAverageRealCurrent(3, 50, loguj);
            res.NoTries = -9999;
            DisableChannel(channel, true);
            // SP < 700 => There is Out voltage !!!!!!!!!!
            SendSetpointInt(800, loguj);
            return (res); ;
        }
    }
}
