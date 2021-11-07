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
        /// <param name="channel"></param>
        /// <returns></returns>
        private int GetVoltage(int channel)
        {
            String cmd = String.Format("~V{0}#", channel);
            sendCommand2(cmd);

            if (commResponse.Length < 5) return (-99999);
            int start = commResponse.IndexOf("~");
            int end = commResponse.IndexOf("#");
            if (start == -1) return (-88888);
            if (end == -1) return (-77777);
            String snum = commResponse.Substring(start + 1, end - start - 1);
            return (Convert.ToInt32(snum));
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
            sendCommand2(cmd);
            if (loguj) lbCommLog.Items.Add(String.Format("SendSetpoint: Value={0}", value));
        }
        /// <summary>
        /// 
        /// </summary>
        private void CalibrateCurrent()
        {
            int value = 0;
            for(int i=0; i<10; i++)
            {
                value += GetVoltage(0);
                Thread.Sleep(1000);
            }
            CurrentZeroOffset = value / 10;
            UpdateCfgData();
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
                res += GetVoltage(0);
                Thread.Sleep(period);
            }
            if (loguj) lbCommLog.Items.Add(String.Format("GetAvgCurrent:  #={0}  PER={1}  Value={2}", samples, period, res / samples));
            return (res / samples);
        }
        private void DisableChannel(int chan, bool state)
        {
            String cmd = String.Format("~D{0}{1}#", chan, state ? 1 : 0);
            sendCommand2(cmd);
        }
    }
}
