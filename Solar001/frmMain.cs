using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Solar001
{
    public partial class frmMain : Form
    {
        SerialPort _serialPort = null;
        String commResponse = "";
        bool receptionEnded;
        int TimerCounter1 = 0;

        double[] VoltageConversionCoeff = { 1411, 1403 };
        int CurrentZeroOffset = 0;
        double CurrentChanVoltageRatio = 0.03124;  // mV per bit
        double CurrentChanAmpereRatio = 187;       // mv per Amp

        /// <summary>
        /// 
        /// </summary>
        public frmMain()
        {
            InitializeComponent();
            UpdateSerialPorts();
            OurInit();
        }
        /// <summary>
        /// 
        /// </summary>
        private void OurInit()
        {
            cbTests01Channel.SelectedIndex = 0;
            cbTest01ChannelVoltage.SelectedIndex = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateSerialPorts_Click(object sender, EventArgs e)
        {
            UpdateSerialPorts();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openSerialPort();
        }

        private void btnClearMainLog_Click(object sender, EventArgs e)
        {
            lbMainLog.Items.Clear();
        }

        private void btnCloseSerialPort_Click(object sender, EventArgs e)
        {
            closeSerialPort();
        }

        private void btnClearCommLog_Click(object sender, EventArgs e)
        {
            lbCommLog.Items.Clear();
        }

        private void btnTestSendCommand_Click(object sender, EventArgs e)
        {
            sendCommand2(txTestCommand.Text, true);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            mainTimerHandler();
        }

        private void btnClearMainLog2_Click(object sender, EventArgs e)
        {
            lbMainLog.Items.Clear();
        }

        private void btnUpdateVoltageCh0_Click(object sender, EventArgs e)
        {
            int v = GetRawVoltage(1, true);
            txCh0VoltageRaw.Text = String.Format("{0:D5}", v);
            double volt = v / VoltageConversionCoeff[0];
            txCh0VoltageReal.Text = String.Format("{0:F2}", volt);
        }

        private void btnUpdateVoltageCh1_Click(object sender, EventArgs e)
        {
            int v = GetRawVoltage(2, true);
            txCh1VoltageRaw.Text = String.Format("{0:D5}", v);
            double volt = v / VoltageConversionCoeff[1];
            txCh1VoltageReal.Text = String.Format("{0:F2}", volt);
        }

        private void btnUpdateCurrent_Click(object sender, EventArgs e)
        {
            int v = GetRawVoltage(0, true);
            txCurrentRaw.Text = String.Format("{0:D5}", v);
            double milliVolts = (v - CurrentZeroOffset) * CurrentChanVoltageRatio;
            double amps = milliVolts / CurrentChanAmpereRatio;
            amps = Math.Abs(amps);
            txCurrentReal.Text = String.Format("{0:F3}", amps);
        }

        private void btnSendSetpoint_Click(object sender, EventArgs e)
        {
            SendSetpoint();
        }

        private void btnCalibrateCurrent_Click(object sender, EventArgs e)
        {
            CalibrateCurrent();
            UpdateCfgData();
        }

        private void btnUpdateCfgData_Click(object sender, EventArgs e)
        {
            UpdateCfgData();
        }

        private void btnSpPlus1_Click(object sender, EventArgs e)
        {
            int currSp;
            if ((currSp = GetCurrentSp()) < 0) return;
            if ((currSp + 1) <= 4095) currSp += 1;
            txSetpoint.Text = String.Format("{0:D5}", currSp);
            SendSetpoint();
        }

        private void btnSpPlus10_Click(object sender, EventArgs e)
        {
            int currSp;
            if ((currSp = GetCurrentSp()) < 0) return;
            if ((currSp + 10) <= 4095) currSp += 10;
            else currSp = 4095;
            txSetpoint.Text = String.Format("{0:D5}", currSp);
            SendSetpoint();
        }

        private void btnSpPlus100_Click(object sender, EventArgs e)
        {
            int currSp;
            if ((currSp = GetCurrentSp()) < 0) return;
            if ((currSp + 100) <= 4095) currSp += 100;
            else currSp = 4095;
            txSetpoint.Text = String.Format("{0:D5}", currSp);
            SendSetpoint();
        }

        private void btnSpMinus1_Click(object sender, EventArgs e)
        {
            int currSp;
            if ((currSp = GetCurrentSp()) < 0) return;
            if ((currSp - 1) >= 0) currSp -= 1;
            else currSp = 0;
            txSetpoint.Text = String.Format("{0:D5}", currSp);
            SendSetpoint();
        }

        private void btnSpMinus10_Click(object sender, EventArgs e)
        {
            int currSp;
            if ((currSp = GetCurrentSp()) < 0) return;
            if ((currSp - 10) >= 0) currSp -= 10;
            else currSp = 0;
            txSetpoint.Text = String.Format("{0:D5}", currSp);
            SendSetpoint();
        }

        private void btnSpMinus100_Click(object sender, EventArgs e)
        {
            int currSp;
            if ((currSp = GetCurrentSp()) < 0) return;
            if ((currSp - 100) >= 0) currSp -= 100;
            else currSp = 0;
            txSetpoint.Text = String.Format("{0:D5}", currSp);
            SendSetpoint();
        }

        private void btnDisableChan0_Click(object sender, EventArgs e)
        {
            String cmd = String.Format("~D01#");
            pbChan0.BackColor = Color.Red;
            sendCommand2(cmd, true);
        }

        private void btnEnableChan0_Click(object sender, EventArgs e)
        {
            String cmd = String.Format("~D00#");
            pbChan0.BackColor = Color.Lime;
            sendCommand2(cmd, true);
        }

        private void btnDisableChn1_Click(object sender, EventArgs e)
        {
            String cmd = String.Format("~D11#");
            pbChan1.BackColor = Color.Red;
            sendCommand2(cmd, true);
        }

        private void btnEnableChan1_Click(object sender, EventArgs e)
        {
            String cmd = String.Format("~D10#");
            pbChan1.BackColor = Color.Lime;
            sendCommand2(cmd, true);
        }

        private void btnGetAvgCurrent_Click(object sender, EventArgs e)
        {
            int samples, period;
            try { samples = Convert.ToInt32(txCurrentAvgSamples.Text); }
            catch(Exception ex) {
                String sPom = ex.Message;
                MessageBox.Show("Wrong SAMPLES format !");
                return;
            }
            try { period = Convert.ToInt32(txCurrentAvgPeriod.Text); }
            catch (Exception ex)
            {
                MessageBox.Show("Wrong PERIOD format !");
                return;
            }
            int v = GetAverageCurrent(samples, period, true);
            txCurrentRaw.Text = String.Format("{0:D5}", v);
            double milliVolts = (v - CurrentZeroOffset) * CurrentChanVoltageRatio;
            double amps = milliVolts / CurrentChanAmpereRatio;
            amps = Math.Abs(amps);
            txCurrentReal.Text = String.Format("{0:F3}", amps);
        }

        private void chkAutoCurrent_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoCurrent.Checked) chkAutoCurrentAvg.Checked = false;
        }

        private void chkAutoCurrentAvg_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutoCurrentAvg.Checked) chkAutoCurrent.Checked = false;
        }

        private void btnTests01SetCurrent_Click(object sender, EventArgs e)
        {
            SetCurrent();
        }

        private void btnTests01SetVoltage_Click(object sender, EventArgs e)
        {
            GetLoadChars();
        }
    }
}
