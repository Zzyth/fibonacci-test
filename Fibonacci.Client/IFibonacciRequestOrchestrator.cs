namespace Fibonacci.Client;

public interface IFibonacciRequestOrchestrator
{
    public void StartRequesting(int startFromStep, string session);
    public void StopRequesting();
}