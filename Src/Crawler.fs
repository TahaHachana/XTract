module Crawler

open OpenQA.Selenium.PhantomJS
open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open System.Net
open System.Text.RegularExpressions

module Helpers =

    type Task =
        | Crawl of string
        | Click

    type Message =
        | Done
        | Mailbox of MailboxProcessor<Message>
        | Stop
        | Url of string option
        | Start of AsyncReplyChannel<unit>
        | Pause
        | CrawlerTask of Task

    // Gates the number of crawling agents.
    [<Literal>]
    let Gate = 2

    // Extracts links from HTML.
    let extractLinks html =
        let pattern1 = "(?i)href\\s*=\\s*(\"|\')/?((?!#.*|/\B|" + 
                       "mailto:|location\.|javascript:)[^\"\']+)(\"|\')"
        let pattern2 = "(?i)^https?"
 
        let links =
            [
                for x in Regex(pattern1).Matches(html) do
                    yield x.Groups.[2].Value
            ]
            |> List.filter (fun x -> Regex(pattern2).IsMatch(x))
        links
    
    // Fetches a Web page.
    let fetch (url : string) =
        try
            let req = WebRequest.Create(url) :?> HttpWebRequest
            req.UserAgent <- "Mozilla/5.0 (Windows; U; MSIE 9.0; Windows NT 9.0; en-US)"
            req.Timeout <- 5000
            use resp = req.GetResponse()
            let content = resp.ContentType
            let isHtml = Regex("html").IsMatch(content)
            match isHtml with
            | true -> use stream = resp.GetResponseStream()
                      use reader = new StreamReader(stream)
                      let html = reader.ReadToEnd()
                      Some html
            | false -> None
        with
        | _ -> None
    
    let collectLinks url =
        let html = fetch url
        match html with
        | Some x -> extractLinks x
        | None -> []

open Helpers

type Crawler(f) as this = 
    // Concurrent queue for saving collected urls.
    let q = ConcurrentQueue<string>()
    let mutable status = false
    [<DefaultValue>] val mutable repl : AsyncReplyChannel<unit>

//    // Holds crawled URLs.
//    let set = HashSet<string>()

    // Creates a mailbox that synchronizes printing to the console (so 
    // that two calls to 'printfn' do not interleave when printing)
    let printer = 
        MailboxProcessor<string>.Start(fun x -> async {
          while true do 
            let! str = x.Receive()
            stdout.WriteLine str
            //printfn "%s" str
          })
    // Hides standard 'printfn' function (formats the string using 
    // 'kprintf' and then posts the result to the printer agent.
    let printfn fmt = 
        Printf.kprintf printer.Post fmt

    let supervisor =
