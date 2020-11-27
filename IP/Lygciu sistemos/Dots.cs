using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimizavimas
{
    class Dots
    {
        public double S { get; private set; }
        public double[] x { get; private set; }
        public double[] y { get; private set; }

        public Dots(int i)
        {
            string[] lines = File.ReadAllLines(@"../../Data/" + i + ".txt");
            S = Double.Parse(lines[0]);
            x = lines[1].Split(',').Select(l => Double.Parse(l)).ToArray();
            y = lines[2].Split(',').Select(l => Double.Parse(l)).ToArray();
        }

        public override string ToString()
        {
            string value = "";
            value += "S = " + S + "\n";
            value += "x[] = " + "\n";
            for (int i = 0; i < x.Length; i++)
            {
                value += x[i] + ", ";
            }
            value += "\ny[] = " + "\n";
            for (int i = 0; i < y.Length; i++)
            {
                value += y[i] + ", ";
            }
            return value;
        }
    }
}
