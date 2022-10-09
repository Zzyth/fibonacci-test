// See https://aka.ms/new-console-template for more information

using Fibonacci.Client;
using Fibonacci.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

Console.WriteLine("Hello, World!");

// setup configuration
IConfiguration Configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

// set up DI
var services = new ServiceCollection();
services.Configure<MessageBusConfig>(Configuration.GetSection(nameof(MessageBusConfig)));

services.AddHttpClient<IFibonacciRequestOrchestrator, FibonacciRequestOrchestrator>();

services.AddLogging(configure => configure.AddConsole())
    .AddTransient<IFibonacciRequestOrchestrator, FibonacciRequestOrchestrator>(x =>
    {
        var threadCount = Configuration.GetValue<int>("threadCount");
        var address = Configuration.GetValue<string>("serviceUrl");

        if (string.IsNullOrEmpty(address))
        {
            throw new Exception("Invalid application configuration");
        }
        
        var httpClientFactory = services.BuildServiceProvider().GetService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(address);
        return new FibonacciRequestOrchestrator(client, threadCount < 1 ? 1 : threadCount);
    })
    .AddTransient<IFibonacciResultOrchestrator, FibonacciResultOrchestrator>()
    .AddTransient<IFibonacciResultHandler, FibonacciResultHandler>()
    .AddSingleton<IMessageBusConfig>(sp =>
        sp.GetRequiredService<IOptions<MessageBusConfig>>().Value);

var serviceProvider = services.BuildServiceProvider();


// work itself
var requester = serviceProvider.GetService<IFibonacciRequestOrchestrator>();
var receiver = serviceProvider.GetService<IFibonacciResultOrchestrator>();

if (requester == null || receiver == null)
{
    throw new Exception("Invalid DI configuration");
}

const int initStep = 1;
var sessionId = Guid.NewGuid().ToString();

await receiver.StartListenResults(initStep, sessionId);
requester.StartRequesting(initStep, sessionId);

while (Console.ReadLine() != "q")
{
    requester.StopRequesting();
    receiver.StopListenResults();
}
