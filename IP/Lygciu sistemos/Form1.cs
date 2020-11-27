//Jokubas Akramas IFF-8/12
//P170B328 Lygiagretusis programavimas (6 kr.)
//Inžinerinis projektas - SMA II projektinės užduoties 3 dalis (optimizavimas - 7 var.)

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Optimizavimas
{

    public partial class Form1 : Form
    {
        public static int ThreadCount;

        public Form1()
        {
            InitializeComponent();
            Initialize();
        }

        //----------------------------------------------------------------------UZDUOTIS----------------------------------------------------------------------
        Series z1, p1, z2, p2;

        private void Button3_Click(object sender, EventArgs e)
        {
            //---
            int rinkinys = 1 + listBox1.SelectedIndex; // x e [1; 10]
            int maxThreads = 12;
            int kartojimaiVidurkiui = 5;
            //---
            double[] X = { -10, 10 };
            double[] Y = { -10, 10 };
            //---            
            ClearForm1();
            PreparareForm((float)X[0], (float)X[1], (float)Y[0], (float)Y[1]);
            //---
            //---
            z1 = chart1.Series.Add("Pradiniai kontūrai");
            z1.ChartType = SeriesChartType.Line;
            z1.Color = Color.Blue;
            //---
            p1 = chart1.Series.Add("Pradiniai taškai");
            p1.ChartType = SeriesChartType.Point;
            p1.Color = Color.Black;
            //---
            z2 = chart1.Series.Add("Galutiniai kontūrai");
            z2.ChartType = SeriesChartType.Line;
            z2.Color = Color.Red;
            //---
            p2 = chart1.Series.Add("Galutiniai taškai");
            p2.ChartType = SeriesChartType.Point;
            p2.Color = Color.Green;
            //---
            //---
            z1.BorderWidth = 1;
            p1.BorderWidth = 3;
            p2.BorderWidth = 3;
            z2.BorderWidth = 1;
            //---
            GreitaveikosTyrimas(rinkinys, maxThreads, kartojimaiVidurkiui);
        }

        /// <summary>
        /// Programos entry point nuo vartotojo lango
        /// </summary>
        /// <param name="rinkinys">Pasirinkto rinkinio numeris</param>
        /// <param name="maxThreads">Maksimalių gijų skaičius tyrime</param>
        /// <param name="kartojimaiVidurkiui">Kartojimų skaičius matavimų vidurkiui išvesti</param>
        private void GreitaveikosTyrimas(int rinkinys, int maxThreads, int kartojimaiVidurkiui)
        {
            //Sukuriamas taškų objektas pagal vartotojo pasirinktą rinkinio dydį
            Dots dots = new Dots(rinkinys);
            double[] x = dots.x;
            double[] y = dots.y;
            double s = dots.S;
            //---            
            //Brėžiama pradinė taškų seka
            PrintPoints(x, y, z1, p1);
            //---
            Stopwatch stopWatch = new Stopwatch();
            string line45 = new string('-', 45);
            string line92 = new string('-', 92);
            //---
            double[] xnew = new double[x.Length];
            double[] ynew = new double[y.Length];
            double[] benchmarkData = new double[maxThreads];
            //---
            richTextBox1.AppendText(string.Format("Rinkinio dydis = {0}\n", dots.x.Length));
            richTextBox1.AppendText(line45 + "\n");
            //Sukamas ciklas kiekvienam kiekiui procesų
            for (int i = 1; i <= maxThreads; i++)
            {
                richTextBox1.AppendText(line45 + "\n");
                ThreadCount = i;
                double suma = 0;
                //Kiekvienam procesui skaičiavimai kartojami kelis kartus, pagal šį ciklą
                for (int u = 0; u < kartojimaiVidurkiui; u++)
                {
                    //---
                    Array.Copy(x, xnew, x.Length);
                    Array.Copy(y, ynew, y.Length);
                    //---                    
                    //Kviečiamas uždavinio sprendimo metodas ir matuojamas atlikimo laikas
                    stopWatch.Start();
                    Optimizacija(xnew, ynew, s);
                    stopWatch.Stop();
                    //---
                    //Laiko reikšmė atspausdinama į ekraną ir išsaugojama vidurkiui išvesti
                    double ms = stopWatch.ElapsedMilliseconds;
                    suma += ms;
                    richTextBox1.AppendText(string.Format("Procesas: {0}, laikas: {1}, kartojimas: {2}/{3}\n", i, ms, u + 1, kartojimaiVidurkiui));
                    stopWatch.Reset();
                }
                benchmarkData[i - 1] = suma / kartojimaiVidurkiui;
            }
            richTextBox1.AppendText(line92 + "\n");
            richTextBox1.AppendText(line92 + "\n");
            //---
            //Išvedama duomenų rinkinio greitaveikos informacija pagal panaudotų gijų skaičių
            for (int i = 0; i < benchmarkData.Length; i++)
            {
                richTextBox1.AppendText(string.Format("Procesų skaičius = {0}, Vidutinis laikas = {1}\n", i + 1, benchmarkData[i]));
            }
            //---
            richTextBox1.AppendText(line92 + "\n");
            richTextBox1.AppendText(line92 + "\n");
        }

        /// <summary>
        /// Pagrindinis uždavinio sprendimo metodas
        /// </summary>
        /// <param name="x">Abscisių reikšmių vektorius</param>
        /// <param name="y">Oordinačių reikšmių vektorius</param>
        /// <param name="s">Užduoties argumentas S</param>
        private void Optimizacija(double[] x, double[] y, double s)
        {
            //Pradiniai duomenys skaičiavimams (tikslumas, iteracijų sk., zingsnis, ir t.t.)
            double eps = 1e-6;
            int maxIter = 500;
            double zingsnis = 0.1;
            double tikslumas;
            int iteracija = 0;

            //Iteracinis ciklas tolimesnėms reikšmėms skaičiuoti
            for (; iteracija < maxIter; iteracija++)
            {
                int n = x.Length;
                //Vidutinio atstumo tarp taškų radimas
                double vid = Ilgis(x, y, 0, true) / (n * (n - 1) * 1 / 2);
                //Išlygiagretintas metodas, užimantis 99% laiko resursų
                double[,] grad = Gradientas(x, y, vid, s);
                //Tikslo funkcijos apskaičiavimas esamame taške
                double f0 = Tikslo(x, y, vid, s);
                //Gradiento normalės radimas
                double[,] deltaX = Gradiento_norma(grad, zingsnis);
                //Ėjimas prieš gradiento kryptį, t.y tikslo funkcijos mažėjimo kryptimi
                for (int u = 1; u < n; u++)
                {
                    x[u] -= deltaX[u, 0];
                    y[u] -= deltaX[u, 1];
                }
                //Tikslo funkcijos apskaičiavimas sekančiame taške
                double f1 = Tikslo(x, y, vid, s);
                //Jei tikslo funkcija padidėjo, grįžtama į buvusį tašką (eil. 174-178) ir mažinamas žingsnis (eil. 179)
                if (f1 > f0)
                {
                    for (int u = 1; u < n; u++)
                    {
                        x[u] += deltaX[u, 0];
                        y[u] += deltaX[u, 1];
                    }
                    zingsnis /= 2;
                }
                //Jei tikslo funkcija sumažėjo (to siekiame), žingsnis padvigubinamas
                else
                {
                    zingsnis *= 2;
                }
                //Apskaičiuojamas tikslumas
                tikslumas = Math.Abs(f0 - f1) / (Math.Abs(f0) + Math.Abs(f1));
                //Jei tikslumas atitinka nurodytą, ciklas užbaigiamas
                if (tikslumas < eps)
                {
                    richTextBox1.AppendText("Baigta sekmingai\n");
                    break;
                }
                else if (iteracija == maxIter - 1)
                {
                    richTextBox1.AppendText("Baigta nesekmingai\n");
                }
            }
            //Brėžiama gauta taškų seka
            PrintPoints(x, y, z2, p2);
        }

        /// <summary>
        /// Išlygiagretintas metodas, užimantis 99% laiko resursų
        /// </summary>
        /// <param name="x">Abscisių reikšmių vektorius</param>
        /// <param name="y">Oordinačių reikšmių vektorius</param>
        /// <param name="vid">Vidutinis atstumas tarp taškų</param>
        /// <param name="s">Užduoties argumentas S</param>
        /// <returns></returns>
        private double[,] Gradientas(double[] x, double[] y, double vid, double s)
        {
            //Pradiniai duomenys skaičiavimams
            int n = x.Length;
            double zingsnis = 0.0001;
            double[,] grad = new double[n, 2];
            //Tikslo funkcija esamame taške
            double f0 = Tikslo(x, y, vid, s);
            //---
            var numbers = Enumerable.Range(0, n);
            //---
            //Lygiagretinti metodai
            //Gradiento skaičiavimas abscisių ašies atžvilgiu
            double[] gradX = (from i in numbers.AsParallel().AsOrdered().WithMergeOptions(ParallelMergeOptions.NotBuffered).WithDegreeOfParallelism(ThreadCount)
                              select ((Tikslo(F1(x, i, zingsnis), y, vid, s) - f0) / zingsnis)).ToArray();
            //Gradiento skaičiavimas oordinačių ašies atžvilgiu
            double[] gradY = (from i in numbers.AsParallel().AsOrdered().WithMergeOptions(ParallelMergeOptions.NotBuffered).WithDegreeOfParallelism(ThreadCount)
                              select ((Tikslo(x, F1(y, i, zingsnis), vid, s) - f0) / zingsnis)).ToArray();
            //---
            //Abscisės ir oordinatės sujungiamos į matricą
            for (int i = 0; i < n; i++)
            {
                grad[i, 0] = gradX[i];
                grad[i, 1] = gradY[i];
            }

            //Originaliai taikytas sprendimo būdas prieš pradedant taikyti lygiagretų sprendimą (eil. 217-230)
            /* 
            for (int i = 0; i < n; i++)
            {
                grad[i, 0] = (Tikslo(F1(x, i, zingsnis), y, vid, s) - f0) / zingsnis;
                grad[i, 1] = (Tikslo(x, F1(y, i, zingsnis), vid, s) - f0) / zingsnis;
            }
            */

            return grad;
        }

        private double[,] Gradiento_norma(double[,] gradientas, double zingsnis)
        {
            double suma = 0;
            for (int i = 0; i < gradientas.GetLength(0); i++)
            {
                for (int u = 0; u < gradientas.GetLength(1); u++)
                {
                    suma += Math.Pow(gradientas[i, u], 2);
                }
            }
            double normale = Math.Sqrt(suma);
            double[,] copy = new double[gradientas.GetLength(0), gradientas.GetLength(1)];
            for (int i = 0; i < gradientas.GetLength(0); i++)
            {
                for (int u = 0; u < gradientas.GetLength(1); u++)
                {
                    copy[i, u] = gradientas[i, u] / normale * zingsnis;
                }
            }
            return copy;
        }


        public void PrintPoints(double[] x, double[] y, Series z, Series p)
        {
            z.Points.Clear();
            p.Points.Clear();
            for (int i = 0; i < x.Length; i++)
            {
                p.Points.AddXY(x[i], y[i]);
                for (int u = i + 1; u < x.Length; u++)
                {
                    z.Points.AddXY(x[i], y[i]);
                    z.Points.AddXY(x[u], y[u]);
                    p.Points.AddXY(x[u], y[u]);
                    z.Points.AddXY(x[i], y[i]);
                }
            }
        }
        private double[] F1(double[] a, int i, double zing)
        {
            int n = a.Length;
            double[] copy = new double[n];
            Array.Copy(a, copy, n);
            copy[i] += zing;
            return copy;
        }

        private double SumaPagalPozymi(bool pozymis, double vid, double[] x, double[] y, int i, int u)
        {
            if (pozymis) return Math.Sqrt(Math.Pow(x[u] - x[i], 2) + Math.Pow(y[u] - y[i], 2));
            else return Math.Pow(Math.Sqrt(Math.Pow(x[u] - x[i], 2) + Math.Pow(y[u] - y[i], 2)) - vid, 2);
        }

        private double Ilgis(double[] x, double[] y, double vid, bool pozymis)
        {
            int n = x.Length;
            double suma = 0;
            for (int i = 0; i < n; i++)
            {
                for (int u = i + 1; u < n; u++)
                {
                    suma += SumaPagalPozymi(pozymis, vid, x, y, i, u);
                }
            }
            return suma;
        }

        private double Tikslo(double[] x, double[] y, double vid, double s)
        {
            return Ilgis(x, y, vid, false) + Math.Abs(Ilgis(x, y, vid, true) - s);
        }



        // ---------------------------------------------- KITI METODAI ----------------------------------------------

        /// <summary>
        /// Uždaroma programa
        /// </summary>
        private void Button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Išvalomas grafikas ir consolė
        /// </summary>
        private void Button2_Click(object sender, EventArgs e)
        {
            ClearForm1();
        }


        public void ClearForm1()
        {
            richTextBox1.Clear(); // isvalomas richTextBox1

            // isvalomos visos nubreztos kreives
            chart1.Series.Clear();
        }
    }
}
