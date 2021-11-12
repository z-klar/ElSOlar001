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
                if (diff > 0) setpoint -= step;
                else setpoint += step;
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
            int setpoint = 1700, step, ct = 0;
            double diff, reldiff, reldiffold, delta, voltage = 0;
            VoltageResult res = new VoltageResult();
            String sPom;
            DateTime dtStart = DateTime.Now;
            DateTime dtEnd;
            TimeSpan span;
            ArrayList alStates = new ArrayList();

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
                    dtEnd = DateTime.Now;
                    span = dtEnd.Subtract(dtStart);
                    res.Duration = span.TotalMilliseconds;
                    LogujStates(res, alStates);
                    return (res); ;
                }
                delta = Math.Abs(reldiff - reldiffold);
                reldiffold = reldiff;

                if(delta < 0.3)
                {
                    if (reldiff > 50) step = 100;
                    else if (reldiff > 8) step = 60;
                    else if (reldiff > 4) step = 30;
                    else step = 10;
                }
                else if(delta < 1)
                {
                    if (reldiff > 50) step = 24;
                    else if (reldiff > 8) step = 15;
                    else if (reldiff > 4) step = 8;
                    else step = 4;
                }
                else if(delta < 3)
                {
                    if (reldiff > 50) step = 10;
                    else if (reldiff > 8) step = 6;
                    else if (reldiff > 4) step = 3;
                    else step = 2;
                }
                else if(delta < 5)
                {
                    if (reldiff > 50) step = 5;
                    else if (reldiff > 8) step = 3;
                    else if (reldiff > 4) step = 2;
                    else step = 1;
                }
                else
                {
                    if (reldiff > 50) step = 5;
                    else if (reldiff > 8) step = 2;
                    else if (reldiff > 4) step = 1;
                    else step = 1;
                }


                testno--;
                diff = voltage - reqVoltage;
                if (diff > 0) setpoint += step;
                else  setpoint -= step;
                sPom = String.Format("{0}; {1:F2}; {2:F2}; {3:F2}; {4};  {5}", 
                                      testno,   voltage,       reldiff,        delta,     step,     setpoint);
                if(loguj) lbMainLog.Items.Add(sPom);
                OperatioState1 state = new OperatioState1(ct, voltage, reldiff, delta, step, setpoint);
                alStates.Add(state);
                ct++;
            }
            res.U12Volt = voltage;
            res.I12Volts = GetAverageRealCurrent(3, 50, loguj);
            res.NoTries = 9999;
            DisableChannel(channel, true);
            // SP < 700 => There is Out voltage !!!!!!!!!!
            SendSetpointInt(800, loguj);
            dtEnd = DateTime.Now;
            span = dtEnd.Subtract(dtStart);
            res.Duration = span.TotalMilliseconds;
            LogujStates(res, alStates);
            return (res); ;
        }
        /// <summary>
        /// 
        /// </summary>
        private void LogujStates(VoltageResult res, ArrayList states)
        {
            if (res.NoTries < 15) return;

            DateTime ted = DateTime.Now;
            String sTimestamp = String.Format("{0:D4}{1:D2}{2:D2}T{3:D2}:{4:D2}:{5:D2}:",
                                            ted.Year, ted.Month, ted.Day, ted.Hour, ted.Minute, ted.Second);
            String sFileName = String.Format("E:\\Tests\\Solar01\\states01_{0:D4}{1:D2}{2:D2}.log",
                                                                   ted.Year, ted.Month, ted.Day);
            String sData;
            StreamWriter sw = null;
            try
            {
                FileStream fs = File.Open(sFileName, FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs, System.Text.Encoding.ASCII);
                sw.WriteLine("-----------------------");
                sw.WriteLine(sTimestamp);
                sw.WriteLine("-----------------------");
                foreach(OperatioState1 state in states)
                {
                    sData = String.Format("{0}; {1:F2}; {2:F2}; {3:F2}; {4};  {5}",
                           state.Id, state.Voltage, state.RelDiff, state.Delta, state.Step, state.SetPoint);

                    sw.WriteLine(sData);
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                lbMainLog.Items.Add(ex.Message);
            }

        }
        //******************************************************************************************
        /// <summary>
        /// 
        /// </summary>
        private void GetLoadChars2()
        {
            int chan, tries;
            double voltage, tolerance, KP, KD, KI;
            VoltageResult ares;

            if (cbTests01Channel.SelectedIndex > 0) chan = 1;
            else chan = 0;
            if ((voltage = CheckDouble(txPidVoltage.Text)) < 0)
            {
                MessageBox.Show("Wrong VOLTAGE format !");
                return;
            }
            if ((tries = CheckInt(txPidNotries.Text)) < 0)
            {
                MessageBox.Show("Wrong # Tries format !");
                return;
            }
            if ((tolerance = CheckDouble(txPidTolerance.Text)) < 0)
            {
                MessageBox.Show("Wrong tolerance format !");
                return;
            }
            if ((KP = CheckDouble(txPidKP.Text)) < 0)
            {
                MessageBox.Show("Wrong KP format !");
                return;
            }
            if ((KD = CheckDouble(txPidKD.Text)) < 0)
            {
                MessageBox.Show("Wrong KD format !");
                return;
            }
            if ((KI = CheckDouble(txPidKI.Text)) < 0)
            {
                MessageBox.Show("Wrong KI format !");
                return;
            }
            ares = GetLoadCharsIn2(chan, voltage, tries, tolerance, false, KP, KD, KI);
            txPidResultVolt.Text = String.Format("{0:F3}", ares.U12Volt);
            txPidResultAmp.Text = String.Format("{0:F3}", ares.I12Volts);
            txPidIterations.Text = String.Format("{0}", ares.NoTries);
        }
        double[] LastDiff = { 0, 0, 0, 0, 0 };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="reqVoltage"></param>
        /// <param name="tries"></param>
        /// <param name="tolerance"></param>
        /// <param name="loguj"></param>
        /// <param name="KP"></param>
        /// <param name="KD"></param>
        /// <param name="KI"></param>
        /// <returns></returns>
        private VoltageResult GetLoadCharsIn2(int channel, double reqVoltage, int tries, double tolerance,
                                              bool loguj, double KP, double KD, double KI)
        {
            int testno = tries;
            int setpoint = 1700, step=0, ct = 0;
            double diff,  diffold, delta, voltage = 0;
            double derivative = 0, integral = 0, output=0;
            VoltageResult res = new VoltageResult();
            String sPom;
            DateTime dtStart = DateTime.Now;
            DateTime dtEnd;
            TimeSpan span;
            ArrayList alStates = new ArrayList();
            OperationState2 state;

            DisableChannel(channel, false);
            res.Uopen = GetRealVoltage(channel, loguj);
            if (res.Uopen < reqVoltage) return (res);

            LastDiff[0] = res.Uopen - reqVoltage;

            while (testno >= 0)
            {
                SendSetpointInt(setpoint, loguj);
                Thread.Sleep(50);
                voltage = GetRealVoltage(channel, loguj);
                diff = voltage - reqVoltage;
                if(IsInTolerance(voltage, reqVoltage, tolerance))
                {
                    res.U12Volt = voltage;
                    res.I12Volts = GetAverageRealCurrent(3, 50, loguj);
                    res.NoTries = tries - testno;
                    DisableChannel(channel, true);
                    // SP < 700 => There is Out voltage !!!!!!!!!!
                    SendSetpointInt(800, loguj);
                    dtEnd = DateTime.Now;
                    span = dtEnd.Subtract(dtStart);
                    res.Duration = span.TotalMilliseconds;
                    state = new OperationState2(ct, voltage, diff, derivative, integral, output, step, setpoint);
                    alStates.Add(state);
                    LogujStates2(res, alStates, KP, KD, KI);
                    return (res); ;
                }
                InsertNewDiff(diff, LastDiff);
                derivative = LastDiff[0] - LastDiff[1];
                integral = CalculateIntegral(LastDiff);
                output = KP * diff + KD * derivative + KI * integral;
                step = (int)output;
                setpoint += step;
                testno--;
                state = new OperationState2(ct, voltage, diff, derivative, integral, output, step, setpoint);
                alStates.Add(state);
                ct++;
            }
            res.U12Volt = voltage;
            res.I12Volts = GetAverageRealCurrent(3, 50, loguj);
            res.NoTries = 9999;
            DisableChannel(channel, true);
            // SP < 700 => There is Out voltage !!!!!!!!!!
            SendSetpointInt(800, loguj);
            dtEnd = DateTime.Now;
            span = dtEnd.Subtract(dtStart);
            res.Duration = span.TotalMilliseconds;
            LogujStates2(res, alStates, KP, KD, KI);
            return (res); ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        private bool IsInTolerance(double val1, double val2, double tol)
        {
            double pom = Math.Abs((val1 - val2) / (val2 / 100));
            return (pom <= tol);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="target"></param>
        private void InsertNewDiff(double val, double [] target)
        {
            int n = target.Length;
            for (int i = 1; i < n; i++) target[i] = target[i - 1];
            target[0] = val;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private double CalculateIntegral(double [] values)
        {
            double res = 0;
            for (int i = 0; i < values.Length; i++) res += values[i];
            return (res);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="res"></param>
        /// <param name="states"></param>
        private void LogujStates2(VoltageResult res, ArrayList states, double KP, double KD, double KI)
        {
            //if (res.NoTries < 15) return;
            DateTime ted = DateTime.Now;
            String sTimestamp = String.Format("{0:D4}{1:D2}{2:D2}T{3:D2}:{4:D2}:{5:D2}:   KP={6:F2}, KD={7:F2}, KI={8:F2},",
                                            ted.Year, ted.Month, ted.Day, ted.Hour, ted.Minute, ted.Second, KP, KD, KI);
            String sFileName = String.Format("E:\\Tests\\Solar01\\states02_{0:D4}{1:D2}{2:D2}.log",
                                                                   ted.Year, ted.Month, ted.Day);
            String sData;
            StreamWriter sw = null;
            try
            {
                FileStream fs = File.Open(sFileName, FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs, System.Text.Encoding.ASCII);
                sw.WriteLine("----------------------------------");
                sw.WriteLine(sTimestamp);
                sw.WriteLine("----------------------------------");
                foreach (OperationState2 state in states)
                {
                    sData = String.Format("{0}; {1:F2}; {2:F2}; {3:F2}; {4:F2};  {5:F2}; {6}; {7}",
                           state.Id, state.Voltage, state.Diff, state.Derivative, state.Integral, state.Output, state.Step, state.SetPoint);

                    sw.WriteLine(sData);
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                lbMainLog.Items.Add(ex.Message);
                sw.Close();
            }

        }
    }
}
