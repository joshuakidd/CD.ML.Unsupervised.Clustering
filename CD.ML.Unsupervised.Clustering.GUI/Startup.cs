using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(CD.ML.Unsupervised.Clustering.GUI.Startup))]
namespace CD.ML.Unsupervised.Clustering.GUI {

    public class Startup {

        public void Configuration(IAppBuilder app) {
            app.MapSignalR();
        }
    }
}