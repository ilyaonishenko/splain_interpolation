using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication7
{
    public partial class Form1 : Form
    {

        // Массивы для хранения координат опорных точек
        double[] x, y;
        // Массив для хранения коэффициентов для уравнений сплайна
        double[,] coeffs;
        // Количество опорных точек
        int size;
        // Номер строки в файле с первой ошибкой, если такая имеется
        int error_line;
        // Счетчик для осведомления об ошибке при введении двух или более координат с одной и той же координатой х
        bool same = false;

        double x_same = 0;

        StreamReader reader;

        int spline_size = 0;

        List<double> xlist;

        List<double> ylist;

        List<double> listx;

        List<double> listy;



        public Form1()
        {
            InitializeComponent();
            //button3.Enabled = false;
            button4.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
            xlist = new List<double>();
            ylist = new List<double>();
            listx = new List<double>();
            listy = new List<double>();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public void ReadPoints()
        {
            reader = new StreamReader("C:\\Users\\veryoldbarny\\Documents\\input.txt");
            // Подсчет количества опорных узлов
            size = 0;
            // Номер строки с первой ошибкой, если такая имеется
            error_line = 0;

            size = Convert.ToInt32(reader.ReadLine());
            x = new double[size];
            for (int i = 0; i < size; i++) x[i] = 0.0;
            y = new double[size];
            for (int i = 0; i < size; i++) y[i] = 0.0;

            // Проверка на корректность считанных точек
            string line = "";
            string[] points;

            for (int i = 0; i < size; i++)
            {
                line = reader.ReadLine();
                points = line.Split();
                error_line++;
                x[i] = Convert.ToDouble(points[0]);
                y[i] = Convert.ToDouble(points[1]);
            }

            // Сортировка точек в порядке возрастания по x
            double x_change = 0, y_change = 0;
            for (int i = 0; i < size - 1; i++)
                for (int j = 0; j < size - i - 1; j++)
                    if (x[j] > x[j + 1])
                    {
                        x_change = x[j];
                        x[j] = x[j + 1];
                        x[j + 1] = x_change;
                        y_change = y[j];
                        y[j] = y[j + 1];
                        y[j + 1] = y_change;
                    }

            // Оповещение ошибки в том случае, если несколько точек имеют одинаковое значение координаты х
            x_same = 0;
            for (int i = 1; i < size; i++)
                if (x[i] == x[i - 1])
                {
                    same = true;
                    x_same = x[i];
                }
            reader.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void realChart()
        {
            for (int i = 0; i < x.Length; i++)
            {
                chart1.Series["real"].Points.AddXY(x[i], y[i]);
            }
            chart1.Series["real"].ChartType = SeriesChartType.Line;
        }

        private void clearChart()
        {
            foreach (var series in chart1.Series)
                series.Points.Clear();
            listx.Clear();
            listy.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                spline_size = Int32.Parse(textBox1.Text);
                this.ReadPoints();
                this.clearChart();
                this.realChart();
            }
            catch (System.FormatException err)
            {
                MessageBox.Show("System.FormatException\n"+err.StackTrace);
            }

            if (spline_size != 0)
            {
                //button3.Enabled = true;
                button4.Enabled = true;
            }
            else
            {
                MessageBox.Show("Spline size required");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            /*int n = Int16.Parse(textBox1.Text);
            label1.Text = n.ToString();
            //cInterpolation();
            //qInterpolation();
            CubicSplineInterpolation csinterpol = new CubicSplineInterpolation();
            List<double> xlist = x.ToList<double>();
            List<double> ylist = y.ToList<double>();
            csinterpol.Init(xlist, ylist);
            List<double> listx = new List<double>();
            List<double> listy = new List<double>();
            for (int i = Convert.ToInt32(x[0]); i < x[x.Length - 1]; i++)
            {
                listx.Add(i);
                listy.Add(csinterpol.Interpolate(i));
            }
            for (int i = 0; i < listx.Count; i++)
            {
                chart1.Series["interpolated4"].Points.AddXY(listx[i], listy[i]);
            }
            chart1.Series["interpolated4"].ChartType = SeriesChartType.Line;*/

            int siz = Int32.Parse(textBox2.Text);
            Generator gen = new Generator();
            gen.generate(siz, 1, 3);

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void cInterpolation()
        {
            CubicSpline cSpline = new CubicSpline();
            cSpline.BuildSpline(x, y, x.Length);
            List<double> listx = new List<double>();
            List<double> listy = new List<double>();
            for (int i = Convert.ToInt32(x[0]); i < x[x.Length - 1]; i++)
            {
                listx.Add(i);
                listy.Add(cSpline.Interpolate(i));
            }
            for (int i = 0; i < listx.Count; i++)
            {
                chart1.Series["interpolated3"].Points.AddXY(listx[i], listy[i]);
            }
            chart1.Series["interpolated3"].ChartType = SeriesChartType.Line;
        }

        private void qInterpolation()
        {
            QSpline qSpline = new QSpline();
            qSpline.BuildSpline(x, y, x.Length);
            List<double> listx = new List<double>();
            List<double> listy = new List<double>();
            for (int i = Convert.ToInt32(x[0]); i < x[x.Length - 1]; i++)
            {
                listx.Add(i);
                listy.Add(qSpline.Interpolate(i));
            }
            for (int i = 0; i < listx.Count; i++)
            {
                chart1.Series["interpolated4"].Points.AddXY(listx[i], listy[i]);
            }
            chart1.Series["interpolated4"].ChartType = SeriesChartType.Line;
        }

        private void button4_Click(object sender, EventArgs e)
        {

            if (Int32.Parse(textBox1.Text) != spline_size)
            {
                spline_size = Int32.Parse(textBox1.Text);
            }

            this.clearChart();
            this.realChart();
            this.checkSplines();
        }

        private void checkSplines()
        {
            xlist = x.ToList<double>();
            ylist = y.ToList<double>();

            switch(spline_size)
            {
                case 1:
                    LinearSplineInterpolation splineInterpol = new LinearSplineInterpolation();
                    splineInterpol.Init(xlist, ylist);
                    for (int i = Convert.ToInt32(x[0]); i < x[x.Length - 1]; i++)
                    {
                        listx.Add(i);
                        listy.Add(splineInterpol.Interpolate(i));
                    }
                    break;
                case 2:
                    //MessageBox.Show("do not work for now");
                    QuadraticSpline quadraticSplineInterpol = new QuadraticSpline();
                    quadraticSplineInterpol.Init(xlist, ylist);
                    for(int i = Convert.ToInt32(x[0]); i< x[x.Length-1]; i++)
                    {
                        listx.Add(i);
                        listy.Add(quadraticSplineInterpol.Interpolate(i));
                    }
                    break;
                case 3:
                    CubicSplineInterpolation cubicSplineInterpol = new CubicSplineInterpolation();
                    cubicSplineInterpol.Init(xlist, ylist);
                    for (int i = Convert.ToInt32(x[0]); i < x[x.Length - 1]; i++)
                    {
                        listx.Add(i);
                        listy.Add(cubicSplineInterpol.Interpolate(i));
                    }
                    break;
                default:
                    CubicSplineInterpolation cubicSplineInterpol1 = new CubicSplineInterpolation();
                    cubicSplineInterpol1.Init(xlist, ylist);
                    for (int i = Convert.ToInt32(x[0]); i < x[x.Length - 1]; i++)
                    {
                        listx.Add(i);
                        listy.Add(cubicSplineInterpol1.Interpolate(i));
                    }
                    break;
            }
            for (int i = 0; i < listx.Count; i++)
            {
                chart1.Series["interpolated4"].Points.AddXY(listx[i], listy[i]);
            }
            chart1.Series["interpolated4"].ChartType = SeriesChartType.Spline;
        }
    }
}
