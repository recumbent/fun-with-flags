// Fun with Flags Demo 01
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

Console.WriteLine("Hello, World!");

if (configuration["FeatureManagement:ShinyNewFeature"] == "true")
{
    Console.WriteLine($"The time now is: {DateTime.Now}");
}