//        let repl =
//            match start with
//            | Start repl -> repl
//            | _ -> failwith "Expected Start message!"

        MailboxProcessor.Start(fun x -> async {
            // The agent expects to receive 'Start' message first - the message
            // carries a reply channel that is used to notify the caller
            // when the agent completes crawling.
//            let! start = x.Receive()
//            let repl =
//              match start with
//              | Start repl -> repl
//              | _ -> failwith "Expected Start message!"

            let rec loop run =
                async {
                    let! msg = x.Receive()
                    match msg with
                    | Start channel ->
                        this.repl <- channel
                        return! loop true
                    | Pause -> return! loop false

                    | Mailbox(mailbox) -> 
//                        let count = set.Count
//                        if count < limit - 1 && run then 
//                        if run then 
                        match run with
                        | false ->
                            mailbox.Post (Url None)
                            return! loop false
                        | true ->
                            let url = q.TryDequeue()
                            match url with
                            | true, str ->
                                mailbox.Post <| Url(Some str)
                                return! loop run 
//                            if not (set.Contains str) then
//                                                let set'= set.Add str
//                                                mailbox.Post <| Url(Some str)
//                                                return! loop run
//                                            else
//                                                mailbox.Post <| Url None
//                                                return! loop run

                            | _ -> mailbox.Post Pause //mailbox.Post Stop //mailbox.Post <| Url None
                                   return! loop run
//                        else
//                            mailbox.Post Stop
//                            return! loop run
//                    | Stop -> return! loop false
//                    | Start _ -> failwith "Unexpected start message!"
                    | Url _ -> failwith "Unexpected URL message!"
                    | _ ->    printfn "Supervisor is done."
                              (x :> IDisposable).Dispose()
                              // Notify the caller that the agent has completed
                              //repl.Reply(())
                              
//                              doneAsync |> Async.Start
                }
            do! loop false })

    
    let urlCollector =
        MailboxProcessor<Message>.Start(fun y ->
            let rec loop count =
                async {
                    let! msg = y.Receive()
                    match msg with
                    | Stop ->
                        (y :> IDisposable).Dispose()
                        printfn "URL collector is done."
                    | _ ->
                        match count with
                        | Gate ->
                            this.repl.Reply(())
                            supervisor.Post Pause
                            return! loop 1
    //                        supervisor.Post Done
    //                        (y :> IDisposable).Dispose()
    //                        printfn "URL collector is done."
                        | _ -> return! loop (count + 1)


//                    let! msg = y.TryReceive(6000)
//                    match msg with
//                    | Some message ->
//                        match message with
//                        | Url u ->
//                            match u with
//                            | Some url -> q.Enqueue url
//                                          return! loop count
//                            | None -> return! loop count
//                        | _ ->
//                            match count with
//                            | Gate -> supervisor.Post Done
//                                      (y :> IDisposable).Dispose()
//                                      printfn "URL collector is done."
//                            | _ -> return! loop (count + 1)
//                    | None -> supervisor.Post Stop
//                              return! loop count
                }
            loop 1)
    
    let rec waitComplete (driver:PhantomJSDriver) =
        let state = driver.ExecuteScript("return document.readyState;").ToString()
        match state with
        | "complete" -> ()
        | _ -> waitComplete driver

    /// Initializes a crawling agent.
    let crawler id =
        let browser = new PhantomJSDriver(@"C:\Users\AHMED\Desktop\phantomjs-1.9.8-windows")
        MailboxProcessor.Start(fun inbox ->
            let rec loop() =
                async {
                    let! msg = inbox.Receive()
                    match msg with
                    | CrawlerTask t ->
                        printfn "task"
                        supervisor.Post(Mailbox(inbox))
                        return! loop()

                    | Url x ->
                        match x with
                        | Some url ->
                                browser.Url <- url
                                waitComplete browser
                                printfn "%s crawled by agent %d." url id
                                let html = browser.PageSource
                                f html url
//                                let links = collectLinks url
//                                printfn "%s crawled by agent %d." url id
//                                for link in links do
//                                    urlCollector.Post <| Url (Some link)
                                supervisor.Post(Mailbox(inbox))
                                return! loop()
                        | None -> supervisor.Post(Mailbox(inbox))
                                  return! loop()
                    | Pause ->
                        urlCollector.Post Pause
                        return! loop() //urlCollector.Post Done

                    | _ -> urlCollector.Post Done
                           browser.Quit()
                           printfn "Agent %d is done." id
                           (inbox :> IDisposable).Dispose()
                    }
            loop())

    // Send 'Start' message to the main agent. The result
    // is asynchronous workflow that will complete when the
    // agent crawling completes

    // Spawn the crawlers.
    let crawlers =
//        browsers
//        |> List.mapi (fun idx x -> crawler x idx)
        [
            for x in 1 .. Gate do
                yield crawler x
        ]
    
//    crawlers.[0].Post (Url <| Some "http://news.google.com")

    // Post the first messages.
//    crawlers.Head.Post <| Url (Some urlrl)
//    crawlers.Tail |> List.iter (fun ag -> ag.Post <| Url None) 
    
    member __.Crawl(urls) =
        urls |> Seq.iter q.Enqueue
        crawlers |> List.iter (fun ag -> ag.Post <| Url None) 
        supervisor.PostAndAsyncReply(Start)

    member __.Stop() =
        crawlers |> List.iter (fun ag -> ag.Post Stop)
        urlCollector.Post Stop
        supervisor.Post Stop

    member __.Click() =
        crawlers |> List.iter (fun ag -> ag.Post (CrawlerTask Click))
        supervisor.PostAndAsyncReply(Start)

let c = Crawler(fun html url -> printfn "%s" url)

let urls =
    File.ReadAllLines (__SOURCE_DIRECTORY__ + "/TextFile1.txt")
    |> fun x -> x.[..2]

c.Crawl urls |> Async.RunSynchronously

c.Click() |> Async.RunSynchronously 

c.Stop()

