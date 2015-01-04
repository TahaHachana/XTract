module XTract.Throttler

open System
open System.Collections.Generic

type Message = Quit | Work

let throttle (asyncs:seq<Async<unit>>) limit callback =
 
    let q = Queue(asyncs)
    
    let dequeue() = try q.Dequeue() |> Some with _ -> None
    
    let agent =
        MailboxProcessor.Start(fun x ->
            let rec loop count =
                async {
                    let! msg = x.Receive()
                    match msg with
                    | Work ->
                        let work = dequeue()
                        match work with
                        | Some work' ->
                            async {
                                try
                                    do! work'
                                finally
                                    x.Post Work
                            } |> Async.Start
                            return! loop count
                        | None ->
                            x.Post Quit
                            return! loop (count + 1)
                    | Quit ->
                        match count with
                        | _ when count = limit ->
                            callback |> Async.Start
                            (x:> IDisposable).Dispose()
                        | _ -> return! loop count
                }
            loop 0
        )
    [1 .. limit] |> List.iter (fun _ -> agent.Post Work)