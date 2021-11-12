using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solar001
{
    class OperationState2
    {
        public int Id;
        public double Voltage;
        public double Diff;
        public double Derivative;
        public double Integral;
        public double Output;
        public int Step;
        public int SetPoint;

        public OperationState2(int Id, double volt, double diff, double der,
                                double integ, double output, int step, int sp)
        {
            this.Id = Id;
            this.Voltage = volt;
            this.Diff = diff;
            this.Derivative = der;
            this.Integral = integ;
            this.Output = output;
            this.Step = step;
            this.SetPoint = sp;
        }

    }
}
