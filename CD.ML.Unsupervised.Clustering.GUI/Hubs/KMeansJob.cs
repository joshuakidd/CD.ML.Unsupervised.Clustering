using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.ML.Unsupervised.Clustering.GUI {
    public class KMeansJob : KMeans {

        public string ClientId { get; set; }

        public KMeansJob(string clientId, Matrix<double> data, int k, int sleep) : base(data, k, sleep: sleep) {
            ClientId = clientId; 
        }
    }
}