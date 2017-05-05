using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication7
{
    public partial class Form2 : Form
    {

        [DllImport("kernel32.dll")]
        static extern uint SleepEx(uint dwMilliseconds, bool bAlertable);

        //ТАЙМЕРЫ!
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);


        public delegate void TimerCompleteDelegate();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(IntPtr hTimer, 
            [In] ref long pDueTime, 
            int lPeriod, 
            IntPtr pfnCompletionRoutine, 
            IntPtr lpArgToCompletionRoutine, 
            bool fResume);

        [DllImport("kernel32.dll")]
        static extern IntPtr FindFirstChangeNotification(string lpPathName,
            bool bWatchSubtree, 
            uint dwNotifyFilter);

        [DllImport("kernel32.dll")]
        static extern bool FindNextChangeNotification(IntPtr hChangeHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        
        private long startTime;
        private IntPtr waitableTimer;
        public Form2()
        {
            InitializeComponent();
            label2.Visible = false;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            dateTimePicker2.Format = DateTimePickerFormat.Time;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*222
            if (radioButton1.Checked)
                Commons.SyncWay = Sync.Semaphore;
            else if (radioButton2.Checked)
                Commons.SyncWay = Sync.Mutex;
            else
                Commons.SyncWay = Sync.Event;
            this.Hide();
             */
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label1.Visible = false;
            button2.Visible = false;
            Thread.Sleep(10);
            startTime = (dateTimePicker1.Value.Date + dateTimePicker2.Value.TimeOfDay).ToFileTime();
            waitableTimer = CreateWaitableTimer(IntPtr.Zero, true, "waitabletimer");

            if (SetWaitableTimer(waitableTimer, ref startTime,0, IntPtr.Zero, IntPtr.Zero, true))
            {
                WaitForSingleObject(waitableTimer, 0xFFFFFFFF);
            }

            Form1 mainProgram = new Form1();
            mainProgram.Show();
            //Hide();
            /*
            if (radioButton1.Checked)
                syncWay = 1; // semaphore
            else if (radioButton2.Checked)
                syncWay = 2; // mutex
            else
                syncWay = 3; // event


            // todo write exception
            N = Int32.Parse(textBox1.Text);

            Generator gen = new Generator();

            gen.generate(N, 1, 3);
            ReadPoints();

            //arrays[2*N] = syncWay;

            //label3.Text = arrays[2 * N].ToString();

            // create pipe
            //IntPtr pipe = CreateNamedPipe("\\\\.\\pipe\\MyPipe", 0x00000003, 0x00000004 | 0x00000002 | 0x00000000, 1, 512, 512, 5000, IntPtr.Zero);
            //IntPtr pipe1 = CreateNamedPipe("\\\\.\\pipe\\MyPipe1", 0x00000003, 0x00000004 | 0x00000002 | 0x00000000, 1,
            //  512, 512, 5000, IntPtr.Zero);
            // PIPE_ACCESS_DUPLEX = 0x00000003
            // PIPE_TYPE_MESSAGE = 0x00000004
            //PIPE_READMODE_MESSAGE = 0x00000002
            //PIPE_WAIT = 0x00000000
            //PIPE_NOWAIT = 0x00000001
            IntPtr lenFile = CreateFile("\\\\.\\pipe\\LengthPipe", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.OpenOrCreate, 0, IntPtr.Zero);
            IntPtr arrayXFile = CreateFile("\\\\.\\pipe\\ArrayXPipe", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero,
                FileMode.OpenOrCreate, 0, IntPtr.Zero);
            IntPtr arrayYFile = CreateFile("\\\\.\\pipe\\ArrayYPipe", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero,
                FileMode.OpenOrCreate, 0, IntPtr.Zero);
            IntPtr syncWayFile = CreateFile("\\\\.\\pipe\\SyncWayPipe", FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero,
                FileMode.OpenOrCreate, 0, IntPtr.Zero);
            //gen.SortArrays(syncWay);
            WriteFile(lenFile, new double[] { N }, 64, out bytesWritten, IntPtr.Zero);
            WriteFile(arrayXFile, X, 1024, out bytesWritten1, IntPtr.Zero);
            WriteFile(arrayYFile, Y, 1024, out bytesWritten1, IntPtr.Zero);
            WriteFile(syncWayFile, new double[] { syncWay }, 64, out bytesWritten, IntPtr.Zero);
            // todo change location file
            //CreateProcessHelper.CreateProcess("C:\\Users\\veryoldbarny\\Documents\\WindowsFormsApplication7.exe", String.Empty);
            //String path = "C:\\Users\\veryoldbarny\\WindowsFormsApplication7.exe";

            //CloseHandle(lenFile);
            //CloseHandle(arrayXFile);
            //CloseHandle(arrayYFile);
            ///CloseHandle(syncWayFile);
             */
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker2_Leave(object sender, EventArgs e)
        {
            label2.Visible = true;
        }
    }
}
