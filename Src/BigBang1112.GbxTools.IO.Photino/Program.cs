using BigBang1112.GbxTools.IO.Photino.Services;
using BigBang1112.GbxTools.IO.Services;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;

namespace BigBang1112.GbxTools.IO.Photino
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var builder = PhotinoBlazorAppBuilder.CreateDefault(args);
            builder.Services.AddLogging();

            builder.Services.AddGbxIo();
            builder.Services.AddTransient<IDownloadService, DownloadService>();

            // register root component
            builder.RootComponents.Add<App>("app");

            var app = builder.Build();

            // customize window
            app.MainWindow
                .SetSize(1280, 720)
                .SetIconFile("favicon.ico")
                .SetTitle("MyBlazorPhotinoHybrid");

            AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
            {
                app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());
            };

            app.Run();
        }
    }
}