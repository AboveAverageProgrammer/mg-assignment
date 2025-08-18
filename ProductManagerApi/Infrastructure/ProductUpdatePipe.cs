using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace ProductManagerApi.Infrastructure;

public record KeyedWork<TKey>(
    TKey Key,
    Func<CancellationToken, Task> Work,
    TaskCompletionSource<bool> Tcs,
    CancellationToken CallCt);

public class ProductUpdatePipeOptions
{
    public int DegreeOfParallelism { get; init; } = Environment.ProcessorCount;
    public int BoundedCapacity { get; init; } = 1024;
    public int MaxRetries { get; init; } = 0;
    public TimeSpan RetryBaseDelay { get; init; } = TimeSpan.FromMilliseconds(200);
    public Func<Exception, bool>? IsTransient { get; init; }
}

public interface IProductUpdatePipe
{
    Task RunForProduct(int productId, Func<CancellationToken, Task> work, CancellationToken ct = default);
    Task<bool> TryRunForProduct(int productId, Func<CancellationToken, Task> work, TimeSpan timeout, CancellationToken ct = default);
    Task DrainAndCompleteAsync();
}

public class ProductUpdatePipe : IProductUpdatePipe, IAsyncDisposable
{
    private readonly ConcurrentDictionary<int, Task> _tails = new();
    private readonly ActionBlock<KeyedWork<int>> _block;
    private readonly ProductUpdatePipeOptions _opts;

    public ProductUpdatePipe(ProductUpdatePipeOptions? options = null)
    {
        _opts = options ?? new ProductUpdatePipeOptions();

        var execOpts = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = _opts.DegreeOfParallelism,
            BoundedCapacity = _opts.BoundedCapacity,
            EnsureOrdered = false,
        };

        _block = new ActionBlock<KeyedWork<int>>(ProcessAsync, execOpts);
    }

    public async Task RunForProduct(int productId, Func<CancellationToken, Task> work, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var msg = new KeyedWork<int>(productId, work, tcs, ct);
        await _block.SendAsync(msg, ct).ConfigureAwait(false);
        await tcs.Task.ConfigureAwait(false);
    }

    public async Task<bool> TryRunForProduct(int productId, Func<CancellationToken, Task> work, TimeSpan timeout, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var msg = new KeyedWork<int>(productId, work, tcs, ct);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        bool enqueued;
        try
        {
            enqueued = await _block.SendAsync(msg, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) 
        { 
            return false; 
        }

        if (!enqueued)
        {
            return false;
        }

        try
        {
            await tcs.Task.WaitAsync(ct).ConfigureAwait(false);
            return true; 
        }
        catch (OperationCanceledException)
        { 
            return false; 
        }
    }

    private Task ProcessAsync(KeyedWork<int> msg)
    {
        var tail = _tails.AddOrUpdate(
            msg.Key,
            _ => RunWithPoliciesAsync(Task.CompletedTask, msg),
            (_, existingTail) => RunWithPoliciesAsync(existingTail, msg));

        tail.ContinueWith(t =>
        {
            _tails.TryGetValue(msg.Key, out var current);

            if (ReferenceEquals(current, t))
            {
                _tails.TryRemove(msg.Key, out _);
            }
        }, TaskScheduler.Default);

        return Task.CompletedTask;
    }

    private async Task RunWithPoliciesAsync(Task prior, KeyedWork<int> msg)
    {
        try 
        { 
            await prior.ConfigureAwait(false);
        } catch { }

        var attempt = 0;
        for (; ; )
        {
            msg.CallCt.ThrowIfCancellationRequested();
            try
            {
                await msg.Work(msg.CallCt).ConfigureAwait(false);
                msg.Tcs.TrySetResult(true);
                return;
            }
            catch (Exception ex)
            {
                var canRetry = attempt < _opts.MaxRetries && (_opts.IsTransient?.Invoke(ex) ?? false);
                if (!canRetry)
                {
                    msg.Tcs.TrySetException(ex);
                    return;
                }

                attempt++;
                var delay = TimeSpan.FromMilliseconds(_opts.RetryBaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));

                try
                {
                    await Task.Delay(delay, msg.CallCt).ConfigureAwait(false);
                }
                catch (OperationCanceledException oce)
                {
                    msg.Tcs.TrySetException(oce);
                    return;
                }
            }
        }
    }

    public async Task DrainAndCompleteAsync()
    {
        _block.Complete();
        await _block.Completion.ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await DrainAndCompleteAsync().ConfigureAwait(false);
    }
}