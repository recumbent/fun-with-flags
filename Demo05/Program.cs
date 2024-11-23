// Fun with Flags Demo 05 - Branch by abstraction via a factory
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var serviceProvider = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)
    .AddLogging(builder => builder.AddConsole())
    .AddSingleton<IFeatureManager, FeatureManager>()
    .AddTransient<OriginalImportantService>()
    .AddTransient<ShinyNewImportantService>()
    .AddTransient<IImportantService>(sc =>
    {
        var fm = sc.GetRequiredService<IFeatureManager>();
        var enableShinyNewFeature = fm.IsEnabled(FeatureManager.ShinyNewFeature);

        return enableShinyNewFeature switch
        {
            true => sc.GetRequiredService<ShinyNewImportantService>(),
            _ => sc.GetRequiredService<OriginalImportantService>()
        };
    })
    .BuildServiceProvider();

var importantService = serviceProvider.GetRequiredService<IImportantService>();

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

public interface IImportantService
{
    string DoSomething(string name);
}

public class OriginalImportantService : IImportantService
{
    private readonly ILogger<OriginalImportantService> logger;
    
    public OriginalImportantService(ILogger<OriginalImportantService> logger)
    {
        this.logger = logger;
    }

    public string DoSomething(string input)
    {
        logger.LogInformation("Original Service");
         return $"Hello, {input}!";
    }
}

public class ShinyNewImportantService : IImportantService
{
    private readonly ILogger<ShinyNewImportantService> logger;
    
    public ShinyNewImportantService(ILogger<ShinyNewImportantService> logger)
    {
        this.logger = logger;
    }

    public string DoSomething(string input)
    {
        logger.LogInformation("Shiny New Service");
        return $"Hello, {input}!{Environment.NewLine}The time now is: {DateTime.Now}";
    }
}