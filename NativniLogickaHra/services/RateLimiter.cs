namespace NativniLogickaHra.Services;

public class RateLimiter
{
    private readonly int _maxCalls;
    private readonly TimeSpan _window;

    private readonly Queue<DateTime> _calls = new();
    private readonly object _lock = new();

    public RateLimiter(int maxCalls, TimeSpan window)
    {
        _maxCalls = maxCalls;
        _window = window;
    }

    public bool TryAcquire(out TimeSpan retryAfter)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;

            // odstranění starých volání
            while (_calls.Count > 0 && now - _calls.Peek() > _window)
                _calls.Dequeue();

            if (_calls.Count >= _maxCalls)
            {
                retryAfter = _window - (now - _calls.Peek());
                return false;
            }

            _calls.Enqueue(now);
            retryAfter = TimeSpan.Zero;
            return true;
        }
    }
}
