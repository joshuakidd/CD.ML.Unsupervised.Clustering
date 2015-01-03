using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.ML.Unsupervised.Clustering {
    public class DataHelper {

        public static double Euclidean(double[] x, double[] y) {

            // verify the arrays are the same size 
            if (x.Length != y.Length)
                throw new InvalidOperationException();

            double result = 0;
            for (int i = 0; i < x.Length; i++)
                result += Math.Pow((x[i] - y[i]), 2);
            return Math.Sqrt(result);
        }

        public static double[][] Sample(int rows, int cols, int seed = 0) {
            int randomSeed = (seed == 0) ? Environment.TickCount : seed;
            Random random = new Random(randomSeed);

            double[][] newArray = new double[rows][];
            for (int r = 0; r < rows; r++) {
                newArray[r] = new double[cols];
                for (int c = 0; c < cols; c++)
                    newArray[r][c] = random.NextDouble();
            }

            return newArray;
        }
    }
}
