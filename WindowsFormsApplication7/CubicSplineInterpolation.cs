using System;
using System.Collections.Generic;

namespace WindowsFormsApplication7
{
    public class CubicSplineInterpolation : IInterpolationMethod
    {
        CubicHermiteSplineInterpolation _hermiteSpline;
        public CubicSplineInterpolation()
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

        public void Init(IList<double> t, IList<double> x)
        {
            Init(
                t,
                x,
                SplineBoundaryCondition.SecondDerivative,
                0.0,
                SplineBoundaryCondition.SecondDerivative,
                0.0
                );
        }
        public void Init(IList<double> t, IList<double> x, SplineBoundaryCondition leftBoundaryCondition, double leftBoundary, SplineBoundaryCondition rightBoundaryCondition, double rightBoundary)
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
                throw new ArgumentException("x");
            }

            int n = t.Count;

            double[] tt = new double[n];
            t.CopyTo(tt, 0);
            double[] xx = new double[n];
            x.CopyTo(xx, 0);

            Sorting.Sort(tt, xx);

            // normalize special cases
            if((n == 2)
                && (leftBoundaryCondition == SplineBoundaryCondition.ParabolicallyTerminated)
                && (rightBoundaryCondition == SplineBoundaryCondition.ParabolicallyTerminated))
            {
                leftBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                leftBoundary = 0d;
                rightBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                rightBoundary = 0d;
            }

            if(leftBoundaryCondition == SplineBoundaryCondition.Natural)
            {
                leftBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                leftBoundary = 0d;
            }

            if(rightBoundaryCondition == SplineBoundaryCondition.Natural)
            {
                rightBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                rightBoundary = 0d;
            }

            double[] a1 = new double[n];
            double[] a2 = new double[n];
            double[] a3 = new double[n];
            double[] b = new double[n];

            // Left Boundary
            switch(leftBoundaryCondition)
            {
                case SplineBoundaryCondition.ParabolicallyTerminated:
                    a1[0] = 0;
                    a2[0] = 1;
                    a3[0] = 1;
                    b[0] = 2 * (xx[1] - xx[0]) / (tt[1] - tt[0]);
                    break;
                case SplineBoundaryCondition.FirstDerivative:
                    a1[0] = 0;
                    a2[0] = 1;
                    a3[0] = 0;
                    b[0] = leftBoundary;
                    break;
                case SplineBoundaryCondition.SecondDerivative:
                    a1[0] = 0;
                    a2[0] = 2;
                    a3[0] = 1;
                    b[0] = 3 * (xx[1] - xx[0]) / (tt[1] - tt[0]) - 0.5 * leftBoundary * (tt[1] - tt[0]);
                    break;
                default:
                    throw new NotSupportedException("BoundaryCondition");
            }

            // Central Conditions
            for(int i = 1; i < tt.Length - 1; i++)
            {
                a1[i] = tt[i + 1] - tt[i];
                a2[i] = 2 * (tt[i + 1] - tt[i - 1]);
                a3[i] = tt[i] - tt[i - 1];
                b[i] = 3 * (xx[i] - xx[i - 1]) / (tt[i] - tt[i - 1]) * (tt[i + 1] - tt[i]) + 3 * (xx[i + 1] - xx[i]) / (tt[i + 1] - tt[i]) * (tt[i] - tt[i - 1]);
            }

            // Right Boundary
            switch(rightBoundaryCondition)
            {
                case SplineBoundaryCondition.ParabolicallyTerminated:
                    a1[n - 1] = 1;
                    a2[n - 1] = 1;
                    a3[n - 1] = 0;
                    b[n - 1] = 2 * (xx[n - 1] - xx[n - 2]) / (tt[n - 1] - tt[n - 2]);
                    break;
                case SplineBoundaryCondition.FirstDerivative:
                    a1[n - 1] = 0;
                    a2[n - 1] = 1;
                    a3[n - 1] = 0;
                    b[n - 1] = rightBoundary;
                    break;
                case SplineBoundaryCondition.SecondDerivative:
                    a1[n - 1] = 1;
                    a2[n - 1] = 2;
                    a3[n - 1] = 0;
                    b[n - 1] = 3 * (xx[n - 1] - xx[n - 2]) / (tt[n - 1] - tt[n - 2]) + 0.5 * rightBoundary * (tt[n - 1] - tt[n - 2]);
                    break;
                default:
                    throw new NotSupportedException("BoundaryCondition");
            }

            // Build Spline
            double[] dd = SolveTridiagonal(a1, a2, a3, b);
            _hermiteSpline.InitInternal(tt, xx, dd);
        }

        public double Interpolate(double t)
        {
            return _hermiteSpline.Interpolate(t);
        }

        public double Differentiate(double t, out double first, out double second)
        {
            return _hermiteSpline.Differentiate(t, out first, out second);
        }

        public double Integrate(double t)
        {
            return _hermiteSpline.Integrate(t);
        }
        static double[] SolveTridiagonal(double[] a, double[] b, double[] c, double[] d)
        {
            double[] x = new double[a.Length];

            for(int k = 1; k < a.Length; k++)
            {
                double t = a[k] / b[k - 1];
                b[k] = b[k] - t * c[k - 1];
                d[k] = d[k] - t * d[k - 1];
            }

            x[x.Length - 1] = d[d.Length - 1] / b[b.Length - 1];
            for(int k = x.Length - 2; k >= 0; k--)
            {
                x[k] = (d[k] - c[k] * x[k + 1]) / b[k];
            }

            return x;
        }
    }
}
