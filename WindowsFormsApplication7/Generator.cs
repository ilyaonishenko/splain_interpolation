using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WindowsFormsApplication7
{
    class Generator
    {
        double[] array;
        double[] arrax;
        int N;

        public void generate(int n, int min, int max)
        {
            N = n;
            array = new double[n];
            arrax = new double[n];

            Random r = new Random();

            for (int i = 0; i < n; i++)
            {
                array[i] = Math.Sin(i) * r.NextDouble() * (max - min) + min;
                arrax[i] = Math.Cos(i) * r.NextDouble() *10* (max - min) + min;

                if (arrax[i] > (double)Decimal.MaxValue)
                {
                    arrax[i] = (double)Decimal.MaxValue;
                }
                else if (arrax[i] < (double)Decimal.MinValue)
                {
                    arrax[i] = (double)Decimal.MinValue;
                }

                if (array[i] > (double)Decimal.MaxValue)
                {
                    array[i] = (double)Decimal.MaxValue;
                }else if (array[i] < (double)Decimal.MinValue){
                    array[i] = (double) Decimal.MinValue;
                }
            }

            writeInFile();
        }

        public void writeInFile()
        {
            StreamWriter writer = new StreamWriter("C:\\Users\\veryoldbarny\\Documents\\input.txt");
            writer.WriteLine(N);
            for (int i = 0; i < N; i++)
            {
                writer.WriteLine(arrax[i] + " " + array[i]);
            }
            writer.Close();
        }
    }
}
