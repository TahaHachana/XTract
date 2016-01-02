[<AutoOpen>]
module XTract.Async

open System.Threading
open System.Threading.Tasks
open System.Timers

/// Checks the status of the supplied task recursively.
/// Returns the task's result if it ran to completion and
/// None if it faulted or was canceled.
//let rec private checkStatus (task: Task<'T option>) =
//    match task.Status with
//    | TaskStatus.Created
//    | TaskStatus.WaitingForActivation
//    | TaskStatus.WaitingToRun
//    | TaskStatus.Running -> checkStatus task
//    | TaskStatus.RanToCompletion -> task.Result
//    | _ -> None

type Async with

    /// <summary>
    /// Executes a computation in the thread pool. The computation
    /// is canceled if it doesn't complete within the specified
    /// milliseconds interval.
    /// </summary>
    /// <param name="millisecondsDueTimeout">The timeout interval.</param>
    /// <param name="computation">The asynchronous computation.</param>
    static member StartWithTimeout millisecondsDueTimeout computation =
        async {
            try
                use cancellationSource = new CancellationTokenSource()
                let cancellationToken = cancellationSource.Token

                use timer = new Timer(float millisecondsDueTimeout)         
                timer.Elapsed.Add(fun _ -> cancellationSource.Cancel())
                timer.Start()

                let task = Async.StartAsTask(computation, cancellationToken = cancellationToken)
                task.Wait()

//                while task.Status <> TaskStatus.RanToCompletion do ()
                let result = task.Result
                return result
            with _ -> return None
        }