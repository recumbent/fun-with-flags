// Fun with Flags Demo 02
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

var featureManager = new FeatureManager(configuration);

Console.WriteLine("Hello, World!");

if (featureManager.IsEnabled(FeatureManager.ShinyNewFeature))
{
    Console.WriteLine($"The time now is: {DateTime.Now}");
}

public class FeatureManager(IConfiguration configuration)
{
    public static readonly string ShinyNewFeature = "ShinyNewFeature";

    public bool IsEnabled(string feature) => configuration.GetValue<bool>($"FeatureManagement:{feature}");
}
    