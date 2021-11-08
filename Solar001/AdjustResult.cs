using System;
using System.Collections.Generic;
using System.Text;

namespace Solar001
{
    class AdjustResult
    {
        public int NoIterations;
        public double Result;

        public AdjustResult(double res, int iter)
        {
            this.Result = res;
            this.NoIterations = iter;
        }
    }
}
