namespace Fibonacci.Calculator.Services;

public interface IFibonacciService
{
    Task CalculateFibonacciNumberAsync(int number, string session = "");
}