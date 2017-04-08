using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication7
{
    class QuadraticSpline : IInterpolationMethod
    {
        SplineInterpolation _spline;

        public QuadraticSpline()
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

        public void Init(IList<double> t, IList<double> x)
        {
            if (null == t)
            {
                throw new ArgumentNullException("t");
            }

            if (null == x)
            {
                throw new ArgumentNullException("x");
            }

            if (t.Count < 2)
            {
                throw new ArgumentOutOfRangeException("t");
            }

            if (t.Count != x.Count)
            {
                throw new ArgumentException("ArgumentVectorsSameLengths");
            }

            int n = t.Count;

            double[] c = new double[4 * (t.Count - 1)];
            double[] tt = new double[t.Count];
            t.CopyTo(tt, 0);
            double[] xx = new double[x.Count];
            x.CopyTo(xx, 0);

            Sorting.Sort(tt, xx);

            double[] a1 = new double[n];
            double[] a2 = new double[n];
            double[] a3 = new double[n];
            double[] b = new double[n];

            a1[0] = 0;
            a2[0] = 1;
            a3[0] = 0;
            b[0] = 0;

            for (int i = 1; i < tt.Length-1; i++)
            {
                a1[i] = tt[i + 1] - tt[i];
                a2[i] = 2 * (tt[i + 1] - tt[i - 1]);
                a3[i] = tt[i] - tt[i - 1];
                b[i] = 3 * (xx[i] - xx[i - 1]) / (tt[i] - tt[i - 1]) * (tt[i + 1] - tt[i]) + 3 * (xx[i + 1] - xx[i]) / (tt[i + 1] - tt[i]) * (tt[i] - tt[i - 1]);
            }

            a1[n - 1] = 0;
            a2[n - 1] = 1;
            a3[n - 1] = 0;
            b[n - 1] = 0;

            double[] dd = SolveTridiagonal(a1, a2, a3, b);
            InitInternal(tt, xx, dd);
        }

        internal void InitInternal(double[] t, double[] x, double[] d)
        {
            double[] c = new double[4 * (t.Length - 1)];

            for (int i = 0, j = 0; i < t.Length - 1; i++, j += 4)
            {
                double delta = t[i + 1] - t[i];
                double delta2 = delta * delta;
                double delta3 = delta * delta2;
                c[j] = x[i];
                c[j + 1] = d[i];
                c[j + 2] = (3 * (x[i + 1] - x[i]) - 2 * d[i] * delta - d[i + 1] * delta) / delta2;
                c[j + 3] = 0;
            }

            _spline.Init(t, c);
        }

        static double[] SolveTridiagonal(double[] a, double[] b, double[] c, double[] d)
        {
            double[] x = new double[a.Length];

            for (int k = 1; k < a.Length; k++)
            {
                double t = a[k] / b[k - 1];
                b[k] = b[k] - t * c[k - 1];
                d[k] = d[k] - t * d[k - 1];
            }

            x[x.Length - 1] = d[d.Length - 1] / b[b.Length - 1];
            for (int k = x.Length - 2; k >= 0; k--)
            {
                x[k] = (d[k] - c[k] * x[k + 1]) / b[k];
            }

            return x;
        }

        public double Differentiate(double t, out double first, out double second)
        {
            return _spline.Differentiate(t, out first, out second);
        }

        public double Integrate(double t)
        {
            return _spline.Integrate(t);
        }

        public double Interpolate(double t)
        {
            return _spline.Interpolate(t);
        }
    }
}
