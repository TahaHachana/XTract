module XTract.Async

open System.Threading
open System.Timers
open System.Threading.Tasks

/// Executes a computation in the thread pool. The computation
/// is cancelled if it doesn't complete within the specified
/// interval of milliseconds.
let StartWithTimeout interval computation =
    async {
        try
            use cancellationSource = new CancellationTokenSource()
            let cancellationToken = cancellationSource.Token
            use timer = new Timer(interval)
            timer.Elapsed.Add(fun _ -> cancellationSource.Cancel())
            timer.Start()
            let task = Async.StartAsTask(computation, cancellationToken = cancellationToken)
            task.Wait()
            match task.Status with
            | TaskStatus.RanToCompletion -> return task.Result
            | _ -> return None
        with _ -> return None
    }