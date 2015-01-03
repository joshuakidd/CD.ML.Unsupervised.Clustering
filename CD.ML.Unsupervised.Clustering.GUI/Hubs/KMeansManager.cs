using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CD.ML.Unsupervised.Clustering.GUI {
    public class KMeansManager {

        // Singleton instance
        private readonly static Lazy<KMeansManager> _instance
            = new Lazy<KMeansManager>(() => 
                new KMeansManager(GlobalHost.ConnectionManager.GetHubContext<KMeansHub>().Clients));

        // collection of running kmeans jobs 
        private ConcurrentDictionary<Guid, KMeansJob> _jobs = new ConcurrentDictionary<Guid, KMeansJob>(); 

        private IHubConnectionContext<dynamic> Clients { get; set; }

        private KMeansManager(IHubConnectionContext<dynamic> clients) {
            Clients = clients;
        }

        public static KMeansManager Instance {
            get {
                return _instance.Value;
            }
        }

        public Guid Fit(string clientId, double[][] data, int k, int sleep) {

            // create a new matrix using the specified data 
            Matrix<double> matrix = new Matrix<double>(data);
            // create kmeans job 
            KMeansJob job = new KMeansJob(clientId, matrix, k, sleep);
            // wire up events 
            job.OnFitStep += StepNotification;
            job.OnFitStepFailure += FailureNotification;
            // add job to dictionary of running jobs 
            AddJob(job); 

            // asynchronously fit the data using the kmeans clustering algorithm 
            Task.Factory.StartNew(() => { 
                // fit the data 
                FitResult result = job.Fit(); 
                // notify the client that clustering has completed 
                CompleteNotification(result);
                // remove job from dictionary of running jobs 
                RemoveJob(job.Id); 
            }, 
            TaskCreationOptions.LongRunning);

            return job.Id; 
        }

        private void FailureNotification(FitStepFailure failure) {
            string clientId = GetClientId(failure.Id);
            Clients.Client(clientId).failureNotification(failure); 
        }

        private void StepNotification(FitStep step) {
            string clientId = GetClientId(step.Id);
            Clients.Client(clientId).stepNotification(step); 
            
        }

        private void CompleteNotification(FitResult result) {
            string clientId = GetClientId(result.Step.Id);
            Clients.Client(clientId).completeNotification(result); 
        }

        private bool AddJob(KMeansJob job) {
            return _jobs.TryAdd(job.Id, job); 
        }

        private bool RemoveJob(Guid id) {
            KMeansJob job; 
            return _jobs.TryRemove(id, out job); 
        }

        private string GetClientId(Guid id) {
            KMeansJob job;
            if (_jobs.TryGetValue(id, out job))
                return job.ClientId;
            else
                throw new InvalidOperationException(); 
        }
    }
}