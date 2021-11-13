using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solar001
{
    class ProfileItem
    {
        public DateTime MainTimestamp;
        public DateTime ItemTimestamp;
        public double Voltage;
        public double Current;
        public int Setpoint;


        public ProfileItem(DateTime main, DateTime item, 
                           double volt, double amp, int sp)
        {
            this.MainTimestamp = main;
            this.ItemTimestamp = item;
            this.Voltage = volt;
            this.Current = amp;
            this.Setpoint = sp;
        }
    }
}
