// Fun with Flags Demo 03
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
var importantServiceLogger = factory.CreateLogger<ImportantService>();

var featureManager = new FeatureManager(configuration);
var importantService = new ImportantService(importantServiceLogger, featureManager);

var result = importantService.DoSomething("World");
Console.WriteLine(result);

public interface IFeatureManager
{
    bool IsEnabled(string feature);
}

public class FeatureManager(IConfiguration configuration) : IFeatureManager
{
    public static readonly string ShinyNewFeature = "ShinyNewFeature";

    public bool IsEnabled(string feature) => configuration.GetValue<bool>($"FeatureManagement:{feature}");
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

    public string DoSomething(string input)
    {
        var showTime = featureManager.IsEnabled(FeatureManager.ShinyNewFeature);
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