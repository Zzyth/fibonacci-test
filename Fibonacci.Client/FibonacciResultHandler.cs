using Fibonacci.Shared;

namespace Fibonacci.Client;

public class FibonacciResultHandler : IFibonacciResultHandler
{
    public Task ProcessMessageAsync(FibonacciMessage message)
    {
        return Task.Run(() => Console.WriteLine($"new Fibonacci value received: step={message.Number}\tvalue={message.Value}"));
    }
}