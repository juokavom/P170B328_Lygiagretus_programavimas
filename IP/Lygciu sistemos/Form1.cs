//Jokubas Akramas IFF-8/12
//P170B328 Lygiagretusis programavimas (6 kr.)
//Inžinerinis projektas - SMA II projektinės užduoties 3 dalis (optimizavimas - 7 var.)

using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Optimizavimas
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            Initialize();
        }

        //----------------------------------------------------------------------UZDUOTIS----------------------------------------------------------------------
        Series z1, p1, z2, p2;

        private void Button3_Click(object sender, EventArgs e)
        {
            ClearForm1();
            PreparareForm(-10, 10, -10, 10);
            //---
            double s = 500;
            int count = 6;
            //---
            Random randNum = new Random();
            double[] x = Enumerable.Repeat(0, count).Select(i => randNum.NextDouble()*20-10).ToArray();
            double[] y = Enumerable.Repeat(0, count).Select(i => randNum.NextDouble()*20-10).ToArray();
            x[0] = 0;
            y[0] = 0;
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
            for (int i = 0; i < x.Length; i++)
            {
                p1.Points.AddXY(x[i], y[i]);
                for (int u = i+1; u < x.Length; u++)
                {
                    z1.Points.AddXY(x[i], y[i]);
                    z1.Points.AddXY(x[u], y[u]);
                    p1.Points.AddXY(x[u], y[u]);
                    z1.Points.AddXY(x[i], y[i]);
                }
            }
            z1.BorderWidth = 1;
            p1.BorderWidth = 3;
            p2.BorderWidth = 3;
            z2.BorderWidth = 1;

            //Sprendimas
            Optimizacija(x, y, s);
        }
        public void PrintPoints(double[] x, double[] y)
        {
            z2.Points.Clear();
            p2.Points.Clear();
            for (int i = 0; i < x.Length; i++)
            {
                p1.Points.AddXY(x[i], y[i]);
                for (int u = i + 1; u < x.Length; u++)
                {
                    z2.Points.AddXY(x[i], y[i]);
                    z2.Points.AddXY(x[u], y[u]);
                    p2.Points.AddXY(x[u], y[u]);
                    z2.Points.AddXY(x[i], y[i]);
                }
            }
        }

        private void Optimizacija(double[] x, double[] y, double s) 
        {
            double eps = 1e-6;
            int maxIter = 500;
            double zingsnis = 0.1;
            double tikslumas = Double.MaxValue;
            int iteracija = 0;

            for (; iteracija < maxIter; iteracija++) 
            {
                //printPoints(x, y);
                double vid = Vidurkis(x, y);
                int n = x.Length;
                double[,] grad = Gradientas(x, y, vid, s);
                double f0 = Tikslo(x, y, vid, s);
                double[,] deltaX = Gradiento_norma(grad, zingsnis);
                for(int u = 1; u < n; u++)
                {
                    x[u] -= deltaX[u, 0];
                    y[u] -= deltaX[u, 1];
                }
                double f1 = Tikslo(x, y, vid, s);
                if (f1 > f0)
                {
                    for (int u = 1; u < n; u++)
                    {
                        x[u] += deltaX[u, 0];
                        y[u] += deltaX[u, 1];
                    }
                    zingsnis /= 2;
                }
                else 
                {
                    zingsnis *= 2;
                }
                tikslumas = Math.Abs(f0-f1)/(Math.Abs(f0)+Math.Abs(f1));
                if (tikslumas < eps)
                {
                    richTextBox1.AppendText("Baigta sekmingai\n");
                    break;
                }
                else if (iteracija == maxIter - 1)
                {
                    richTextBox1.AppendText("Baigta nesekmingai\n");
                }
                richTextBox1.AppendText(string.Format("Iteracija: {0}, tikslumas: {1}, tikslo f-ija: {2, 0:F2}\n", iteracija, tikslumas, f1));
            }
            richTextBox1.AppendText(string.Format("Iteracijų skaičius = {0}, tikslumas = {1}\n", iteracija, tikslumas));
            PrintPoints(x, y);
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

        private double Vidurkis(double[] x, double[] y)
        {
            double n = x.Length;
            double suma = 0;
            int count = 0;
            for (int i = 0; i < n; i++) 
            {
                for (int u = i + 1; u < n; u++) 
                {
                    suma += Math.Sqrt(Math.Pow(x[u]-x[i], 2) + Math.Pow(y[u] - y[i], 2));
                    count++;
                }
            }
            return suma / count;
        }
        private double Ilgis(double[] x, double[] y)
        {
            double n = x.Length;
            double suma = 0;
            for (int i = 0; i < n; i++)
            {
                for (int u = i + 1; u < n; u++)
                {
                    suma += Math.Sqrt(Math.Pow(x[u] - x[i], 2) + Math.Pow(y[u] - y[i], 2));
                }
            }
            return suma;
        }

        private double[,] Gradientas(double[] x, double[] y, double vid, double s)
        {
            int n = x.Length;
            double zingsnis = 0.0001;
            double[,] grad = new double[n, 2]; 
            double f0 = Tikslo(x, y, vid, s);

            for (int i = 0; i < n; i++)
            {
                grad[i, 0] = (Tikslo(F1(x, i, zingsnis), y, vid, s) - f0) / zingsnis;
                grad[i, 1] = (Tikslo(x, F1(y, i, zingsnis), vid, s) - f0) / zingsnis;
            }

            return grad;
        }

        private double[] F1(double[] a, int i, double zing)
        {
            int n = a.Length;
            double[] copy = new double[n];
            Array.Copy(a, copy, n);
            copy[i] += zing;
            return copy;
        }

        private double Tikslo(double[] x, double[] y, double vid, double s)
        {
            int n = x.Length;
            double suma = 0;
            for (int i = 0; i < n; i++)
            {
                for (int u = i + 1; u < n; u++)
                {
                    suma += Math.Pow(Math.Sqrt(Math.Pow(x[u] - x[i], 2) + Math.Pow(y[u] - y[i], 2)) - vid, 2);
                }
            }
            return suma + Math.Abs(Ilgis(x, y) - s);
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
