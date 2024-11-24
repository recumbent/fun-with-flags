// Fun with Flags Demo 07 - Open Feature with Microsoft Feature Management provider (if it works)
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenFeature;
using OpenFeature.Contrib.Providers.FeatureManagement;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
var importantServiceLogger = factory.CreateLogger<ImportantService>();

var featureManagementProvider = new FeatureManagementProvider(configuration);
await OpenFeature.Api.Instance.SetProviderAsync(featureManagementProvider);
var featureClient = OpenFeature.Api.Instance.GetClient();

var importantService = new ImportantService(importantServiceLogger, featureClient);

var result = await importantService.DoSomething("World");
Console.WriteLine(result);

public static class Features
{
    public const string ShinyNewFeature = "ShinyNewFeature06";
}

public class ImportantService
{
    private readonly ILogger<ImportantService> logger;
    private readonly IFeatureClient featureManager;
    
    public ImportantService(ILogger<ImportantService> logger, IFeatureClient featureManager)
    {
        this.logger = logger;
        this.featureManager = featureManager;
    }

    public async Task<string> DoSomething(string input)
    {
        var showTime = await featureManager.GetBooleanValueAsync(Features.ShinyNewFeature, false, null);
        logger.LogInformation("ShinyNewFeature {IsEnabled}", showTime);

        return showTime switch
        {
            true => NewImplementation(input),
            _ => OldImplementation(input)
        };

        string OldImplementation(string input)
        {
            return $"Hello, {input}!";
        }

        string NewImplementation(string input)
        {
            return $"Hello, {input}!{Environment.NewLine}The time now is: {DateTime.Now}";
        }
    }
}