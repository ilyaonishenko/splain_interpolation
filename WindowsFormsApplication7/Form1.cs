using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

        private double[] N;

        StreamReader reader;

        Commons.SplineBox spline1;
        Commons.SplineBox spline2;
        Commons.SplineBox spline3;
        private Commons.SplineBox cSpline1;
        private Commons.SplineBox cSpline2;
        private Commons.SplineBox cSpline3;

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

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadFile(IntPtr hFile, 
            [Out] double[] lpBuffer,
            uint nNumberOfBytesToRead, 
            out uint lpNumberOfBytesRead, 
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadFile(IntPtr hFile,
            [Out] int lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        const int STD_OUTPUT_HANDLE = -11;
        const int STD_INPUT_HANDLE = -10;
        const int STD_ERROR_HANDLE = -12;


        [DllImport("kernel32.dll", EntryPoint = "GetStartupInfoW")]
        static extern void GetStartupInfo(out STARTUPINFO lpStartupInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("kernel32.dll")]
        static extern bool ConnectNamedPipe(IntPtr hNamedPipe,
            [In] ref System.Threading.NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateNamedPipe(string lpName,
            uint dwOpenMode,
            uint dwPipeMode,
            uint nMaxInstances,
            uint nOutBufferSize,
            uint nInBufferSize,
            uint nDefaultTimeOut,
            IntPtr lpSecurityAttributes);

        [DllImport("kernel32.dll")]
        static extern bool CreateProcess(string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        static extern uint SleepEx(uint dwMilliseconds, bool bAlertable);

        //ТАЙМЕРЫ!
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        public delegate void TimerCompleteDelegate();

        [DllImport("kernel32.dll")]
        static extern bool SetWaitableTimer(IntPtr hTimer,
            [In] ref long pDueTime,
            int lPeriod,
            TimerCompleteDelegate pfnCompletionRoutine,
            IntPtr lpArgToCompletionRoutine, bool fResume);

        [DllImport("kernel32.dll")]
        static extern IntPtr FindFirstChangeNotification(string lpPathName,
            bool bWatchSubtree, 
            uint dwNotifyFilter);

        [DllImport("kernel32.dll")]
        static extern bool FindNextChangeNotification(IntPtr hChangeHandle);

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }


        private static IntPtr _semaphore;
        private static IntPtr _mutex;
        private static IntPtr _event;
        private static IntPtr c1;
        private static  IntPtr c2;

        private static STARTUPINFO startupInfo;
        private static PROCESS_INFORMATION processInfo;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 1)]
        private static IntPtr lenPipe;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 1)]
        private static IntPtr arrayXPipe;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 1)]
        private static IntPtr arrayYPipe;
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 1)]
        private static IntPtr syncWayPipe;

        private const string Path = "C:\\Users\\veryoldbarny\\input\\input.txt";
        private static SECURITY_ATTRIBUTES _secAttr;

        public Form1()
        {
            InitializeComponent();
            //button3.Enabled = false;
            button4.Enabled = false;
            Commons.LogToFile = true;
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
            //Form2 f2 = new Form2();
            //f2.ShowDialog();
            c1 = IntPtr.Zero;
            c2 = new IntPtr(1);

            //lab 5
            // disable form 2 appereance and
            button3.Visible = false;
            button6.Visible = false;
            label3.Visible = false;
            label2.Visible = false;
            textBox2.Visible = false;
            startupInfo = new STARTUPINFO();
            processInfo = new PROCESS_INFORMATION();
            _secAttr = new SECURITY_ATTRIBUTES();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
            Commons.xlist = new List<double>();
            Commons.ylist = new List<double>();
            /*switch (Commons.SyncWay)
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
            uint dwThread;
            IntPtr fileChangesThread = new IntPtr();
            fileChangesThread = (IntPtr)CreateThread(ref _secAttr, 0, LookForChanges, 0, SuspendThread(fileChangesThread), out dwThread);
            ResumeThread(fileChangesThread);
        }

        private static void CheckFileExist()
        {

            if (!File.Exists(Path))
            {
                DialogResult result = MessageBox.Show("Файл не найден.\nЖдать появления файла?", "Сообщение", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    while (true)
                    {
                        if (File.Exists(Path))
                        {
                            break;
                        }
                        Thread.Sleep(5000);
                    }
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        private static void LookForChanges()
        {
            
            var changeNotification = FindFirstChangeNotification("C:\\Users\\veryoldbarny\\input\\", true, 0x00000010);

            while (true)
            {
                CheckFileExist();
                var singleObject = WaitForSingleObject(changeNotification, 1000);

                FindNextChangeNotification(changeNotification);
                if (singleObject != 0x00000000) continue;
                DialogResult result = MessageBox.Show("Файл был изменен.\nНачать работу с новыми данными?", "Message", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    CreatePipe();
                    /*if (Commons.isRunning)
                    {
                        
                    }*/
                }
                //MessageBox.Show("Файл был изменен.");
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private void ReadDataFromPipe()
        {
            NativeOverlapped zz = new NativeOverlapped();
            ConnectNamedPipe(lenPipe, ref zz);
            ConnectNamedPipe(arrayXPipe, ref zz);
            ConnectNamedPipe(arrayYPipe, ref zz);
            ConnectNamedPipe(syncWayPipe, ref zz);
            uint q1 = 0;
            N = new double[1];

            ReadFile(lenPipe, N, 64, out q1, IntPtr.Zero);
            Console.WriteLine(N[0]);
            var n = (int)N[0];
            Commons.SIZE = n;
            Console.WriteLine("N is " + n);
            var arrayX = new double[n];
            var arrayY = new double[n];

            ReadFile(arrayXPipe, arrayX, 128, out q1, IntPtr.Zero);
            ReadFile(arrayYPipe, arrayY, 128, out q1, IntPtr.Zero);

            ReadFile(syncWayPipe, N, 64, out q1, IntPtr.Zero);
            
            var syncWay = N[0];
            Commons.SyncWay = GetValue((int)syncWay);
            Commons.X = arrayX;
            Commons.Y = arrayY;
            Console.WriteLine("ArrayX: "+arrayX.Length);
            foreach (var dob in arrayX)
            {
                Console.Write(dob+"  ");
            }
            Console.WriteLine("ArrayY: "+arrayY.Length);
            foreach (var dob in arrayY)
            {
                Console.Write(dob + "  ");
            }
            Console.WriteLine();
            Console.WriteLine("SyncWay: "+Commons.SyncWay);
        }


        private static void CreatePipe()
        {

            String pathToStarter = "C:\\Users\\veryoldbarny\\Starter.exe";
            lenPipe = CreateNamedPipe("\\\\.\\pipe\\LengthPipe", 0x00000003, 0x00000004 | 0x00000002 | 0x00000000, 1, 512, 512, 5000, IntPtr.Zero);
            arrayXPipe = CreateNamedPipe("\\\\.\\pipe\\ArrayXPipe", 0x00000003, 0x00000004 | 0x00000002 | 0x00000000, 1, 512, 512, 5000, IntPtr.Zero);
            arrayYPipe = CreateNamedPipe("\\\\.\\pipe\\ArrayYPipe", 0x00000003, 0x00000004 | 0x00000002 | 0x00000000, 1, 512, 512, 5000, IntPtr.Zero);
            syncWayPipe = CreateNamedPipe("\\\\.\\pipe\\SyncWayPipe", 0x00000003, 0x00000004 | 0x00000002 | 0x00000000, 1, 512, 512, 5000, IntPtr.Zero);
            //System.Threading.NativeOverlapped zz = new System.Threading.NativeOverlapped();

            //режимокрытияканала
            //режимработыканала
            //максимальноеколичествореализаций
            //размервыходногобуфера
            //размервходногобуфера
            //времяожиданиявмс
            //адресаттрибутовзащиты

            // PIPE_ACCESS_DUPLEX = 0x00000003
            // PIPE_TYPE_MESSAGE = 0x00000004
            //PIPE_READMODE_MESSAGE = 0x00000002
            //PIPE_WAIT = 0x00000000
            //PIPE_NOWAIT = 0x00000001
            CreateProcess(pathToStarter, null, IntPtr.Zero, IntPtr.Zero, true, 0, IntPtr.Zero, null, ref startupInfo, out processInfo);
           // Form2 f2 = new Form2();
           // f2.ShowDialog();
        }

        private static Sync GetValue(int syncWay)
        {
            if (syncWay == 1)
                return Sync.Semaphore;
            if (syncWay == 2)
            {
                return Sync.Mutex;
            }
            if (syncWay == 3)
            {
                return Sync.Event;
            }
            return 0;
        }

        public void ReadPoints()
        {
            
            /*
            reader = new StreamReader("C:\\Users\\veryoldbarny\\input.txt");

            //reader = new StreamReader("C:\\Users\\veryoldbarny\\Documents\\input.txt");
            // Номер строки с первой ошибкой, если такая имеется
            error_line = 0;

            Commons.SIZE = Convert.ToInt32(reader.ReadLine());
            int sync = Convert.ToInt32(reader.ReadLine());
            Commons.SyncWay = GetValue(sync);
            Commons.X = new double[Commons.SIZE];
            for (int i = 0; i < Commons.SIZE; i++) Commons.X[i] = 0.0;
            Commons.Y = new double[Commons.SIZE];
            for (int i = 0; i < Commons.SIZE; i++) Commons.Y[i] = 0.0;

            // Проверка на корректность считанных точек
            string line = "";
            string[] points;

            for (int i = 0; i < Commons.SIZE; i++)
            {
                line = reader.ReadLine();
                points = line.Split();
                error_line++;
                Commons.X[i] = Convert.ToDouble(points[0]);
                Commons.Y[i] = Convert.ToDouble(points[1]);
            }

            // Сортировка точек в порядке возрастания по x
            double x_change = 0, y_change = 0;
            for (int i = 0; i < Commons.SIZE - 1; i++)
                for (int j = 0; j < Commons.SIZE - i - 1; j++)
                    if (Commons.X[j] > Commons.X[j + 1])
                    {
                        x_change = Commons.X[j];
                        Commons.X[j] = Commons.X[j + 1];
                        Commons.X[j + 1] = x_change;
                        y_change = Commons.Y[j];
                        Commons.Y[j] = Commons.Y[j + 1];
                        Commons.Y[j + 1] = y_change;
                    }

            // Оповещение ошибки в том случае, если несколько точек имеют одинаковое значение координаты х
            x_same = 0;
            for (int i = 1; i < Commons.SIZE; i++)
                if (Commons.X[i] == Commons.X[i - 1])
                {
                    same = true;
                    x_same = Commons.X[i];
                }
            reader.Close();
             */
             
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void realChart()
        {
            for (int i = 0; i < Commons.X.Length; i++)
            {
                chart1.Series["real"].Points.AddXY(Commons.X[i], Commons.Y[i]);
                chart2.Series["real"].Points.AddXY(Commons.X[i], Commons.Y[i]);
                chart3.Series["real"].Points.AddXY(Commons.X[i], Commons.Y[i]);
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
                spline1 = new Commons.SplineBox(Int32.Parse(textBox1.Text));
                spline2 = new Commons.SplineBox(Int32.Parse(textBox4.Text));
                spline3 = new Commons.SplineBox(Int32.Parse(textBox3.Text));

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
            /*
            int naturalDotsSize = Int32.Parse(textBox2.Text);
            Generator gen = new Generator();
            gen.generate(naturalDotsSize, 1, 3);
             * */
        }

        private void goWork()
        {
            Console.WriteLine("writing in console");
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        /*private void cubicInterpolation(Commons.SplineBox spline, Chart chart)
        {
            CubicSpline cSpline = new CubicSpline();
            cSpline.BuildSpline(Commons.X, Commons.Y, Commons.X.Length);
            /*
            List<double> listx = new List<double>();
            List<double> listy = new List<double>();
             *
            for (int i = Convert.ToInt32(Commons.X[0]); i < Commons.X[Commons.X.Length - 1]; i++)
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

        /*private void qInterpolation(Commons.SplineBox spline, Chart chart)
        {
            QSpline qSpline = new QSpline();
            qSpline.BuildSpline(Commons.X, Commons.Y, Commons.X.Length);
            /*
            List<double> listx = new List<double>();
            List<double> listy = new List<double>();
             *
            for (int i = Convert.ToInt32(Commons.X[0]); i < Commons.X[Commons.X.Length - 1]; i++)
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
        private static void checkSplines(Commons.SplineBox spline, Chart chart)
        {
            spline.dict = new Dictionary<double, double>();
            Commons.xlist = Commons.X.ToList<double>();
            Commons.ylist = Commons.Y.ToList<double>();

            double step = 0.3;

            if(spline.POWER == 1)
            {
                spline.dict = new Dictionary<double, double>();
                LinearSplineInterpolation splineInterpol = new LinearSplineInterpolation();
                splineInterpol.Init(Commons.xlist, Commons.ylist);
                for (double i = Convert.ToInt32(Commons.X[0]); i < Commons.X[Commons.X.Length - 1]; i += step)
                {
                    // spline.listX.Add(i);
                    spline.dict.Add(i, splineInterpol.Interpolate(i));
                    //spline.listY.Add(splineInterpol.Interpolate(i));
                }
            } else if(spline.POWER == 2)
            {
                spline.dict = new Dictionary<double, double>();

                QuadraticSpline quadraticSplineInterpol = new QuadraticSpline();
                quadraticSplineInterpol.Init(Commons.xlist, Commons.ylist);
                for (double i = Convert.ToInt32(Commons.X[0]); i < Commons.X[Commons.X.Length - 1]; i += step)
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
                cubicSplineInterpol.Init(Commons.xlist, Commons.ylist);
                for (double i = Convert.ToInt32(Commons.X[0]); i < Commons.X[Commons.X.Length - 1]; i += step)
                {
                    //spline.listX.Add(i);
                    //spline.listY.Add(cubicSplineInterpol.Interpolate(i));
                    spline.dict.Add(i, cubicSplineInterpol.Interpolate(i));
                }
            }
            else if (spline.POWER >=5)
            {
                spline.dict = new Dictionary<double, double>();

                BulirschStoerRationalInterpolation nevilleInterpol = new BulirschStoerRationalInterpolation(Commons.X, Commons.Y);
                for (double i = Convert.ToInt32(Commons.X[0]); i < Commons.X[Commons.X.Length - 1]; i += step)
                {
                    spline.dict.Add(i, nevilleInterpol.Interpolate(i));
                }
            }
            /*
            switch(spline.POWER)
            {
                case 1:
                    LinearSplineInterpolation splineInterpol = new LinearSplineInterpolation();
                    splineInterpol.Init(Commons.xlist, Commons.ylist);
                    for (double i = Convert.ToInt32(Commons.X[0]); i < Commons.X[Commons.X.Length - 1]; i+=step)
                    {
                       // spline.listX.Add(i);
                        spline.dict.Add(i, splineInterpol.Interpolate(i));
                        //spline.listY.Add(splineInterpol.Interpolate(i));
                    }
                    break;
                case 2:
                    //MessageBox.Show("do not work for now");
                    QuadraticSpline quadraticSplineInterpol = new QuadraticSpline();
                    quadraticSplineInterpol.Init(Commons.xlist, Commons.ylist);
                    for (double i = Convert.ToInt32(Commons.X[0]); i < Commons.X[Commons.X.Length - 1]; i += step)
                    {
                        //spline.listX.Add(i);
                        //spline.listY.Add(quadraticSplineInterpol.Interpolate(i));
                        spline.dict.Add(i, quadraticSplineInterpol.Interpolate(i));
                    }
                    break;
                case 3:
                    CubicSplineInterpolation cubicSplineInterpol = new CubicSplineInterpolation();
                    cubicSplineInterpol.Init(Commons.xlist, Commons.ylist);
                    for (double i = Convert.ToInt32(Commons.X[0]); i < Commons.X[Commons.X.Length - 1]; i += step)
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
                    BulirschStoerRationalInterpolation nevilleInterpol = new BulirschStoerRationalInterpolation(Commons.X, Commons.Y);
                    NevillePolynomialInterpolation logLinearInterpol = new NevillePolynomialInterpolation(Commons.X, Commons.Y);
                    cubicSplineInterpol1.Init(Commons.xlist, Commons.ylist);
                    quadraticSplineInterpol1.Init(Commons.xlist, Commons.ylist);
                    for (double i = Convert.ToInt32(Commons.X[0]); i < Commons.X[Commons.X.Length - 1]; i += step)
                    {
                        //spline.listX.Add(i);
                        //spline.listY.Add(cubicSplineInterpol1.Interpolate(i));
                       // if (Commons.xlist.Contains(i)&&spline.POWER%2!=0)

                        ///if (spline.POWER % 2 != 0)
                        //    spline.dict.Add(i, cubicSplineInterpol1.Interpolate(i + spline.POWER * 1.5));
                        //else spline.dict.Add(i, quadraticSplineInterpol1.Interpolate(i + spline.POWER * 1.5));

                        ///spline.dict.Add(i, nevilleInterpol.Interpolate(i));
                        //spline.dict.Add(i, logLinearInterpol.Interpolate(i));
                        //else if (Commons.xlist.Contains(i) && spline.POWER%2==0)
                         //   spline.dict.Add(i, quadraticSplineInterpol1.Interpolate(i+spline.POWER*1.5));
                    }
                    break;
            }*/
        }
        private void drawChart(Commons.SplineBox spline, Chart chart)
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
            Commons.isRunning = true;
            //foreach (var spline1Priority in Commons.ThreadPriorities)
            //{
            //    foreach (var spline2Priority in Commons.ThreadPriorities)
            //    {
            //        foreach (var spline3Priority in Commons.ThreadPriorities)
            //        {
            //            Commons.EnterCriticalSection(Commons.LockObject);
            //            try
            //            {
            //               cycleCount++;
            //                Logger.Current.WriteLine("Цикл номер: {0}", cycleCount);
                            StartNewThread(spline1, chart1, Commons.ThreadPriorities[3]);
                            StartNewThread(spline2, chart2, Commons.ThreadPriorities[3]);
                            StartNewThread(spline3, chart3, Commons.ThreadPriorities[3]);
            MakeChanges(spline1, chart1, Commons.ThreadPriorities[3], null);
            MakeChanges(spline2, chart2, Commons.ThreadPriorities[3], null);
            MakeChanges(spline3, chart3, Commons.ThreadPriorities[3], null);
            Thread.Sleep(100);
            //            }
            //            catch (NullReferenceException e)
            //            {
            //                MessageBox.Show("NRE occures. Please restart the program");
            //            }
            //            finally
            ///            {
              //              Commons.LeaveCriticalSection(Commons.LockObject);
             //           }
              //          Thread.Sleep(100);
              //          Commons.SevEvent();
           //         }
           //     }
           // }
            processTimeStopwatch.Stop();
            Logger.Current.WriteLine();
            //Logger.Current.WriteLine("Время работы процесса: {0}", processTimeStopwatch.Elapsed);
            //Console.WriteLine(processTimeStopwatch.Elapsed);
            Commons.isRunning = false;
            drawChart(spline1, chart1);
            drawChart(spline2, chart2);
            drawChart(spline3, chart3);

        }

        [STAThread]
        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void StartNewThread(Commons.SplineBox spline, Chart chart, ThreadPriority threadPriority)
        {
            Action action = () => Worker(spline, chart, threadPriority);
            var threadStart = new ThreadStart(action);
            var h1Handle = new IntPtr();
            uint dwThread;
            
            h1Handle = (IntPtr) CreateThread(ref _secAttr, 0, threadStart, 0, SuspendThread(h1Handle), out dwThread);

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


        private static void Worker(Commons.SplineBox spline, Chart chart, ThreadPriority threadPriority)
        {

            ThreadTimeStopwatch threadTimeStopwatch = new ThreadTimeStopwatch();
            threadTimeStopwatch.Start();
            
            switch (Commons.SyncWay)
            {
                case Sync.Semaphore:
                {
                    //c1 = getSemaphoreCount(_semaphore, 1);
                    //Thread.Sleep(10);
                    //WaitForSingleObject(_semaphore, 0xFFFFFFFF);
                    //Thread.Sleep(10);
                    //c2 = getSemaphoreCount(_semaphore, 1);
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
                    Commons.EnterCriticalSection(Commons.LockObject);
                    Commons.LeaveCriticalSection(Commons.LockObject);
                    break;
                }

            }
            //log();
            /*
            try
            {
                makeChanges(spline, chart, threadPriority, threadTimeStopwatch);
            }
            finally
            {
                Commons.WaitForSingleObject();
                Commons.SevEvent();
            }*/
            //Console.WriteLine(c1);
            //Console.WriteLine(c2);
        }

        

        private static void MakeChanges(Commons.SplineBox spline, Chart chart, ThreadPriority threadPriority,
            ThreadTimeStopwatch threadTimeStopwatch)
        {
            checkSplines(spline, chart);
            if (threadTimeStopwatch != null)
            {

                threadTimeStopwatch.Stop();

                WriteResultSummary(spline, threadPriority, threadTimeStopwatch.Elapsed);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void WriteResultSummary(Commons.SplineBox spline, ThreadPriority threadPriority, TimeSpan elapsed)
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
            cSpline1 = new Commons.SplineBox(spline1.POWER);
            cSpline2 = new Commons.SplineBox(spline2.POWER);
            cSpline3 = new Commons.SplineBox(spline3.POWER);
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
            if (Commons.isRunning == false)
            {

                processTimeStopwatch_spec.Start();
                Commons.isRunning = true;
            }

            stepRunThreads(i_global, j_global, K_global);

            if (K_global < Commons.ThreadPriorities.Length-1)
            {
                K_global++;
            }
            else if (j_global < Commons.ThreadPriorities.Length-1)
            {
                j_global++;
                K_global = 0;
            }
            else if (i_global < Commons.ThreadPriorities.Length-1)
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
                Commons.isRunning = false;
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
                Commons.EnterCriticalSection(Commons.LockObject);
                cycleCount++;
                Logger.Current.WriteLine("Цикл номер: {0}", cycleCount);

                StartNewThread(spline1, chart4, Commons.ThreadPriorities[i]);
                StartNewThread(spline2, chart4, Commons.ThreadPriorities[j]);
                StartNewThread(spline3, chart4, Commons.ThreadPriorities[k]);

            }
            finally
            {
                Commons.LeaveCriticalSection(Commons.LockObject);
            }
            //Thread.Sleep(10);
            Commons.SevEvent();
            drawChart(cSpline1, chart1);
            drawChart(cSpline2, chart2);
            drawChart(cSpline3, chart3);
            label11.Text = ("Поток со сплайном степени " + spline1.POWER + ", приоритет: " + Commons.ThreadPriorities[i]);
            label12.Text = ("Поток со сплайном степени " + spline2.POWER + ", приоритет: " + Commons.ThreadPriorities[j]);
            label13.Text = ("Поток со сплайном степени " + spline3.POWER + ", приоритет: " + Commons.ThreadPriorities[k]);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            /*
             * disable for lab5
            Form2 f2 = new Form2();
            f2.ShowDialog();
             */
            /*switch (Commons.SyncWay)
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

        private void button7_Click(object sender, EventArgs e)
        {
            CreatePipe();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ReadDataFromPipe();
        }
    }
}
