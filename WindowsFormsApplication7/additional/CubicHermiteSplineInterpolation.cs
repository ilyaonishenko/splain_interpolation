using System;
using System.Collections.Generic;

namespace WindowsFormsApplication7
{
    public class CubicHermiteSplineInterpolation : IInterpolationMethod
    {
        SplineInterpolation _spline;

        public CubicHermiteSplineInterpolation()
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

        public void Init(IList<double> t, IList<double> x, IList<double> d)
        {
            if(null == t)
            {
                throw new ArgumentNullException("t");
            }

            if(null == x)
            {
                throw new ArgumentNullException("x");
            }

            if(null == d)
            {
                throw new ArgumentNullException("d");
            }

            if(t.Count < 2)
            {
                throw new ArgumentOutOfRangeException("t");
            }

            if(t.Count != x.Count)
            {
                throw new ArgumentException("x ArgumentVectorsSameLengths");
            }

            if(t.Count != d.Count)
            {
                throw new ArgumentException("d ArgumentVectorsSameLengths");
            }

            double[] tt = new double[t.Count];
            t.CopyTo(tt, 0);
            double[] xx = new double[x.Count];
            x.CopyTo(xx, 0);
            double[] dd = new double[d.Count];
            d.CopyTo(dd, 0);

            Sorting.Sort(tt, xx, dd);

            InitInternal(tt, xx, dd);
        }

        internal void InitInternal(double[] t, double[] x, double[] d)
        {
            double[] c = new double[4 * (t.Length - 1)];

            for(int i = 0, j = 0; i < t.Length - 1; i++, j += 4)
            {
                double delta = t[i + 1] - t[i];
                double delta2 = delta * delta;
                double delta3 = delta * delta2;
                c[j] = x[i];
                c[j + 1] = d[i];
                c[j + 2] = (3 * (x[i + 1] - x[i]) - 2 * d[i] * delta - d[i + 1] * delta) / delta2;
                c[j + 3] = (2 * (x[i] - x[i + 1]) + d[i] * delta + d[i + 1] * delta) / delta3;
            }

            _spline.Init(t, c);
        }

        public double Interpolate(double t)
        {
            return _spline.Interpolate(t);
        }

        public double Differentiate(double t, out double first, out double second)
        {
            return _spline.Differentiate(t, out first, out second);
        }

        public double Integrate(double t)
        {
            return _spline.Integrate(t);
        }
    }
}
