namespace Fibonacci.Calculator.Services;

public class FibonacciCalculator : IFibonacciCalculator
{
    /// <summary>
    /// Вычисляем  i-тое число Фибоначи по формуле Бине
    /// Можно было реализовать классический способ вычисления как N(i) = N(i-1) + N(i-2)
    /// и кешировать какждый i-тый член последовательности для ускорения вычисления следующего
    /// но это решение проще и быстрее
    /// </summary>
    public double GetFibonacciSequenceItem(int number)
    {
        var index =  Math.Pow(5, 0.5);
        var left = (1 + index) / 2;
        var right = (1 - index) / 2;
        return Math.Round((Math.Pow(left, number) - Math.Pow(right, number)) / index);
    }
}