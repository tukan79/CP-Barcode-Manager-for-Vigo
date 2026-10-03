using CPBarcodeManager.Service;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "CP Barcode Manager for Vigo";
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
