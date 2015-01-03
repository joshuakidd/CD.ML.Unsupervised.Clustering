using System.Web;
using System.Web.Optimization; 

namespace CD.ML.Unsupervised.Clustering.GUI {
    public class BundleConfig {

        public static void RegisterBundles(BundleCollection bundles) {

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                        "~/Content/lib/angular.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Content/lib/bootstrap/bootstrap.min.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/d3").Include(
                "~/Content/lib/d3/d3.min.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Content/lib/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                        "~/Content/lib/jquery.signalR-2.1.2.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/Content/js/app.js",
                "~/Content/js/services/signalRHubProxy.js",
                "~/Content/js/controllers/clusteringCtrl.js")
            );

            bundles.Add(new StyleBundle("~/styles/app").Include(
                "~/Content/css/bootstrap.min.css",
                "~/Content/fonts/font-awesome-4.2.0/css/font-awesome.min.css",
                "~/Content/css/app.css")
            );
        }
    }
}