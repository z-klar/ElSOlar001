using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solar001
{
    class OperatioState1
    {
        public int Id;
        public double Voltage;
        public double RelDiff;
        public double Delta;
        public int Step;
        public int SetPoint;

        public OperatioState1()
        {
            this.Delta = 0;
            this.Id = 0;
            this.RelDiff = 0;
            this.SetPoint = 0;
            this.Step = 0;
            this.Voltage = 0;
        }

        public OperatioState1(int id, double v, double r, double d, int step, int sp)
        {
            this.Id = id;
            this.Voltage = v;
            this.RelDiff = r;
            this.Delta = d;
            this.Step = step;
            this.SetPoint = sp;
        }
    }
}
