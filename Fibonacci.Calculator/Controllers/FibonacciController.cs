using Fibonacci.Calculator.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fibonacci.Calculator.Controllers;

[ApiController]
[Route("[controller]")]
public class FibonacciController : ControllerBase
{
    private readonly IFibonacciService _service;

    public FibonacciController(IFibonacciService service)
    {
        _service = service;
    }
    
    [HttpGet("{number}")]
    public async Task<IActionResult> GetFibonacciNumber(int number, [FromQuery] string session)
    {
        await _service.CalculateFibonacciNumberAsync(number, session);
        return Ok();
    }
}