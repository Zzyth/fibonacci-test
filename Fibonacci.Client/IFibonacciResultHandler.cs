using Fibonacci.Shared;

namespace Fibonacci.Client;

public interface IFibonacciResultHandler
{
    Task ProcessMessageAsync(FibonacciMessage message);
}