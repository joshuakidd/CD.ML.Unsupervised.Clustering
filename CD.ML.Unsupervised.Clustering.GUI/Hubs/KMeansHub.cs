using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace CD.ML.Unsupervised.Clustering.GUI {

    [HubName("kmeansHub")]
    public class KMeansHub : Hub {
        
        public Guid Fit(double[][] data, int k, int sleep){
            return KMeansManager.Instance.Fit(Context.ConnectionId, data, k, sleep); 
        }

        public double[][] Sample(int size) {
            return DataHelper.Sample(size, 2);
        }
    }
}