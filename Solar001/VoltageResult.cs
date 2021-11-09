using System;
using System.Collections.Generic;
using System.Text;

namespace Solar001
{
    class VoltageResult
    {
        public double Uopen;
        public double U12Volt;
        public double I12Volts;
        public int    NoTries;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Uo"></param>
        /// <param name="U12"></param>
        /// <param name="I"></param>
        /// <param name="tries"></param>
        public VoltageResult(double Uo, double U12, Double I, int tries)
        {
            this.Uopen = Uo;
            this.U12Volt = U12;
            this.I12Volts = I;
            this.NoTries = tries;
        }
        public VoltageResult()
        {
            this.Uopen = 0;
            this.U12Volt = 0;
            this.I12Volts = 0;
            this.NoTries = 0;
        }
    }
}
