using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CD.ML.Unsupervised.Clustering {

    public delegate void FitStepHandler(FitStep step);
    public delegate void FitStepFailureHandler(FitStepFailure failure); 

    /// <summary>
    /// 
    /// </summary>
    public class KMeans {

        public event FitStepHandler OnFitStep;
        public event FitStepFailureHandler OnFitStepFailure; 

        private int _k;                         // number of clusters 
        private int _m;                         // number of records 
        private int _n;                         // number of features 
        private double _delta;                  // cost change threshold 
        private int _iterations;                // maximum number of iterations 
        private int _sleep;                     // time to sleep between iterations (milliseconds) 
        private int _iteration = 0;             // current iteration 
        private Matrix<double> _data;           // m x n matrix 
        private int[] _membership;              // m array, tracks centroid ownership by data record 
        private double[][] _centroids = null;   // k x n matrix, tracks centroid location 
        private object[] _locks;                // k array, a lock for each centroid  

        private Random _random;                 // random number generator (for initialization) 
        private double _prevCost;               // cost of the last iteration 

        public KMeans(Matrix<double> data, int k, double delta = 0.001, int iterations = 1000, int randomSeed = 0, int sleep = 0) {

            // create a new unique random id 
            Id = Guid.NewGuid(); 

            // initialize clustering variables 
            _k = k;
            _delta = delta;
            _iterations = iterations;
            _sleep = sleep; 
            _data = data;
            _m = _data.Count;
            _n = _data.FeatureCount;
            _membership = new int[_m];
            _centroids = EmptyArray(_k, _n);

            _locks = new object[_k]; 
            for (int i = 0; i < _locks.Length; i++)
                _locks[i] = new object(); 

            // set random number generator seed 
            int seed = (randomSeed == 0) ? Environment.TickCount : randomSeed;
            _random = new Random(seed);

            // initialize the centroids 
            Initialize(); 
        }

        /// <summary>
        /// KMeans unique id 
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        public FitResult Fit(int retryCount = 3) {

            FitResult result = new FitResult(); 

            int retries = 0;
            bool complete = false; 
            while (_iteration < _iterations) {

                // move centroids 
                int orphan = MoveCentroids();
                if (orphan > -1) { // if an orphaned centroid index has been returned (no data members), this iteration has failed 
                    // verify we have not exceeded the retry count, otherwise increment 
                    if (retries++ >= retryCount) {
                        result.State = FitResultState.ExceededRetryThreshold;
                        result.RetryCount = retries; 
                        break; 
                    }

                    if (OnFitStepFailure != null) {
                        OnFitStepFailure(new FitStepFailure() {
                            Id = Id, 
                            Iteration = _iteration,
                            RetryCount = retries,
                            Orphan = orphan 
                        });
                    }

                    // reinitialize the orphaned centroid 
                    Reinitialize(orphan);

                    continue;
                }

                // reassign the data and calculate the cost 
                double cost = Assign();

                if (OnFitStep != null)
                    OnFitStep(CreateFitStep(cost)); 

                // check whether the cost has changed be less than or equal to the specified threshold 
                if ((_prevCost - cost) <= _delta)
                    complete = true; 

                _prevCost = cost;

                if (complete)
                    break; 

                _iteration++;

                if (_sleep > 0)
                    Thread.Sleep(_sleep); 
            }

            result.Step = CreateFitStep(_prevCost); 

            return result;
        }

        private FitStep CreateFitStep(double cost) {
            return new FitStep() {
                Id = Id, 
                Iteration = _iteration,
                Cost = cost,
                Centroids = _centroids,
                Membership = _membership
            };
        }

        private void Initialize() {
            // randomly initialize the centroids 
            InitializeCentroids();
            // assign data to centroids, update cost 
            _prevCost = Assign();
            // reset iteration count 
            _iteration = 0;
        }

        private void Reinitialize(int k) {
            // randomly reinitialize the specified centroid 
            InitializeCentroid(k);
            // assign data to centroids, update cost 
            _prevCost = Assign();
            // reset iteration count 
            _iteration = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private double Assign() {

            // cost function 
            // J = sum_{k=1}^{K}(sum_{i \in C_k}(|| x_i - \mu_k ||^2))

            // cost of each centroid 
            double[] cost = new double[_k];

            // iterate over all data rows in parallel 
            Parallel.For(0, _m, m => {

                double minDistance = -1;
                int minK = -1;

                // find the closest centroid 
                for (int k = 0; k < _k; k++) {
                    // calcuate the distance, compare to previous distance 
                    double distance = DataHelper.Euclidean(_data[m], _centroids[k]);
                    if (minDistance == -1 || distance < minDistance) {
                        minDistance = distance;
                        minK = k;
                    }
                }

                // set the membership tracker 
                _membership[m] = minK;

                // calculate this members cost, lock the centroid, and add to the accumulating cost 
                double memberCost = Math.Pow(minDistance, 2);
                lock (_locks[minK])
                    cost[minK] += memberCost;
            });

            // sum the cost for each centroid and return as overall cost 
            return cost.Sum();
        }

        /// <summary>
        /// Randomly initialize the centroids 
        /// </summary>
        /// <param name="seed">random seed</param>
        private void InitializeCentroids() {
            for (int k = 0; k < _k; k++)
                InitializeCentroid(k);
        }

        /// <summary>
        /// Randomly initialize the specified centroid 
        /// </summary>
        /// <param name="k">centroid id</param>
        /// <param name="random">Random number generator</param>
        private void InitializeCentroid(int k) {
            for (int n = 0; n < _n; n++)
                _centroids[k][n] = _random.NextDouble(); // set to a random double between 0.0 and 1.0 
        }

        /// <summary>
        /// Move centroids based on their respective data members 
        /// </summary>
        /// <returns>{-1} if operation was successful, otherwise returns centroid index that did not contain any members</returns>
        private int MoveCentroids() {

            int result = -1;
            double[][] centroids = EmptyArray(_k, _n);
            int[] memberCount = new int[_k];

            // iterate of each data row 
            for (int m = 0; m < _m; m++) {

                // reference this data row's centroid membership 
                int k = _membership[m];
                // increment centroid membership counter 
                memberCount[k]++;

                // add feature values 
                for (int n = 0; n < _n; n++)
                    centroids[k][n] += _data[m][n];
            }

            for (int k = 0; k < _k; k++) {
                // verify that the current centroid has at least one member 
                if (memberCount[k] > 0) {
                    // average each feature 
                    for (int n = 0; n < _n; n++)
                        centroids[k][n] = centroids[k][n] / memberCount[k];
                }
                else {
                    // this centroid has no members, stop movement operation and return a result of false 
                    result = k;
                    break;
                }
            }

            if (result == -1)
                _centroids = centroids;

            return result;
        }

        private double[][] EmptyArray(int rows, int cols) {
            double[][] newArray = new double[rows][];
            for (int i = 0; i < rows; i++)
                newArray[i] = new double[cols];
            return newArray;
        }
    }

    public class FitResult {

        public FitResult() {
            State = FitResultState.Success; 
        }

        public FitResultState State { get; set; }
        public int RetryCount { get; set; }
        public FitStep Step { get; set; }
    }

    public class FitStep {
        public Guid Id { get; set; }
        public int Iteration { get; set; }
        public double Cost { get; set; }
        public int[] Membership { get; set; }
        public double[][] Centroids { get; set; }
    }

    public class FitStepFailure {
        public Guid Id { get; set; }
        public int Iteration { get; set; }
        public int RetryCount { get; set; }
        public int Orphan { get; set; }
    }

    public enum FitResultState {
        Success, 
        ExceededRetryThreshold 
    }
}
