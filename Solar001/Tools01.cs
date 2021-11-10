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
        private void UpdateSerialPorts()
        {
            string[] ports = SerialPort.GetPortNames();

            cbSerialPort.Items.Clear();
            foreach (string port in ports) cbSerialPort.Items.Add(port);
            if (cbSerialPort.Items.Count > 0) cbSerialPort.SelectedIndex = 0;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel">Number of required ADC channel. Physical meaning:
        /// 0 = Current sensor
        /// 1 = Voltage CHAN0
        /// 2 = Voltage CHAN2</param>
        /// <returns></returns>
        private int GetRawVoltage(int channel, bool loguj)
        {
            String cmd = String.Format("~V{0}#", channel);
            sendCommand2(cmd, loguj);

            if (commResponse.Length < 5) return (-99999);
            int start = commResponse.IndexOf("~");
            int end = commResponse.IndexOf("#");
            if (start == -1) return (-88888);
            if (end == -1) return (-77777);
            String snum = commResponse.Substring(start + 1, end - start - 1);
            return (Convert.ToInt32(snum));
        }
        /// <summary>
        /// Return converted voltage value. Parameter CHANNEL MUST be 0 or 1 !!!!
        /// </summary>
        /// <param name="channel">Number of INPUT channel (0 or 1)</param>
        /// <param name="loguj"></param>
        /// <returns></returns>
        private double GetRealVoltage(int channel, bool loguj)
        {
            double real;
            int raw = GetRawVoltage(channel + 1, loguj);
            if (raw >= -1000) real = raw / VoltageConversionCoeff[channel];
            else real = (double)raw;
            return (real);
        }
        /// <summary>
        /// 
        /// </summary>
        private void SendSetpoint()
        {
            String txValue = txSetpoint.Text;
            int value;
            try
            {
                value = Convert.ToInt32(txValue);
            }
            catch(Exception ex)
            {
                lbMainLog.Items.Add(ex.Message);
                MessageBox.Show("Invalid INT value !");
                return;
            }
            SendSetpointInt(value, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void SendSetpointInt(int value, bool loguj)
        {
            String cmd = String.Format("~S0{0:D4}#", value);
            sendCommand2(cmd, loguj);
            if (loguj) lbCommLog.Items.Add(String.Format("SendSetpoint: Value={0}", value));
        }
        /// <summary>
        /// 
        /// </summary>
        private int CalibrateCurrent()
        {
            int value = 0;
            for(int i=0; i<10; i++)
            {
                value += GetRawVoltage(0, true);
                Thread.Sleep(1000);
            }
            CurrentZeroOffset = value / 10;
            UpdateCfgData();
            return (CurrentZeroOffset);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        private int GetAverageCurrent(int samples, int period, bool loguj)
        {
            int res = 0;
            for(int i=0; i<samples; i++)
            {
                res += GetRawVoltage(0, loguj);
                Thread.Sleep(period);
            }
            if (loguj) lbCommLog.Items.Add(String.Format("GetAvgCurrent:  #={0}  PER={1}  Value={2}", samples, period, res / samples));
            return (res / samples);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="period"></param>
        /// <param name="loguj"></param>
        /// <returns></returns>
        private double GetAverageRealCurrent(int samples, int period, bool loguj)
        {
            int raw = GetAverageCurrent(samples, period, loguj);
            double milliVolts = (raw - CurrentZeroOffset) * CurrentChanVoltageRatio;
            double amps = milliVolts / CurrentChanAmpereRatio;
            return(Math.Abs(amps));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chan"></param>
        /// <param name="state"></param>
        private void DisableChannel(int chan, bool state)
        {
            String cmd = String.Format("~D{0}{1}#", chan, state ? 1 : 0);
            sendCommand2(cmd, true);
        }
    }
}
