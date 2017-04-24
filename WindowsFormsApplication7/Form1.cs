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
using System.Runtime.CompilerServices;
using System.Threading;
using MathNet.Numerics.Interpolation;

namespace WindowsFormsApplication7
{

    public unsafe partial class Form1 : Form
    {
        // Массив для хранения коэффициентов для уравнений сплайна
        double[,] coeffs;
        // Номер строки в файле с первой ошибкой, если такая имеется
        int error_line;
        // Счетчик для осведомления об ошибке при введении двух или более координат с одной и той же координатой х
        bool same = false;

        double x_same = 0;

        private static int cycleCount = 0;

        int i_global = 0;

        StreamReader reader;

        GlobalValues.SplineBox spline1;
        GlobalValues.SplineBox spline2;
        GlobalValues.SplineBox spline3;
        private GlobalValues.SplineBox cSpline1;
        private GlobalValues.SplineBox cSpline2;
        private GlobalValues.SplineBox cSpline3;

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private unsafe static extern uint CreateThread(
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
                uint dwStackSize,
                ThreadStart lpStartAddress,
                uint lpParameter,
                uint dwCreationFlags,
                out uint lpThreadId);

        public struct SECURITY_ATTRIBUTES
        {
            public ulong nLength; //аналог DWORD для С# - ulong
            public IntPtr lpSecurityDescriptor; //аналог LPVOID для C# - IntPtr
            public bool bInheritHandle; //смогут ли дочерние потоки наследовать описатель этого объекта
        }

        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);


