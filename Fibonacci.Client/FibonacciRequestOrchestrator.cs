namespace Fibonacci.Client;

public class FibonacciRequestOrchestrator : IFibonacciRequestOrchestrator
{
    private readonly HttpClient _client;
    private readonly int _threadCount;
    private readonly int _retryCount;
    
    private readonly List<CancellationTokenSource> _cancellationTokens = new();

    private string _sessionId = string.Empty;
    private bool _isInProgress;
    private readonly object _syncObj = new();
    private readonly object _requestLock = new();
    
    private int _stepNumber = 1;

    public FibonacciRequestOrchestrator(HttpClient client, int threadCount, int retryCount = 5)
    {
        // TODO add logging
        _retryCount = retryCount;
        _client = client;
        _threadCount = threadCount;
    }
    
    public void StartRequesting(int startFromStep, string session)
    {
        lock (_syncObj)
        {
            if (_isInProgress)
            {
                throw new InvalidOperationException("already started");
            }

            _isInProgress = true;
        }

        _sessionId = session;
        _stepNumber = startFromStep;
        StartTasks(_threadCount);
    }

    public void StopRequesting()
    {
        foreach (var cancellationTokenSource in _cancellationTokens)
        {
            cancellationTokenSource.Cancel();
        }
        _cancellationTokens.Clear();
        lock (_syncObj)
        {
            _isInProgress = false;
            _sessionId = string.Empty;
        }
    }

    private void StartTasks(int taskCount)
    {
        for (int i = 0; i < taskCount; i++)
        {
            var source = new CancellationTokenSource();
            Task.Run(() => RequestAsync(source.Token), source.Token);
            _cancellationTokens.Add(source);
        }
    }
    
    private async Task RequestAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            int number;
            lock (_requestLock)
            {
                number = _stepNumber;
                _stepNumber++;
            }
            
            HttpResponseMessage? response = null;
            for (var i = 0; i < _retryCount; i++)
            {
                response = await _client.GetAsync($"fibonacci/{number}?session={_sessionId}", cancellationToken);
                if (response.IsSuccessStatusCode) {
                    break;
                }
                Thread.Sleep(100); // wait some time if request was unsuccessful 
            }

            if (response is not {IsSuccessStatusCode: true})
            {
                throw new Exception("something very bad happens with fibonacci calculator service");
            }
        }
    }
}