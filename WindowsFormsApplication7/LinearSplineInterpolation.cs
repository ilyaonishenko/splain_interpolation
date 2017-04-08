using System;
using System.Collections.Generic;

namespace WindowsFormsApplication7
{
    public class LinearSplineInterpolation :
        IInterpolationMethod
    {
        SplineInterpolation _spline;

        public LinearSplineInterpolation()
        {
            _spline = new SplineInterpolation();
        }

        public bool SupportsDifferentiation
        {
            get { return _spline.SupportsDifferentiation; }
        }

        public bool SupportsIntegration
        {
            get { return _spline.SupportsIntegration; }
        }

        public void Init( IList<double> t, IList<double> x)
        {
            if(null == t)
            {
                throw new ArgumentNullException("t");
            }

            if(null == x)
            {
                throw new ArgumentNullException("x");
            }

            if(t.Count < 2)
            {
                throw new ArgumentOutOfRangeException("t");
            }

            if(t.Count != x.Count)
            {
                throw new ArgumentException("ArgumentVectorsSameLengths");
            }

            double[] c = new double[4 * (t.Count - 1)];
            double[] tt = new double[t.Count];
            t.CopyTo(tt, 0);
            double[] xx = new double[x.Count];
            x.CopyTo(xx, 0);

            Sorting.Sort(tt, xx);

            for(int i = 0, j = 0; i < tt.Length - 1; i++, j += 4)
            {
                c[j] = xx[i];
                c[j + 1] = (xx[i + 1] - xx[i]) / (tt[i + 1] - tt[i]);
                c[j + 2] = 0;
                c[j + 3] = 0;
            }

            _spline.Init(tt, c);
        }

        public double Interpolate(double t)
        {
            return _spline.Interpolate(t);
        }

        public double Differentiate(double t, out double first, out double second)
        {
            return _spline.Differentiate(t, out first, out second);
        }

        public double Integrate( double t)
        {
            return _spline.Integrate(t);
        }
    }
}
