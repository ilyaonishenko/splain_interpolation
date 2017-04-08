using System;
using System.Collections.Generic;

namespace WindowsFormsApplication7
{
    public interface IInterpolationMethod
    {
        double Interpolate( double t);

        bool SupportsDifferentiation
        {
            get;
        }

        double Differentiate(double t, out double first, out double second);

        bool SupportsIntegration
        {
            get;
        }

        double Integrate( double t);
    }
}
