using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Optimizavimas
{
    class Test
    {
        public static void Entry()
        {
            //---
            int KartojimaiVidurkiui = 5;
            int maxThread = 6;
            //---
            int count = 20000;
            Diagnostics(count, maxThread, KartojimaiVidurkiui);
            //---
        }

        public static void Diagnostics(int count, int maxThread, int kartojimai)
        {
            Debug.WriteLine("Duomenų rinkinio dydis = " + count);
            double[] numberList = Enumerable.Repeat(0, count).Select(i => new Random().NextDouble() * 20 - 10).ToArray();

            for (int i = 1; i <= maxThread; i++)
            {
                Debug.WriteLine(string.Format("Gijų skaičius = {0}, vidutinis vykdymo laikas = {1}", i, Task(numberList, i, kartojimai)));
            }
        }
        public static long Task(double[] numberList, int threads, int kartojimai)
        {
            Stopwatch stopWatch = new Stopwatch();
            long vidurkis = 0;
            for (int i = 0; i < kartojimai; i++)
            {
                stopWatch.Start();
                double[] queryA = (from num in numberList.AsParallel().WithDegreeOfParallelism(threads)
                                   select Work(num)).ToArray();
                stopWatch.Stop();
                var laikas = stopWatch.ElapsedMilliseconds;
                Debug.WriteLine("Praejes laikas = " + laikas);
                vidurkis += laikas;
                stopWatch.Reset();
            }
            return vidurkis/kartojimai;

        }

        public static double Work(double number)
        {
            for (int i = 0; i < 10000; i++)
            {
                number += 20;
                number -= 20;
            }
            return number * 2;
        }

        public static void PrintArray(double[] array1, double[] array2)
        {
            bool atitinka = true;
            if (array1.Length == array2.Length)
            {
                for (int i = 0; i < array1.Length; i++)
                {
                    //Debug.WriteLine(array1[i] + " " + array2[i]);
                    if (array1[i] != array2[i])
                    {
                        atitinka = false;
                    }
                }
            }
            else 
            {
                atitinka = false;
            }
            string pozymis = atitinka ? "sutampa" : "nesutampa";
            Debug.WriteLine("----------------------------------------------------------------------------------------------------------");
            Debug.WriteLine("Masyvai " + pozymis);
            Debug.WriteLine("----------------------------------------------------------------------------------------------------------");
        }
    }
}
