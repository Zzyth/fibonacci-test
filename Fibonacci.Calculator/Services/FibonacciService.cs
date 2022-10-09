using EasyNetQ;
using EasyNetQ.PollyHandlerRunner;
using Fibonacci.Shared;
using Polly;

namespace Fibonacci.Calculator.Services;

public class FibonacciService : IFibonacciService
{
    private readonly IFibonacciCalculator _fibonacciCalculator;
    private readonly IMessageBusConfig _messageBusConfig;
    private readonly IBus _bus;

    public FibonacciService(IFibonacciCalculator fibonacciCalculator, IMessageBusConfig messageBusConfig)
    {
        _fibonacciCalculator = fibonacciCalculator;
        _messageBusConfig = messageBusConfig;
        
        // TODO add circuit breaker
        // tried to use EasyNetQ.PollyHandlerRunner
        // but it is not compatible with EasyNetQ v7 any more

        _bus = RabbitHutch.CreateBus(_messageBusConfig.Url);
    }
    
    public Task CalculateFibonacciNumberAsync(int number, string session = "")
    {
        var result = _fibonacciCalculator.GetFibonacciSequenceItem(number);
        var message = new FibonacciMessage
        {
            Number = number,
            Value = result
        };
        return _bus.PubSub.PublishAsync(message, $"fibonacci{session}");
    }
}