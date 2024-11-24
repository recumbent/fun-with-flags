// Fun with Flags Demo 11 - flipt cloud native client
using FliptClient;
using FliptClient.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

// Set the fliptProvider as the provider for the OpenFeature SDK
var apiKey = configuration["flipt:apikey"] ?? throw new ArgumentNullException("flipt:apiKey");

var options = new ClientOptions
  {
    Url = "https://features-recumbent.flipt.cloud",
    Authentication = new Authentication { ClientToken = apiKey },
    UpdateInterval = 10
};

var featureClient = new EvaluationClient("default", options);

var looper = new LoopingDemo(featureClient);

var cts = new CancellationTokenSource();
var token = cts.Token;
var task = looper.DoSomething(token);

Console.ReadKey();
cts.Cancel();

try
{
    await task;
}
catch(OperationCanceledException)
{
    Console.WriteLine("Cancelled.");
}
Console.WriteLine("Finis!");


public class LoopingDemo
{
    private EvaluationClient evaluationClient;

    public LoopingDemo(EvaluationClient evaluationClient)
    {
        this.evaluationClient = evaluationClient;
    }

    public async Task DoSomething(CancellationToken cancellationToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        var counter = 0;
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            var flipped = (evaluationClient.EvaluateBoolean("ShinyNewFeature", "", new())).Enabled;
            var displayString = flipped switch
            {
                true => ">>>>>>>>>>",
                _    => "<<<<<<<<<<"
            };
            
            Console.WriteLine($"{++counter:0000}: {displayString}");
        }
    }
}

public static class Features
{
    public const string ShinyNewFeature = "ShinyNewFeature";
}
