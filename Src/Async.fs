module XTract.Async

open System.Threading
open System.Threading.Tasks
open System.Timers

let timeout interval computation =
    async {
        try
            use cancelSource = new CancellationTokenSource()
            use timer = new Timer(interval)
            timer.Elapsed.Add(fun _ -> cancelSource.Cancel())
            timer.Start()
            let task = Async.StartAsTask(computation, cancellationToken = cancelSource.Token)
            task.Wait()
            match task.Status with
            | TaskStatus.RanToCompletion -> return task.Result
            | _ -> return None
        with _ -> return None
    }
