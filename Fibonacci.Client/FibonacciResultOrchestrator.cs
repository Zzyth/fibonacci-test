using EasyNetQ;
using Fibonacci.Shared;

namespace Fibonacci.Client;

public class FibonacciResultOrchestrator : IFibonacciResultOrchestrator
{
    private readonly IFibonacciResultHandler _resultHandler;
    private int _step;
    private readonly PriorityQueue<FibonacciMessage, int> _queue = new();
    private string _session = string.Empty;
    private readonly IBus _bus;
    private SubscriptionResult _subscription;
    private bool _isInProgress;
    private readonly object _syncObj = new ();
    private readonly SemaphoreSlim _syncSemaphore = new(1);

    public FibonacciResultOrchestrator(IFibonacciResultHandler resultHandler, IMessageBusConfig config)
    {
        // TODO add logging
        _resultHandler = resultHandler;

        // TODO add circuit breaker
        // tried to use EasyNetQ.PollyHandlerRunner
        // but it is not compatible with EasyNetQ v7 any more
        _bus = RabbitHutch.CreateBus(config.Url);
    }
    
    public async Task StartListenResults(int startStep, string sessionId)
    {
        lock (_syncObj)
        {
            if (_isInProgress)
            {
                throw new InvalidOperationException("listening already started");
            }

            _isInProgress = true;
        }
        
        _session = sessionId;
        _step = startStep;
        _subscription = await _bus.PubSub.SubscribeAsync<FibonacciMessage>(
            $"fibonacci_subscription_{_session}", 
            async x => await NewResult(x), 
            x=> x.WithTopic($"fibonacci{_session}"));
    }

    public void StopListenResults()
    {
        lock (_syncObj)
        {
            if (!_isInProgress) return;
            _isInProgress = false;
            _subscription.Dispose();
        }
    }

    private async Task NewResult(FibonacciMessage msg)
    {
        _queue.Enqueue(msg, msg.Number);
        
        await _syncSemaphore.WaitAsync();
        try {
            await Kick();
        } finally {
            _syncSemaphore.Release();
        }
    }

    private async Task Kick()
    {
        var topMessage = _queue.Peek();
        while (topMessage != null && topMessage.Number == _step)
        {
            var item = _queue.Dequeue();
            await _resultHandler.ProcessMessageAsync(item);
            _step++;
            topMessage = _queue.Count > 0 ? _queue.Peek() : null;
        }
    }
}