        /*public delegate void Del();

        public const uint dw_milliSec_infinity = 0xFFFFFFFF;
        //Динамически подключаемая библиотека — это файл, содержащий коллекцию модулей, которые могут использоваться любым количеством различных
        [DllImport("Kernel32.dll")]//предоставляет приложениям многие базовые API Win32: управление памятью, операции ввода-вывода, создание процессов и потоков и функции синхронизации...
        public static extern uint CreateThread(
            ref SECURITY_ATTRIBUTES lpThreadAttributes, //Указатель на структуру SECURITY_ATTRIBUTES, которая обуславливает, может ли возвращенный дескриптор быть унаследован дочерними процессами. Если lpThreadAttributes является значением ПУСТО (NULL), дескриптор не может быть унаследован.
            uint dwStackSize,//Начальный размер стека, в байтах. Система округляет это значение до самой близкой страницы памяти. Если это значение нулевое, новый поток использует по умолчанию размер стека исполняемой программы.
            Del lpStartAddress,//Указатель на определяемую программой функцию типа LPTHREAD_START_ROUTINE, код которой исполняется потоком и обозначает начальный адрес потока.
            //Делегат — это тип, который инкапсулирует метод, т. е. его действие схоже с указателем функции в C и C++.
            uint lpParameter,//Указатель на переменную, которая передается в поток.
            uint dwCreationFlags,//Флажки, которые управляют созданием потока. Если установлен флаг CREATE_SUSPENDED, создается поток в состоянии ожидания и не запускается до тех пор, пока не будет вызвана функция ResumeThread.
            out uint lpThreadId);//Указатель на переменную, которая принимает идентификатор потока.

        [DllImport("Kernel32.dll")]
        public static extern int CloseHandle(uint hObject); //закрывает дескриптор открытого объекта
        // extern означает, что метод реализуется вне кода C#
        [DllImport("Kernel32.dll")]
        public static extern uint WaitForSingleObject(uint hHandle, uint dwMilliseconds);
        //позволяют потоку в любой момент приостановиться и ждать освобождения какого-либо объекта ядра.
        // hObject идентифицирует объект ядра, поддерживающий состояния «свободен-занят»
        //dwMilliseconds - сколько времени (в миллисекундах) поток готов ждать освобождения объекта.


        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();//Получает идентификатор текущего потока

        public struct SECURITY_ATTRIBUTES
        {
            public ulong nLength; //аналог DWORD для С# - ulong
            public IntPtr lpSecurityDescriptor; //аналог LPVOID для C# - IntPtr
            public bool bInheritHandle; //смогут ли дочерние потоки наследовать описатель этого объекта
        }*/

        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IAsyncResult hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IAsyncResult handle);

        //Мьютексы
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateMutex(IntPtr lpMutexAttributes, bool bInitialOwner, string lpName);

        [DllImport("kernel32.dll")]
        public static extern bool ReleaseMutex(IntPtr hMutex);

        //Семафоры
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateSemaphore(
            IntPtr lpSemaphoreAttributes,
            int lInitialCount,
            int lMaximumCount,
            string lpName);

        [DllImport("kernel32.dll")]
        static extern bool ReleaseSemaphore(IntPtr hSemaphore, int lReleaseCount, IntPtr lpPreviousCount);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenSemaphore(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

        //События
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll")]
        static extern bool SetEvent(IntPtr hEvent);


        [DllImport("kernel32.dll")]
        static extern bool ResetEvent(IntPtr hEvent);

        [DllImport("kernel32.dll", EntryPoint = "WaitForMultipleObjects", SetLastError = true)]
        static extern int WaitForMultipleObjects(UInt32 nCount, IntPtr[] lpHandles, Boolean fWaitAll, UInt32 dwMilliseconds);


        static IntPtr _semaphore;
        static IntPtr _mutex;
        static IntPtr _event;
        private static IntPtr c1;
        private static  IntPtr c2;

        public Form1()
        {
            InitializeComponent();
            //button3.Enabled = false;
            button4.Enabled = false;
            GlobalValues.LogToFile = true;
            button5.Enabled = false;
            label10.Visible = false;
            label11.Visible = false;
            label12.Visible = false;
            label13.Visible = false;
            _semaphore = CreateSemaphore((IntPtr)null, 1, 1, null);
            _mutex = CreateMutex((IntPtr)null, false, null);
            _event = CreateEvent((IntPtr)null, true, false, null);
            SetEvent(_event);
            label14.Visible = false;
            chart4.Visible = false;
            Form2 f2 = new Form2();
            f2.ShowDialog();
            c1 = IntPtr.Zero;
            c2 = new IntPtr(1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
            GlobalValues.xlist = new List<double>();
            GlobalValues.ylist = new List<double>();
            /*switch (GlobalValues.SyncWay)
            {
                case Sync.Semaphore:
                {
                    label14.Text = "Способ синхронизации: семафоры";
                    break;
                }
                case Sync.Event:
                {
                    label14.Text = "Способ синхронизации: события";
                    break;
                }
                case Sync.Mutex:
                {
                    label14.Text = "Способ синхронизации: мьютексы";
                    break;
                }
            }*/

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
            chart1.Series["real"].MarkerStyle = MarkerStyle.Circle;
            chart2.Series["real"].MarkerStyle = MarkerStyle.Circle;
            chart3.Series["real"].MarkerStyle = MarkerStyle.Circle;
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

        private void goWork()
        {
            Console.WriteLine("writing in console");
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        /*private void cubicInterpolation(GlobalValues.SplineBox spline, Chart chart)
        {
            CubicSpline cSpline = new CubicSpline();
            cSpline.BuildSpline(GlobalValues.X, GlobalValues.Y, GlobalValues.X.Length);
            /*
            List<double> listx = new List<double>();
            List<double> listy = new List<double>();
             *
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
        }*/

        /*private void qInterpolation(GlobalValues.SplineBox spline, Chart chart)
        {
            QSpline qSpline = new QSpline();
            qSpline.BuildSpline(GlobalValues.X, GlobalValues.Y, GlobalValues.X.Length);
            /*
            List<double> listx = new List<double>();
            List<double> listy = new List<double>();
             *
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
        }*/

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
            cycleCount = 0;
            if(checkBox1.Checked == false)
                StartThreads();
            else
            {
                label10.Visible = true;
                label10.Text = "Нажмите кнопку \"Следующий шаг\"";
            }
            /*
            this.checkSplines(spline1,chart1);
            this.checkSplines(spline2,chart2);
            this.checkSplines(spline3,chart3);
             * */
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void checkSplines(GlobalValues.SplineBox spline, Chart chart)
        {
            spline.dict = new Dictionary<double, double>();
            GlobalValues.xlist = GlobalValues.X.ToList<double>();
            GlobalValues.ylist = GlobalValues.Y.ToList<double>();

            double step = 0.3;

            if(spline.POWER == 1)
            {
                spline.dict = new Dictionary<double, double>();
                LinearSplineInterpolation splineInterpol = new LinearSplineInterpolation();
                splineInterpol.Init(GlobalValues.xlist, GlobalValues.ylist);
                for (double i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i += step)
                {
                    // spline.listX.Add(i);
                    spline.dict.Add(i, splineInterpol.Interpolate(i));
                    //spline.listY.Add(splineInterpol.Interpolate(i));
                }
            } else if(spline.POWER == 2)
            {
                spline.dict = new Dictionary<double, double>();

                QuadraticSpline quadraticSplineInterpol = new QuadraticSpline();
                quadraticSplineInterpol.Init(GlobalValues.xlist, GlobalValues.ylist);
                for (double i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i += step)
                {
                    //spline.listX.Add(i);
                    //spline.listY.Add(quadraticSplineInterpol.Interpolate(i));
                    spline.dict.Add(i, quadraticSplineInterpol.Interpolate(i));
                }
            }
            else if(spline.POWER > 2 && spline.POWER<5)
            {
                spline.dict = new Dictionary<double, double>();

                CubicSplineInterpolation cubicSplineInterpol = new CubicSplineInterpolation();
                cubicSplineInterpol.Init(GlobalValues.xlist, GlobalValues.ylist);
                for (double i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i += step)
                {
                    //spline.listX.Add(i);
                    //spline.listY.Add(cubicSplineInterpol.Interpolate(i));
                    spline.dict.Add(i, cubicSplineInterpol.Interpolate(i));
                }
            }
            else if (spline.POWER >=5)
            {
                spline.dict = new Dictionary<double, double>();

                BulirschStoerRationalInterpolation nevilleInterpol = new BulirschStoerRationalInterpolation(GlobalValues.X, GlobalValues.Y);
                for (double i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i += step)
                {
                    spline.dict.Add(i, nevilleInterpol.Interpolate(i));
                }
            }
            /*
            switch(spline.POWER)
            {
                case 1:
                    LinearSplineInterpolation splineInterpol = new LinearSplineInterpolation();
                    splineInterpol.Init(GlobalValues.xlist, GlobalValues.ylist);
                    for (double i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i+=step)
                    {
                       // spline.listX.Add(i);
                        spline.dict.Add(i, splineInterpol.Interpolate(i));
                        //spline.listY.Add(splineInterpol.Interpolate(i));
                    }
                    break;
                case 2:
                    //MessageBox.Show("do not work for now");
                    QuadraticSpline quadraticSplineInterpol = new QuadraticSpline();
                    quadraticSplineInterpol.Init(GlobalValues.xlist, GlobalValues.ylist);
                    for (double i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i += step)
                    {
                        //spline.listX.Add(i);
                        //spline.listY.Add(quadraticSplineInterpol.Interpolate(i));
                        spline.dict.Add(i, quadraticSplineInterpol.Interpolate(i));
                    }
                    break;
                case 3:
                    CubicSplineInterpolation cubicSplineInterpol = new CubicSplineInterpolation();
                    cubicSplineInterpol.Init(GlobalValues.xlist, GlobalValues.ylist);
                    for (double i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i += step)
                    {
                        //spline.listX.Add(i);
                        //spline.listY.Add(cubicSplineInterpol.Interpolate(i));
                        spline.dict.Add(i, cubicSplineInterpol.Interpolate(i));
                    }
                    break;
                default:
                    CubicSplineInterpolation cubicSplineInterpol1 = new CubicSplineInterpolation();
                    QuadraticSpline quadraticSplineInterpol1 = new QuadraticSpline();
                    //MathNet.Numerics.Interpolation.NevillePolynomialInterpolation
                    BulirschStoerRationalInterpolation nevilleInterpol = new BulirschStoerRationalInterpolation(GlobalValues.X, GlobalValues.Y);
                    NevillePolynomialInterpolation logLinearInterpol = new NevillePolynomialInterpolation(GlobalValues.X, GlobalValues.Y);
                    cubicSplineInterpol1.Init(GlobalValues.xlist, GlobalValues.ylist);
                    quadraticSplineInterpol1.Init(GlobalValues.xlist, GlobalValues.ylist);
                    for (double i = Convert.ToInt32(GlobalValues.X[0]); i < GlobalValues.X[GlobalValues.X.Length - 1]; i += step)
                    {
                        //spline.listX.Add(i);
                        //spline.listY.Add(cubicSplineInterpol1.Interpolate(i));
                       // if (GlobalValues.xlist.Contains(i)&&spline.POWER%2!=0)

                        ///if (spline.POWER % 2 != 0)
                        //    spline.dict.Add(i, cubicSplineInterpol1.Interpolate(i + spline.POWER * 1.5));
                        //else spline.dict.Add(i, quadraticSplineInterpol1.Interpolate(i + spline.POWER * 1.5));

                        ///spline.dict.Add(i, nevilleInterpol.Interpolate(i));
                        //spline.dict.Add(i, logLinearInterpol.Interpolate(i));
                        //else if (GlobalValues.xlist.Contains(i) && spline.POWER%2==0)
                         //   spline.dict.Add(i, quadraticSplineInterpol1.Interpolate(i+spline.POWER*1.5));
                    }
                    break;
            }*/
        }
        private void drawChart(GlobalValues.SplineBox spline, Chart chart)
        {
            //List<double> ll = spline.dict.Keys.ToList();
            //Console.WriteLine(spline.dict);
            //ll.Sort();
            try
            {
                foreach (var key in spline.dict.Keys)
                {
                    chart.Series["interpolated4"].Points.AddXY(key, spline.dict[key]);
                }
            }
            catch (KeyNotFoundException e)
            {
                MessageBox.Show("Key not found exception. We will skip this chart");
            }
            catch (InvalidOperationException e)
            {
                MessageBox.Show("Invalid operation exception. We will skip this chart");
            }
            chart.Series["interpolated4"].MarkerStyle = MarkerStyle.Cross;
            chart.Series["interpolated4"].ChartType = SeriesChartType.Spline;
            //spline.dict = new Dictionary<double,double>();
            /*for (int i = 0; i < ll.Count; i++)
            {
                //chart.Series["interpolated4"].Points.AddXY(spline.listX[i], spline.listY[i]);
                chart.Series["interpolated4"].Points.AddXY(i, spline.dict[i]);
            }
            chart.Series["interpolated4"].ChartType = SeriesChartType.Spline;*/
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void StartThreads()
        {
            ProcessTimeStopwatch processTimeStopwatch = new ProcessTimeStopwatch();
            processTimeStopwatch.Start();
            GlobalValues.isRunning = true;
            foreach (var spline1Priority in GlobalValues.ThreadPriorities)
            {
                foreach (var spline2Priority in GlobalValues.ThreadPriorities)
                {
                    foreach (var spline3Priority in GlobalValues.ThreadPriorities)
                    {
                        //GlobalValues.EnterCriticalSection(GlobalValues.LockObject);
                        try
                        {
                            cycleCount++;
                            Logger.Current.WriteLine("Цикл номер: {0}", cycleCount);
                            StartNewThread(spline1, chart1, spline1Priority);
                            StartNewThread(spline2, chart2, spline2Priority);
                            StartNewThread(spline3, chart3, spline3Priority);
                        }
                        catch (NullReferenceException e)
                        {
                            MessageBox.Show("NRE occures. Please restart the program");
                        }
                        finally
                        {
                          //  GlobalValues.LeaveCriticalSection(GlobalValues.LockObject);
                        }
                        Thread.Sleep(10);
                        GlobalValues.SevEvent();
                    }
                }
            }
            processTimeStopwatch.Stop();
            Logger.Current.WriteLine();
            Logger.Current.WriteLine("Время работы процесса: {0}", processTimeStopwatch.Elapsed);
            Console.WriteLine(processTimeStopwatch.Elapsed);
            GlobalValues.isRunning = false;
            drawChart(spline1, chart1);
            drawChart(spline2, chart2);
            drawChart(spline3, chart3);

        }

        [STAThread]
        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void StartNewThread(GlobalValues.SplineBox spline, Chart chart, ThreadPriority threadPriority)
        {
            Action action = () => Worker(spline, chart, threadPriority);
            ThreadStart threadStart = new ThreadStart(action);
            IntPtr h1Handle = new IntPtr();
            uint dwThread;
            SECURITY_ATTRIBUTES secAttr = new SECURITY_ATTRIBUTES();
            h1Handle = (IntPtr) CreateThread(ref secAttr, 0, threadStart, 0, SuspendThread(h1Handle), out dwThread);


            ResumeThread(h1Handle);
            //Thread workThread = new Thread(threadStart);
            //workThread.Priority = threadPriority;
            //workThread.Start();
            //WaitForSingleObject(h1Handle, 0xFFFFFFFF);
            //CloseHandle(h1Handle);
        }

        static IntPtr getSemaphoreCount(IntPtr sem, int maxCOunt)
        {
            IntPtr currentValue = IntPtr.Zero;
            IntPtr[] arr = new IntPtr[]{sem};
            if (ReleaseSemaphore(sem, 1, currentValue))
            {
                WaitForMultipleObjects(1, arr, true, 1);
            }
            return currentValue;
        }


        private static void Worker(GlobalValues.SplineBox spline, Chart chart, ThreadPriority threadPriority)
        {
            ThreadTimeStopwatch threadTimeStopwatch = new ThreadTimeStopwatch();
            threadTimeStopwatch.Start();
            switch (GlobalValues.SyncWay)
            {
                case Sync.Semaphore:
                {
                    c1 = getSemaphoreCount(_semaphore, 1);
                    Thread.Sleep(10);
                    WaitForSingleObject(_semaphore, 0xFFFFFFFF);
                    Thread.Sleep(10);
                    c2 = getSemaphoreCount(_semaphore, 1);
                    MakeChanges(spline, chart, threadPriority, threadTimeStopwatch);
                    ReleaseSemaphore(_semaphore, 1, (IntPtr)null);
                    break;
                }
                case Sync.Mutex:
                {
                    WaitForSingleObject(_mutex, 0xFFFFFFFF);
                    MakeChanges(spline, chart, threadPriority, threadTimeStopwatch);
                    ReleaseMutex(_mutex);
                    break;
                }
                case Sync.Event:
                {

                    WaitForSingleObject(_event, 0xFFFFFFFF);
                    ResetEvent(_event);
                    MakeChanges(spline, chart, threadPriority, threadTimeStopwatch);
                    SetEvent(_event);
                    break;
                }
                default:
                {
                    GlobalValues.EnterCriticalSection(GlobalValues.LockObject);
                    GlobalValues.LeaveCriticalSection(GlobalValues.LockObject);
                    break;
                }

            }

            log();
            /*
            try
            {
                makeChanges(spline, chart, threadPriority, threadTimeStopwatch);
            }
            finally
            {
                GlobalValues.WaitForSingleObject();
                GlobalValues.SevEvent();
            }*/
            Console.WriteLine(c1);
            Console.WriteLine(c2);
        }

        

        private static void MakeChanges(GlobalValues.SplineBox spline, Chart chart, ThreadPriority threadPriority,
            ThreadTimeStopwatch threadTimeStopwatch)
        {
            checkSplines(spline, chart);

            threadTimeStopwatch.Stop();

            WriteResultSummary(spline, threadPriority, threadTimeStopwatch.Elapsed);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void WriteResultSummary(GlobalValues.SplineBox spline, ThreadPriority threadPriority, TimeSpan elapsed)
        {
            Logger.Current.WriteLine(string.Empty);

            string summary = string.Format("Сплайн степени {0}; приоритет: {1}; время выполнения: {2} ", spline.POWER, threadPriority, elapsed);
            Logger.Current.WriteLine(summary);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
                button5.Enabled = true;
            if (checkBox1.Checked == false)
                button5.Enabled = false;

            // Even the hills Have Eyes
            cSpline1 = new GlobalValues.SplineBox(spline1.POWER);
            cSpline2 = new GlobalValues.SplineBox(spline2.POWER);
            cSpline3 = new GlobalValues.SplineBox(spline3.POWER);
            checkSplines(cSpline1, chart4);
            checkSplines(cSpline2, chart4);
            checkSplines(cSpline3, chart4);
            // Even the hills Have Eyes
        }


        int j_global = 0;
        int K_global = 0;
        ProcessTimeStopwatch processTimeStopwatch_spec = new ProcessTimeStopwatch();

        private static void log()
        {
            c2 = new IntPtr(1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (GlobalValues.isRunning == false)
            {

                processTimeStopwatch_spec.Start();
                GlobalValues.isRunning = true;
            }

            stepRunThreads(i_global, j_global, K_global);

            if (K_global < GlobalValues.ThreadPriorities.Length-1)
            {
                K_global++;
            }
            else if (j_global < GlobalValues.ThreadPriorities.Length-1)
            {
                j_global++;
                K_global = 0;
            }
            else if (i_global < GlobalValues.ThreadPriorities.Length-1)
            {
                i_global++;
                K_global = 0;
                j_global = 0;
            }
            else
            {
                processTimeStopwatch_spec.Stop();
                Logger.Current.WriteLine();
                Logger.Current.WriteLine("Время работы процесса: {0}", processTimeStopwatch_spec.Elapsed);
                Console.WriteLine(processTimeStopwatch_spec.Elapsed);
                GlobalValues.isRunning = false;
                label10.Text = "Итерирование завершено";
            }

        }

        private void stepRunThreads(int i, int j, int k)
        {
            label11.Visible = true;
            label12.Visible = true;
            label13.Visible = true;
            label11.Text = i.ToString();
            label12.Text = j.ToString();
            label13.Text = k.ToString();
            clearChart();
            realChart();
            try
            {
                GlobalValues.EnterCriticalSection(GlobalValues.LockObject);
                cycleCount++;
                Logger.Current.WriteLine("Цикл номер: {0}", cycleCount);

                StartNewThread(spline1, chart4, GlobalValues.ThreadPriorities[i]);
                StartNewThread(spline2, chart4, GlobalValues.ThreadPriorities[j]);
                StartNewThread(spline3, chart4, GlobalValues.ThreadPriorities[k]);

            }
            finally
            {
                GlobalValues.LeaveCriticalSection(GlobalValues.LockObject);
            }
            Thread.Sleep(10);
            GlobalValues.SevEvent();
            drawChart(cSpline1, chart1);
            drawChart(cSpline2, chart2);
            drawChart(cSpline3, chart3);
            label11.Text = ("Поток со сплайном степени " + spline1.POWER + ", приоритет: " + GlobalValues.ThreadPriorities[i]);
            label12.Text = ("Поток со сплайном степени " + spline2.POWER + ", приоритет: " + GlobalValues.ThreadPriorities[j]);
            label13.Text = ("Поток со сплайном степени " + spline3.POWER + ", приоритет: " + GlobalValues.ThreadPriorities[k]);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.ShowDialog();
            /*switch (GlobalValues.SyncWay)
            {
                case Sync.Semaphore:
                {
                    label14.Text = "Способ синхронизации: семафоры";
                    break;
                }
                case Sync.Event:
                {
                    label14.Text = "Способ синхронизации: события";
                    break;
                }
                case Sync.Mutex:
                {
                    label14.Text = "Способ синхронизации: мьютексы";
                    break;
                }
            }*/
        }
    }
}
