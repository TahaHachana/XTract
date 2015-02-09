module XTract.Throttler

open System.Collections.Concurrent

type Message =
    | Work
    | WorkAndReply of AsyncReplyChannel<unit>
    | Pause
    | Resume
    | Quit

type ThrottlingAgent(?Limit) as this =

    let limit = defaultArg Limit 5
    let stack = ConcurrentStack<Async<unit>>()
    [<DefaultValue>] val mutable private replyChannel : AsyncReplyChannel<unit>

    let dequeue() =
        match stack.TryPop() with
        | false, _ -> None
        | true, workflow -> Some workflow

    let agent =
        MailboxProcessor.Start(fun x ->
            let rec loop count doWork =
                async {
                    let! msg = x.Receive()
                    match msg with
                    | Pause -> return! loop count false
                    | Resume ->
                        [1 .. limit] |> List.iter (fun _ -> x.Post Work)
                        return! loop count true
                    | WorkAndReply reply ->
                        this.replyChannel <- reply
                        [1 .. limit] |> List.iter (fun _ -> x.Post Work)
                        return! loop count true
                    | Work ->
                        match doWork with
                        | false -> return! loop count false
                        | true ->
                            let work = dequeue()
                            match work with
                            | Some work' ->
                                async {
                                    try
                                        do! work'
                                    finally
                                        x.Post Work
                                } |> Async.Start
                                return! loop count true
                            | None ->
                                x.Post Quit
                                return! loop (count + 1) true
                    | Quit ->
                        match count with
                        | _ when count = limit ->
                            this.replyChannel.Reply()
                            return! loop 0 true
                        | _ -> return! loop count true
                }
            loop 0 true
        )

    /// Throttles running the specified async workflows.
    member __.Work asyncs =
        stack.PushRange <| Seq.toArray asyncs
        agent.PostAndAsyncReply(fun replyChannel -> WorkAndReply replyChannel)

    /// Returns the number of remaining async workflows to execute.
    member __.RemainingTasks = stack.Count

    /// Pauses the throttler.
    member __.Pause() = agent.Post Pause

    /// Resumes running the async workflows.
    member __.Resume() = agent.Post Resume

    /// Cancels running the remaining async workflows.
    member __.CancelRemainingTasks() = stack.Clear()

//let asyncs =
//    [1 .. 10]
//    |> List.map (fun x ->
//        async {
//            do! Async.Sleep (x * 1000)
//            printfn "%d" x
//        })
//
//let test = ThrottlingAgent()
//
//test.Work asyncs
//|> Async.Start
//
//test.RemainingTasks
//
//test.Pause()
//
//test.Resume()


//let throttle (asyncs:seq<Async<unit>>) limit callback =
// 
//    let q = Queue(asyncs)
//    
//    let dequeue() = try q.Dequeue() |> Some with _ -> None
//    
//    let agent =
//        MailboxProcessor.Start(fun x ->
//            let rec loop count =
//                async {
//                    let! msg = x.Receive()
//                    match msg with
//                    | Work ->
//                        let work = dequeue()
//                        match work with
//                        | Some work' ->
//                            async {
//                                try
//                                    do! work'
//                                finally
//                                    x.Post Work
//                            } |> Async.Start
//                            return! loop count
//                        | None ->
//                            x.Post Quit
//                            return! loop (count + 1)
//                    | Quit ->
//                        match count with
//                        | _ when count = limit ->
//                            callback |> Async.Start
//                            (x:> IDisposable).Dispose()
//                        | _ -> return! loop count
//                }
//            loop 0
//        )
//    [1 .. limit] |> List.iter (fun _ -> agent.Post Work)