
using System.Text.Json;
using BuildUtils;

using IHost host = Host.CreateDefaultBuilder(args)
    //.ConfigureServices((_, services) => services.AddGitHubActionServices())
    .Build();


static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();



static async Task Execute(ActionInputs inputs, IHost host)
{
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });
    var logger = loggerFactory.CreateLogger<App>();

    var file = inputs.SettingsFile;
    Settings settings;
    using (var str = File.OpenRead(file))
    {
        str.Seek(0, SeekOrigin.Begin);
        str.Position = 0;
        settings= JsonSerializer.Deserialize<Settings>(str, new JsonSerializerOptions(){PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
    }
   
    
    
    var app = new App(settings,new ExecutionConfiguration(),logger);
    bool done = await app.Execute();

}
    
var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    errors =>
    {
        Get<ILoggerFactory>(host)
            .CreateLogger("Community.GitHubAction.Build")
            .LogError(
                string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

        Environment.Exit(2);
    });

await parser.WithParsedAsync(options => Execute(options, host));
await host.RunAsync();