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
        private void openSerialPort()
        {

            try
            {
                _serialPort = new SerialPort();
                // Allow the user to set the appropriate properties.
                _serialPort.PortName = cbSerialPort.SelectedItem.ToString();
                _serialPort.BaudRate = 115200;
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;
                _serialPort.Handshake = Handshake.None;
                // Set the read/write timeouts
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;
                _serialPort.Open();
                String msg = String.Format("Serial port [{0}] succesfully opened ...", cbSerialPort.SelectedItem.ToString());
                lbMainLog.Items.Add(msg);
            }
            catch(Exception ex)
            {
                lbMainLog.Items.Add(ex.Message);
                MessageBox.Show("Error - see the logger !");
                _serialPort = null;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        private void closeSerialPort()
        {
            if(_serialPort == null)
            {
                MessageBox.Show("No serial port opened !");
            }
            else
            {
                _serialPort.Close();
                String msg = String.Format("Serial port [{0}] succesfully closed ...", _serialPort.PortName);
                lbMainLog.Items.Add(msg);
                _serialPort = null;
            }
        }

        private void sendCommand2(string cmd, bool loguj)
        {
            if (errOpenedPort()) return;
            byte[] bytes;
            bytes = Encoding.ASCII.GetBytes(cmd);
            _serialPort.Write(bytes, 0, bytes.Length);
            if (loguj)
            {
                string msg = String.Format("TX: {0}", cmd);
                lbCommLog.Items.Add(msg);
            }
            commResponse = "";
            receptionEnded = false;
            WaitForResponse(loguj);
        }

        private void sendCommand(string cmd)
        {
            if (errOpenedPort()) return;
            byte[] bytes;
            bytes = Encoding.ASCII.GetBytes(cmd);
            _serialPort.Write(bytes, 0, bytes.Length);
            string msg = String.Format("TX: {0}", cmd);
            lbCommLog.Items.Add(msg);

            commResponse = "";
            receptionEnded = false;

            ThreadStart entryPoint = new ThreadStart(GetResponse);
            Thread myThread = new Thread(entryPoint);
            myThread.Name = "Receiving EL";
            myThread.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool errOpenedPort()
        {
            if (_serialPort == null)
            {
                lbMainLog.Items.Add("No COMM port opened !");
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void GetResponse()
        {
            byte[] rxBuff = new byte[200];
            String sPom = "";

            Thread.Sleep(500);
            try
            {
                int iNum = _serialPort.Read(rxBuff, 0, 200);
                if (iNum > 0)
                {
                    sPom = Encoding.ASCII.GetString(rxBuff, 0, iNum);
                    String msg = String.Format("RX: {0}", sPom);
                    UpdateLog loguj = new UpdateLog(lbCommLog);
                    loguj.UpdateLB(msg);
                }
                else
                {
                    UpdateLog loguj = new UpdateLog(lbCommLog);
                    loguj.UpdateLB("NO data received !");
                }
                receptionEnded = true;
                commResponse = sPom;
            }
            catch(Exception ex)
            {
                sPom = ex.Message;
                UpdateLog loguj = new UpdateLog(lbCommLog);
                loguj.UpdateLB("Timeout expired - NO data received !");
                receptionEnded = true;
                commResponse = sPom;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void WaitForResponse(bool loguj)
        {
            int round = 20, poc, noBytesReceived = 0; ;
            byte[] rxbuff = new byte[100];
            bool jedem = true;
            String sPom = "";

            while(jedem)
            {
                round--;
                if (round <= 0) break;
                poc = _serialPort.BytesToRead;
                if(poc > 0)
                {
                    _serialPort.Read(rxbuff, noBytesReceived, poc);
                    noBytesReceived += poc;
                    if(rxbuff[noBytesReceived-1] == '#')
                    {
                        jedem = false;
                    }
                }
                Thread.Sleep(50);
            }
            sPom = Encoding.ASCII.GetString(rxbuff, 0, noBytesReceived);
            if (loguj)
            {
                String msg = String.Format("RX: {0}", sPom);
                lbCommLog.Items.Add(msg);
            }
            commResponse = sPom;
            receptionEnded = true;
        }
    }
}
