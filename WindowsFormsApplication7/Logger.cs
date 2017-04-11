using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApplication7
{
    class Logger : IDisposable
    {
        StreamWriter file;
        FileStream fileStream;
        public static Logger Current = new Logger();
        public static object lockObject = new object();

        private StreamWriter File
        {
            get
            {
                if (file == null)
                {
                    string fileName = "log.txt";
                    fileStream = System.IO.File.Create(fileName);
                    file = new StreamWriter(fileStream);
                }
                return file;
            }
        }

        public void WriteLine()
        {
            WriteLine(string.Empty);
        }

        public void WriteLine(string value)
        {
            Monitor.Enter(lockObject);
            try
            {
                Console.WriteLine(value);
                if (GlobalValues.LogToFile)
                {
                    File.WriteLine(value);
                }
            }
            finally
            {
                Monitor.Exit(lockObject);
            }
        }

        public void WriteLine(string format, object arg0)
        {
            WriteLine(string.Format(format, arg0));
        }

        public void Dispose()
        {
            Flush();
            if (fileStream != null)
            {
                fileStream.Dispose();
            }
        }

        public void Flush()
        {
            if (fileStream != null)
            {
                fileStream.Flush();
            }
        }
    }
}
