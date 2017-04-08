using System;
using System.Collections.Generic;

namespace WindowsFormsApplication7
{
    public class SplineInterpolation : IInterpolationMethod
    {
        IList<double> _t;
        IList<double> _c;
        int _n;
        public uint RowCount;
        public uint ColumCount;
        public double[][] Matrix { get; set; }
        public double[] RightPart { get; set; }
        public double[] Answer { get; set; }

        public SplineInterpolation()
        {

        }

        public SplineInterpolation(uint Row, uint Colum)
        {
            RightPart = new double[Row];
            Answer = new double[Row];
            Matrix = new double[Row][];
            for (int i = 0; i < Row; i++)
                Matrix[i] = new double[Colum];
            RowCount = Row;
            ColumCount = Colum;

            for (int i = 0; i < Row; i++)
            {
                Answer[i] = 0;
                RightPart[i] = 0;
                for (int j = 0; j < Colum; j++)
                    Matrix[i][j] = 0;
            }
        }
        public bool SupportsDifferentiation
        {
            get { return true; }
        }
        public bool SupportsIntegration
        {
            get { return true; }
        }

        public void Init(IList<double> t, IList<double> c)
        {
            if (null == t)
            {
                throw new ArgumentNullException("t");
            }

            if (null == c)
            {
                throw new ArgumentNullException("c");
            }

            if (t.Count < 1)
            {
                throw new ArgumentOutOfRangeException("t");
            }

            if (c.Count != 4 * (t.Count - 1))
            {
                throw new ArgumentOutOfRangeException("c");
            }

            _t = t;
            _c = c;
            _n = t.Count;
        }

        public double Interpolate(double t)
        {
            int low = 0;
            int high = _n - 1;
            while (low != high - 1)
            {
                int middle = (low + high) / 2;
                if (_t[middle] > t)
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            t = t - _t[low];
            int k = low << 2;//*4
            return _c[k] + t * (_c[k + 1] + t * (_c[k + 2] + t * _c[k + 3]));
        }

        public double Differentiate(double t, out double first, out double second)
        {
            int low = 0;
            int high = _n - 1;
            while (low != high - 1)
            {
                int middle = (low + high) / 2;
                if (_t[middle] > t)
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            t = t - _t[low];
            int k = low << 2;
            first = _c[k + 1] + 2 * t * _c[k + 2] + 3 * t * t * _c[k + 3];
            second = 2 * _c[k + 2] + 6 * t * _c[k + 3];
            return _c[k] + t * (_c[k + 1] + t * (_c[k + 2] + t * _c[k + 3]));
        }

        public double Integrate(double t)
        {
            int low = 0;
            int high = _n - 1;
            while (low != high - 1)
            {
                int middle = (low + high) / 2;
                if (_t[middle] > t)
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            double result = 0;
            for (int i = 0, j = 0; i < low; i++, j += 4)
            {
                double w = _t[i + 1] - _t[i];
                result += w * (_c[j] + w * (_c[j + 1] * 0.5 + w * (_c[j + 2] / 3 + w * _c[j + 3] * 0.25)));
            }

            t = t - _t[low];
            int k = low << 2;
            return result + t * (_c[k] + t * (_c[k + 1] * 0.5 + t * (_c[k + 2] / 3 + t * _c[k + 3] * 0.25)));
        }

        private void SortRows(int SortIndex)
        {
            double MaxElement = Matrix[SortIndex][SortIndex];
            int MaxElementIndex = SortIndex;
            for (int i = SortIndex + 1; i < RowCount; i++)
            {
                if (Matrix[i][SortIndex] > MaxElement)
                {
                    MaxElement = Matrix[i][SortIndex];
                    MaxElementIndex = i;
                }
            }

            if (MaxElementIndex > SortIndex)
            {
                double Temp;
                Temp = RightPart[MaxElementIndex];
                RightPart[MaxElementIndex] = RightPart[SortIndex];
                RightPart[SortIndex] = Temp;
                for (int i = 0; i < ColumCount; i++)
                {
                    Temp = Matrix[MaxElementIndex][i];
                    Matrix[MaxElementIndex][i] = Matrix[SortIndex][i];
                    Matrix[SortIndex][i] = Temp;
                }
            }
        }
        public int SolveMatrix()
        {
            if (RowCount != ColumCount)
                return 1;
            for (int i = 0; i < RowCount - 1; i++)
            {
                SortRows(i);
                for (int j = i + 1; j < RowCount; j++)
                {
                    if (Matrix[i][i] != 0)
                    {
                        double MultElement = Matrix[j][i] / Matrix[i][i];
                        for (int k = i; k < ColumCount; k++)
                            Matrix[j][k] -= Matrix[i][k] * MultElement;
                        RightPart[j] -= RightPart[i] * MultElement;
                    }
                }
            }

            for (int i = (int)(RowCount - 1); i >= 0; i--)
            {
                Answer[i] = RightPart[i];
                for (int j = (int)(RowCount - 1); j > i; j--)
                    Answer[i] -= Matrix[i][j] * Answer[j];
                if (Matrix[i][i] == 0)
                    if (RightPart[i] == 0)
                        return 2;
                    else
                        return 1;
                Answer[i] /= Matrix[i][i];
            }
            return 0;
        }

        public override String ToString()
        {
            String S = "";
            for (int i = 0; i < RowCount; i++)
            {
                S += "\r\n";
                for (int j = 0; j < ColumCount; j++)
                {
                    S += Matrix[i][j].ToString("F04") + "\t";
                }

                S += "\t" + Answer[i].ToString("F08");
                S += "\t" + RightPart[i].ToString("F04");
            }
            return S;
        }
    }
}
