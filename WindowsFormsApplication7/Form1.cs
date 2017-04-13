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
using System.Threading;

namespace WindowsFormsApplication7
{
    public partial class Form1 : Form
    {
        // Массив для хранения коэффициентов для уравнений сплайна
        double[,] coeffs;
        // Номер строки в файле с первой ошибкой, если такая имеется
        int error_line;
        // Счетчик для осведомления об ошибке при введении двух или более координат с одной и той же координатой х
        bool same = false;

        double x_same = 0;

        private static int cycleCount = 0;

        StreamReader reader;

        GlobalValues.SplineBox spline1;
        GlobalValues.SplineBox spline2;
        GlobalValues.SplineBox spline3;

        public Form1()
        {
            InitializeComponent();
            //button3.Enabled = false;
            button4.Enabled = false;
            GlobalValues.LogToFile = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
            GlobalValues.xlist = new List<double>();
            GlobalValues.ylist = new List<double>();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public void ReadPoints()
        {
            reader = new StreamReader("C:\\Users\\veryoldbarny\\Documents\\input.txt");
            // Номер строки с первой ошибкой, если такая имеется
            error_line = 0;

            GlobalValues.SIZE = Convert.ToInt32(reader.ReadLine());
            GlobalValues.X = new double[GlobalValues.SIZE];
            for (int i = 0; i < GlobalValues.SIZE; i++) GlobalValues.X[i] = 0.0;
            GlobalValues.Y = new double[GlobalValues.SIZE];
            for (int i = 0; i < GlobalValues.SIZE; i++) GlobalValues.Y[i] = 0.0;

            // Проверка на корректность считанных точек
            string line = "";
            string[] points;

            for (int i = 0; i < GlobalValues.SIZE; i++)
            {
                line = reader.ReadLine();
                points = line.Split();
                error_line++;
                GlobalValues.X[i] = Convert.ToDouble(points[0]);
                GlobalValues.Y[i] = Convert.ToDouble(points[1]);
            }

            // Сортировка точек в порядке возрастания по x
            double x_change = 0, y_change = 0;
            for (int i = 0; i < GlobalValues.SIZE - 1; i++)
                for (int j = 0; j < GlobalValues.SIZE - i - 1; j++)
                    if (GlobalValues.X[j] > GlobalValues.X[j + 1])
                    {
                        x_change = GlobalValues.X[j];
                        GlobalValues.X[j] = GlobalValues.X[j + 1];
                        GlobalValues.X[j + 1] = x_change;
                        y_change = GlobalValues.Y[j];
                        GlobalValues.Y[j] = GlobalValues.Y[j + 1];
                        GlobalValues.Y[j + 1] = y_change;
                    }

            // Оповещение ошибки в том случае, если несколько точек имеют одинаковое значение координаты х
            x_same = 0;
            for (int i = 1; i < GlobalValues.SIZE; i++)
                if (GlobalValues.X[i] == GlobalValues.X[i - 1])
                {
                    same = true;
                    x_same = GlobalValues.X[i];
                }
            reader.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void realChart()
        {
            for (int i = 0; i < GlobalValues.X.Length; i++)
            {
                chart1.Series["real"].Points.AddXY(GlobalValues.X[i], GlobalValues.Y[i]);
                chart2.Series["real"].Points.AddXY(GlobalValues.X[i], GlobalValues.Y[i]);
                chart3.Series["real"].Points.AddXY(GlobalValues.X[i], GlobalValues.Y[i]);
            }
            chart1.Series["real"].ChartType = SeriesChartType.Line;
            chart2.Series["real"].ChartType = SeriesChartType.Line;
            chart3.Series["real"].ChartType = SeriesChartType.Line;
        }

        private void clearChart()
        {
            foreach (var series in chart1.Series)
                series.Points.Clear();
            foreach (var series in chart2.Series)
                series.Points.Clear();
            foreach (var series in chart3.Series)
                series.Points.Clear();
            /*
            listx.Clear();
            listy.Clear();
             *
            spline1.listX = new List<double>();
            spline1.listY = new List<double>();
            spline2.listX = new List<double>();
            spline2.listY = new List<double>();
            spline3.listX = new List<double>();
            spline3.listY = new List<double>();
             *  */
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                spline1 = new GlobalValues.SplineBox(Int32.Parse(textBox1.Text));
                spline2 = new GlobalValues.SplineBox(Int32.Parse(textBox4.Text));
                spline3 = new GlobalValues.SplineBox(Int32.Parse(textBox3.Text));

                this.ReadPoints();
                this.clearChart();
                this.realChart();
            }
            catch (System.FormatException err)
            {
                MessageBox.Show("System.FormatException\n"+err.StackTrace);
            }

            if (spline1.POWER != 0 && spline2.POWER != 0 && spline3.POWER != 0)
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
            int naturalDotsSize = Int32.Parse(textBox2.Text);
            Generator gen = new Generator();
            gen.generate(naturalDotsSize, 1, 3);

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void cubicInterpolation(GlobalValues.SplineBox spline, Chart chart)
        {
            CubicSpline cSpline = new CubicSpline();
            cSpline.BuildSpline(GlobalValues.X, GlobalValues.Y, GlobalValues.X.Length);
            /*
            List<double> listx = new List<double>();
            List<double> listy = new List<double>();
             * */
            for (int i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i++)
            {
                spline.listX.Add(i);
                spline.listY.Add(cSpline.Interpolate(i));
            }
            for (int i = 0; i < spline.listX.Count; i++)
            {
                chart.Series["interpolated3"].Points.AddXY(spline.listX[i], spline.listY[i]);
            }
            chart.Series["interpolated3"].ChartType = SeriesChartType.Line;
        }

        private void qInterpolation(GlobalValues.SplineBox spline, Chart chart)
        {
            QSpline qSpline = new QSpline();
            qSpline.BuildSpline(GlobalValues.X, GlobalValues.Y, GlobalValues.X.Length);
            /*
            List<double> listx = new List<double>();
            List<double> listy = new List<double>();
             * */
            for (int i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i++)
            {
                spline.listX.Add(i);
                spline.listY.Add(qSpline.Interpolate(i));
            }
            for (int i = 0; i < spline.listX.Count; i++)
            {
                chart.Series["interpolated4"].Points.AddXY(spline.listX[i], spline.listY[i]);
            }
            chart.Series["interpolated4"].ChartType = SeriesChartType.Line;
        }

        private void button4_Click(object sender, EventArgs e)
        {

            if (Int32.Parse(textBox1.Text) != spline1.POWER)
            {
                spline1.POWER = Int32.Parse(textBox1.Text);
            }
            if (Int32.Parse(textBox4.Text) != spline2.POWER)
            {
                spline2.POWER = Int32.Parse(textBox4.Text);
            }
            if (Int32.Parse(textBox3.Text) != spline3.POWER)
            {
                spline3.POWER = Int32.Parse(textBox3.Text);
            }

            this.clearChart();
            this.realChart();
            string paramsLog = string.Format("Начало работы");
            Logger.Current.WriteLine(paramsLog);
            StartThreads();

            drawChart(spline1, chart1);
            drawChart(spline2, chart2);
            drawChart(spline3, chart3);
            /*
            this.checkSplines(spline1,chart1);
            this.checkSplines(spline2,chart2);
            this.checkSplines(spline3,chart3);
             * */
        }

        private static void checkSplines(GlobalValues.SplineBox spline, Chart chart)
        {
            GlobalValues.xlist = GlobalValues.X.ToList<double>();
            GlobalValues.ylist = GlobalValues.Y.ToList<double>();
            Thread.Sleep(5);

            switch(spline.POWER)
            {
                case 1:
                    LinearSplineInterpolation splineInterpol = new LinearSplineInterpolation();
                    splineInterpol.Init(GlobalValues.xlist, GlobalValues.ylist);
                    for (int i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i++)
                    {
                        spline.listX.Add(i);
                        spline.listY.Add(splineInterpol.Interpolate(i));
                    }
                    break;
                case 2:
                    //MessageBox.Show("do not work for now");
                    QuadraticSpline quadraticSplineInterpol = new QuadraticSpline();
                    quadraticSplineInterpol.Init(GlobalValues.xlist, GlobalValues.ylist);
                    for(int i = Convert.ToInt32(GlobalValues.X[0]); i< GlobalValues.X[GlobalValues.X.Length-1]; i++)
                    {
                        spline.listX.Add(i);
                        spline.listY.Add(quadraticSplineInterpol.Interpolate(i));
                    }
                    break;
                case 3:
                    CubicSplineInterpolation cubicSplineInterpol = new CubicSplineInterpolation();
                    cubicSplineInterpol.Init(GlobalValues.xlist, GlobalValues.ylist);
                    for (int i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i++)
                    {
                        spline.listX.Add(i);
                        spline.listY.Add(cubicSplineInterpol.Interpolate(i));
                    }
                    break;
                default:
                    CubicSplineInterpolation cubicSplineInterpol1 = new CubicSplineInterpolation();
                    cubicSplineInterpol1.Init(GlobalValues.xlist, GlobalValues.ylist);
                    for (int i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i++)
                    {
                        spline.listX.Add(i);
                        spline.listY.Add(cubicSplineInterpol1.Interpolate(i));
                    }
                    break;
            }
        }

        private void drawChart(GlobalValues.SplineBox spline, Chart chart)
        {
            for (int i = 0; i < spline.listX.Count; i++)
            {
                chart.Series["interpolated4"].Points.AddXY(spline.listX[i], spline.listY[i]);
            }
            chart.Series["interpolated4"].ChartType = SeriesChartType.Spline;
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void StartThreads()
        {
            ProcessTimeStopwatch processTimeStopwatch = new ProcessTimeStopwatch();
            processTimeStopwatch.Start();
            foreach(var spline1Priority in GlobalValues.ThreadPriorities)
            {
                foreach(var spline2Priority in GlobalValues.ThreadPriorities)
                {
                    foreach(var spline3Priority in GlobalValues.ThreadPriorities)
                    {
                        GlobalValues.EnterCriticalSection(GlobalValues.LockObject);
                        try
                        {
                            cycleCount++;
                            Logger.Current.WriteLine("Цикл номер: {0}", cycleCount);
                            StartNewThread(spline1, chart1, spline1Priority);
                            StartNewThread(spline2, chart2, spline2Priority);
                            StartNewThread(spline3, chart3, spline3Priority);
                        }
                        finally
                        {
                            GlobalValues.LeaveCriticalSection(GlobalValues.LockObject);
                        }
                        Thread.Sleep(10);
                        GlobalValues.SevEvent();
                    }
                }
            }
            processTimeStopwatch.Stop();
            Logger.Current.WriteLine();
            Logger.Current.WriteLine("Время работы процесса: {0}", processTimeStopwatch.Elapsed);
        }

        private static void StartNewThread(GlobalValues.SplineBox spline, Chart chart, ThreadPriority threadPriority)
        {
            Action action = () => Worker(spline, chart, threadPriority);
            ThreadStart threadStart = new ThreadStart(action);



            Thread workThread = new Thread(threadStart);
            workThread.Priority = threadPriority;
            workThread.Start();
        }

        private static void Worker(GlobalValues.SplineBox spline, Chart chart, ThreadPriority threadPriority)
        {
            GlobalValues.EnterCriticalSection(GlobalValues.LockObject);
            GlobalValues.LeaveCriticalSection(GlobalValues.LockObject);
            ThreadTimeStopwatch threadTimeStopwatch = new ThreadTimeStopwatch();
            threadTimeStopwatch.Start();

            try
            {
                checkSplines(spline, chart);

                threadTimeStopwatch.Stop();

                WriteResultSummary(spline, threadPriority, threadTimeStopwatch.Elapsed);
            }
            finally
            {
                GlobalValues.WaitForSingleObject();
                GlobalValues.SevEvent();
            }

        }

        private static void WriteResultSummary(GlobalValues.SplineBox spline, ThreadPriority threadPriority, TimeSpan elapsed)
        {
            Logger.Current.WriteLine(string.Empty);

            string summary = string.Format("Сплайн степени{0}; приоритет: {1}; время выполнения: {2} ", spline.POWER, threadPriority, elapsed);
            Logger.Current.WriteLine(summary);
        }
    }
}
