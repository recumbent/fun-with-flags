// Fun with Flags Demo 06 - Microsoft Feature Management
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
var importantServiceLogger = factory.CreateLogger<ImportantService>();

var configDefinitionProvider = new ConfigurationFeatureDefinitionProvider(configuration);

var featureManager = new FeatureManager(configDefinitionProvider)
{
    FeatureFilters = [new TimeWindowFilter()]
};

var importantService = new ImportantService(importantServiceLogger, featureManager);

var result = await importantService.DoSomething("World");
Console.WriteLine(result);

public static class Features
{
    public const string ShinyNewFeature = "ShinyNewFeature";
}

public class ImportantService
{
    private readonly ILogger<ImportantService> logger;
    private readonly IFeatureManager featureManager;
    
    public ImportantService(ILogger<ImportantService> logger, IFeatureManager featureManager)
    {
        this.logger = logger;
        this.featureManager = featureManager;
    }

    public async Task<string> DoSomething(string input)
    {
        var showTime = await featureManager.IsEnabledAsync(Features.ShinyNewFeature);
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