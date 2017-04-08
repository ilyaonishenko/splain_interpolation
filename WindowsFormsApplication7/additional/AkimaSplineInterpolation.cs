using System;
using System.Collections.Generic;

namespace WindowsFormsApplication7
{
    public class AkimaSplineInterpolation :
        IInterpolationMethod
    {
        CubicHermiteSplineInterpolation _hermiteSpline;

        public
        AkimaSplineInterpolation()
        {
            _hermiteSpline = new CubicHermiteSplineInterpolation();
        }

        public bool SupportsDifferentiation
        {
            get { return _hermiteSpline.SupportsDifferentiation; }
        }

        public bool SupportsIntegration
        {
            get { return _hermiteSpline.SupportsIntegration; }
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

            if(t.Count < 5)
            {
                throw new ArgumentOutOfRangeException("t");
            }

            if(t.Count != x.Count)
            {
                throw new ArgumentException("x ArgumentVectorsSameLengths");
            }

            int n = t.Count;

            double[] tt = new double[n];
            t.CopyTo(tt, 0);
            double[] xx = new double[n];
            x.CopyTo(xx, 0);

            Sorting.Sort(tt, xx);

            // Prepare W (weights), Diff (divided differences)

            double[] w = new double[n - 1];
            double[] diff = new double[n - 1];

            for(int i = 0; i < diff.Length; i++)
            {
                diff[i] = (xx[i + 1] - xx[i]) / (tt[i + 1] - tt[i]);
            }

            for(int i = 1; i < w.Length; i++)
            {
                w[i] = Math.Abs(diff[i] - diff[i - 1]);
            }

            // Prepare Hermite interpolation scheme

            double[] d = new double[n];

            for(int i = 2; i < d.Length - 2; i++)
            {
                if(!Number.AlmostZero(w[i - 1]) || !Number.AlmostZero(w[i + 1]))
                {
                    d[i] = (w[i + 1] * diff[i - 1] + w[i - 1] * diff[i]) / (w[i + 1] + w[i - 1]);
                }
                else
                {
                    d[i] = ((tt[i + 1] - tt[i]) * diff[i - 1] + (tt[i] - tt[i - 1]) * diff[i]) / (tt[i + 1] - tt[i - 1]);
                }
            }

            d[0] = DifferentiateThreePoint(tt[0], tt[0], xx[0], tt[1], xx[1], tt[2], xx[2]);
            d[1] = DifferentiateThreePoint(tt[1], tt[0], xx[0], tt[1], xx[1], tt[2], xx[2]);
            d[n - 2] = DifferentiateThreePoint(tt[n - 2], tt[n - 3], xx[n - 3], tt[n - 2], xx[n - 2], tt[n - 1], xx[n - 1]);
            d[n - 1] = DifferentiateThreePoint(tt[n - 1], tt[n - 3], xx[n - 3], tt[n - 2], xx[n - 2], tt[n - 1], xx[n - 1]);

            // Build Akima spline using Hermite interpolation scheme

            _hermiteSpline.InitInternal(tt, xx, d);
        }

        public double Interpolate(double t)
        {
            return _hermiteSpline.Interpolate(t);
        }

        public double Differentiate( double t, out double first, out double second )
        {
            return _hermiteSpline.Differentiate(t, out first, out second);
        }

        public double Integrate(double t)
        {
            return _hermiteSpline.Integrate(t);
        }
        static double DifferentiateThreePoint( double t, double t0, double x0, double t1, double x1, double t2, double x2)
        {
            // TODO: Optimization potential, but keep numeric stability in mind!
            t = t - t0;
            t1 = t1 - t0;
            t2 = t2 - t0;
            double a = (x2 - x0 - t2 / t1 * (x1 - x0)) / (t2 * t2 - t1 * t2);
            double b = (x1 - x0 - a * t1 * t1) / t1;
            return 2 * a * t + b;
        }
    }
}
