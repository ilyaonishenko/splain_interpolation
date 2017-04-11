using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace WindowsFormsApplication7
{
    static class GlobalValues
    {
        public static bool LogToFile { get; set; }
        //массив значений приоритетов потоков в C#
        public static ThreadPriority[] ThreadPriorities = { ThreadPriority.Lowest, ThreadPriority.BelowNormal, ThreadPriority.Normal, ThreadPriority.AboveNormal, ThreadPriority.Highest };

        public static object SplainInterpolationLockObject1 = new object();
        public static object SplainInterpolationLockObject2 = new object();
        public static object SplainInterpolationLockObject3 = new object();
        public static object SplainInterpolationLockObject4 = new object();

        //сигнальщик событий для   WaitForSingleObject
        public static EventWaitHandle WaitHandle = new AutoResetEvent(false);

        // Массивы для хранения координат опорных точек
        public static double[] X = null, Y = null;

        // Количество опорных точек
        public static int SIZE = 0;

        public static void WaitForSingleObject()
        {
            //.net аналог WaitForSingleObject() winapi функции
            WaitHandle.WaitOne();
        }

        public static void SevEvent()
        {
            //.net аналог SevEvent() winapi функции
            WaitHandle.Set();
        }

        public static void EnterCriticalSection(object lockObj)
        {
            //.net аналог входа в критическую секцию
            Monitor.Enter(lockObj);
        }

        public static void LeaveCriticalSection(object lockObj)
        {
            //.net аналог выхода из критической секции
            Monitor.Exit(lockObj);
        }

        public class SplineBox
        {
            public int POWER = 0;
            public List<double> listX;
            public List<double> listY;
            public SplineBox(int power)
            {
                POWER = power;
                listX = new List<double>();
                listY = new List<double>();
            }
        }
    }
}
