using System.Web.Optimization;

namespace CosmeticShop
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Создание бандла для JavaScript файлов
            bundles.Add(new ScriptBundle("~/bundles/javascript").Include(
                "~/Scripts/jquery.min.js",
                "~/Scripts/popper.min.js",
                "~/Scripts/bootstrap.bundle.min.js",
                "~/Scripts/jquery-3.0.0.min.js",
                "~/Scripts/plugin.js",
                "~/Scripts/jquery.mCustomScrollbar.concat.min.js",
                "~/Scripts/custom.js"));

            // Создание бандла для CSS файлов
            bundles.Add(new StyleBundle("~/Content/styles").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/style.css",
                "~/Content/responsive.css",
                "~/Content/jquery.mCustomScrollbar.min.css",
                "~/Content/owl.carousel.min.css"));

            // Установка режима сжатия и объединения файлов
            BundleTable.EnableOptimizations = true;
        }
    }
}