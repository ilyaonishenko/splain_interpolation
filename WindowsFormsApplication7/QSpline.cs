using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication7
{
    class QSpline
    {
        SplineTuple[] splines;

        private struct SplineTuple
        {
            public double a, b, c, d, e, x;
        }

        public void BuildSpline(double[] x, double[] y, int n)
        {
            splines = new SplineTuple[n];

            for (int i = 0; i < n; ++i)
            {
                splines[i].x = x[i];
                splines[i].a = y[i];
            }
            splines[0].d = splines[n - 1].d = splines[0].c = 0.0;

            double[] alpha = new double[n - 1];
            double[] beta = new double[n - 1];
            alpha[0] = beta[0] = 0.0;
            for (int i = 1; i < n - 1; ++i)
            {
                double hi = x[i] - x[i - 1];
                double hi1 = x[i + 1] - x[i];
                double A = (7.0*Math.Pow(hi,3)+6.0*hi1)/4.0*hi;
                double C = (3.0 * Math.Pow(hi,3) - 3.0 * Math.Pow(hi1,2) * hi + 6.0 * hi1) / 4.0 * hi;
                double B = hi1*hi1/4;
                double F = 0.006*((y[i] - y[i - 1]) / hi);
                double z = (A * alpha[i - 1] + C);
                alpha[i] = -B / z;
                beta[i] = (F - A * beta[i - 1]) / z;
                Console.WriteLine("QSpline");
                Console.WriteLine("A: " + A.ToString() + "\nB: " + B.ToString() + "\nC: " + C.ToString() + "\nF: " + F.ToString() + "\nz: " + z.ToString() + "\nalpha: " + alpha.ToString() + "\nbeta: " + beta.ToString());
            }

            for (int i = n - 2; i > 0; --i)
            {
                splines[i].d = alpha[i] * splines[i + 1].d + beta[i];
            }
            
            /*for (int i = n - 1; i > 0; --i)
            {
                double hi = x[i] - x[i - 1];
                splines[i].e = (splines[i].d - splines[i - 1].d) / 4.0*hi;
                //splines[i].b = hi * (2.0 * splines[i].c + splines[i - 1].c) / 6.0 + (y[i] - y[i - 1]) / hi;
                if (i == n - 1)
                    splines[i].c = 0;
                else
                {
                    splines[i].c = splines[i - 1].c + 3.0 * splines[i].d * hi + 6.0 * (splines[i].d + 3.0 * splines[i - 1].d) * hi / 6.0;
                }
                splines[i].b = (y[i] - y[i - 1]) / hi - splines[i].c * hi - ((splines[i].d - splines[i - 1].d) * hi * hi) / 4.0;
            }*/
            for (int i = 0; i < n-1; i++)
            {
                double hi = x[i+1] - x[i];
                splines[i].e = (splines[i + 1].d - splines[i].d) / (4.0 * hi);
                if (i == 0) { }
                else
                {
                    splines[i].c = splines[i - 1].c + 3.0 * splines[i - 1].d * (x[i] - x[i - 1]) + 6.0 * (splines[i].d + 3.0 * splines[i - 1].d) * (x[i] - x[i - 1]);
                }
                splines[i].b = (y[i+1] - y[i]) / hi - splines[i].c * hi - ((splines[i+1].d - splines[i].d) * hi * hi) / 4.0;
            }
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine("a: " + splines[i].a + " b: " + splines[i].b + " d: " + splines[i].d + " c: " + splines[i].c+" e: "+splines[i].e);
            }
        }

        public double Interpolate(double x)
        {
            if (splines == null)
            {
                return double.NaN; // Если сплайны ещё не построены - возвращаем NaN
            }

            int n = splines.Length;
            SplineTuple s;

            if (x <= splines[0].x) // Если x меньше точки сетки x[0] - пользуемся первым эл-тов массива
            {
                s = splines[0];
            }
            else if (x >= splines[n - 1].x) // Если x больше точки сетки x[n - 1] - пользуемся последним эл-том массива
            {
                s = splines[n - 1];
            }
            else // Иначе x лежит между граничными точками сетки - производим бинарный поиск нужного эл-та массива
            {
                int i = 0;
                int j = n - 1;
                while (i + 1 < j)
                {
                    int k = i + (j - i) / 2;
                    if (x <= splines[k].x)
                    {
                        j = k;
                    }
                    else
                    {
                        i = k;
                    }
                }
                s = splines[j];
            }

            double dx = x - s.x;
            // Вычисляем значение сплайна в заданной точке по схеме Горнера (в принципе, "умный" компилятор применил бы схему Горнера сам, но ведь не все так умны, как кажутся)
            //double y = s.a + (s.b + (s.c + s.d * dx) * dx) * dx;
            double y = s.a + (s.b+(s.c+(s.d+s.e*dx)*dx)*dx) * dx;
            //double y = s.a * Math.Pow(dx, 0) + s.b * Math.Pow(dx, 1) + s.c * Math.Pow(dx, 2) + s.d * Math.Pow(dx, 3) + s.e * Math.Pow(dx, 4);
            Console.WriteLine("x: " + x.ToString() + " y: " + y.ToString());
            return y;
        }
    }
}
