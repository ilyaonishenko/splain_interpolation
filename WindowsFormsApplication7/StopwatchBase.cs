using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication7
{
    public abstract class StopwatchBase
    {
        private long endTimeStamp;
        private long startTimeStamp;

        private bool isRunning;

        public void Start()
        {
            isRunning = true;

            long timestamp = GetCurrentTimes();
            startTimeStamp = timestamp;
        }

        private long GetCurrentTimes()
        {
            long notIntersting;
            long kernelTime, userTime;

            bool success = GetCurrentTimes
                (out notIntersting, out notIntersting, out kernelTime, out userTime);

            if (!success)
                throw new Exception(string.Format
                ("failed to get timestamp. error code: {0}",
                success));

            long result = kernelTime + userTime;
            return result;
        }

        protected abstract bool GetCurrentTimes(out long lpCreationTime,
            out long lpExitTime,
            out long lpKernelTime,
            out long lpUserTime);

        public void Stop()
        {
            isRunning = false;

            long timestamp = GetCurrentTimes();
            endTimeStamp = timestamp;
        }

        public void Reset()
        {
            startTimeStamp = 0;
            endTimeStamp = 0;
        }

        public TimeSpan Elapsed
        {
            get
            {
                long elapsed = endTimeStamp - startTimeStamp;
                TimeSpan result =
                    TimeSpan.FromMilliseconds(elapsed / 10000);
                return result;
            }
        }

        public bool IsRunning
        {
            get { return isRunning; }
        }
    }
}
