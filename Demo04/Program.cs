// Fun with Flags Demo 04 - Branch by abstraction
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddCommandLine(args)
    .Build();

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
var switchLogger = factory.CreateLogger<SwitchingImportantService>();
var oldServiceLogger = factory.CreateLogger<OriginalImportantService>();
var newServiceLogger = factory.CreateLogger<ShinyNewImportantService>();

var featureManager = new FeatureManager(configuration);

IImportantService oldService = new OriginalImportantService(oldServiceLogger);
IImportantService newService = new ShinyNewImportantService(newServiceLogger);
var importantService = new SwitchingImportantService(switchLogger, oldService, newService, featureManager);

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

public class SwitchingImportantService : IImportantService
{
    private readonly ILogger<SwitchingImportantService> logger;
    private readonly IImportantService oldService;
    private readonly IImportantService newService;
    private readonly IFeatureManager featureManager;
    
    public SwitchingImportantService(
        ILogger<SwitchingImportantService> logger,
        IImportantService oldService,
        IImportantService newService,
        IFeatureManager featureManager)
    {
        this.logger = logger;
        this.oldService = oldService;
        this.newService = newService;
        this.featureManager = featureManager;
    }

    public string DoSomething(string name)
    {
        var showTime = featureManager.IsEnabled(FeatureManager.ShinyNewFeature);
        logger.LogInformation("ShinyNewFeature {IsEnabled}", showTime);

        return showTime switch
        {
            true => this.newService.DoSomething(name),
            _ => this.oldService.DoSomething(name)
        };
    }
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