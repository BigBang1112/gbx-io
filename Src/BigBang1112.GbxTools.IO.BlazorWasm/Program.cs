using BigBang1112.GbxTools.IO;
using BigBang1112.GbxTools.IO.BlazorWasm;
using BigBang1112.GbxTools.IO.BlazorWasm.Services;
using BigBang1112.GbxTools.IO.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddGbxIo();
builder.Services.AddTransient<IDownloadService, DownloadService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
