// Fun with Flags Demo 08 - OpenFeature / flagd / flipt - docker
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenFeature;
using OpenFeature.Contrib.Providers.Flagd;
using OpenFeature.Contrib.Providers.Flipt;
using OpenFeature.Model;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
var importantServiceLogger = factory.CreateLogger<ImportantService>();

var backend = configuration["backend"];

FeatureProvider featureProvider = backend switch
{
    "flagd" => new FlagdProvider(new Uri("http://localhost:8013")),
    "flipt" => new FliptProvider("http://localhost:8080", "default", ""),
    _ => throw new ArgumentException($"Invalid backend: {backend}")
};

await OpenFeature.Api.Instance.SetProviderAsync(featureProvider);
var featureClient = OpenFeature.Api.Instance.GetClient("Demo08");

var importantService = new ImportantService(importantServiceLogger, featureClient);

var result = await importantService.DoSomething("World");
Console.WriteLine(result);

public static class Features
{
    public const string ShinyNewFeature = "ShinyNewFeature";
}

public class ImportantService
{
    private readonly ILogger<ImportantService> logger;
    private readonly OpenFeature.IFeatureClient featureManager;
    
    public ImportantService(ILogger<ImportantService> logger, OpenFeature.IFeatureClient featureManager)
    {
        this.logger = logger;
        this.featureManager = featureManager;
    }

    public async Task<string> DoSomething(string input)
    {
        // Just to show that this exists
        var context = EvaluationContext.Builder().Set("email", "murph@flags.test").Build();
        var showTime = await featureManager.GetBooleanValueAsync(Features.ShinyNewFeature, false, context);
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