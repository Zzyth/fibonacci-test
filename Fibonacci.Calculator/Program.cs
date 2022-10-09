using Fibonacci.Calculator.Services;
using Fibonacci.Shared;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<MessageBusConfig>(
    builder.Configuration.GetSection(nameof(MessageBusConfig)));

// Add services to the container.
builder.Services.AddTransient<IFibonacciService, FibonacciService>();
builder.Services.AddTransient<IFibonacciCalculator, FibonacciCalculator>();
builder.Services.AddSingleton<IMessageBusConfig>(sp =>
    sp.GetRequiredService<IOptions<MessageBusConfig>>().Value);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();