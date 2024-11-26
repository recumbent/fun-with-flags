// Fun with Flags Demo 11 - flipt cloud native client
using FliptClient;
using FliptClient.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddCommandLine(args)
    .Build();

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
var importantServiceLogger = factory.CreateLogger<ImportantService>();

// Set the flagdProvider as the provider for the OpenFeature SDK
// var flagdProvider = new FlagdProvider(new Uri("http://localhost:8013"));

// Set the fliptProvider as the provider for the OpenFeature SDK
var apiKey = configuration["flipt:apikey"] ?? throw new ArgumentNullException("flipt:apiKey");

var options = new ClientOptions
  {
    Url = "https://features-recumbent.flipt.cloud",
    Authentication = new Authentication { ClientToken = apiKey }
  };

var featureClient = new EvaluationClient("default", options);

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
    private readonly EvaluationClient featureManager;
    
    public ImportantService(ILogger<ImportantService> logger, EvaluationClient featureManager)
    {
        this.logger = logger;
        this.featureManager = featureManager;
    }

    public async Task<string> DoSomething(string input)
    {
        var showTime = (featureManager.EvaluateBoolean("ShinyNewFeature", "", new())).Enabled;
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