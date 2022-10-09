namespace Fibonacci.Client;

public interface IFibonacciResultOrchestrator
{
    Task StartListenResults(int startStep, string sessionId);
    void StopListenResults();
}