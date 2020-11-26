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
            int count = 200000;
            Random randNum = new Random();
            Stopwatch stopWatch = new Stopwatch();
            double[] numberList = Enumerable.Repeat(0, count).Select(i => randNum.NextDouble() * 20 - 10).ToArray();

            stopWatch.Start();
            double[] queryA = (from num in numberList.AsParallel()
                              select Work(num)).ToArray();
            stopWatch.Stop();
            var lygiagretus = stopWatch.Elapsed;

            stopWatch.Restart();
            double[] queryB = (from num in numberList
                              select Work(num)).ToArray();
            stopWatch.Stop();
            var paprastas = stopWatch.Elapsed;

            Debug.WriteLine("-----------------------------------------------------queryA+B-----------------------------------------------------");
            PrintArray(queryA, queryB);
            Debug.WriteLine("Lygiagretus = " + lygiagretus + "\nPaprastas   = " + paprastas);
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
