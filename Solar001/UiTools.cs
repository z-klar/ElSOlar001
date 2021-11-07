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
        private void UpdateCfgData()
        {
            if (_serialPort == null) txCfgSerialPort.Text = "N/A";
            else txCfgSerialPort.Text = _serialPort.PortName;
            txCfgCurrentZeroOffset.Text = String.Format("{0:D5}", CurrentZeroOffset);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int GetCurrentSp()
        {
            int iSp;
            String txSp = txSetpoint.Text;
            try
            {
                iSp = Convert.ToInt32(txSp);
            }
            catch(Exception ex)
            {
                return (-1);
            }
            return (iSp);
        }
    }
}
