[<AutoOpen>]
module XTract.DynamicScraper

open System.Collections.Concurrent
open System.Collections.Generic
open System
open Newtonsoft.Json
open Deedle
open Microsoft.FSharp.Reflection
open System.IO
open CsvHelper
open SpreadSharp
open SpreadSharp.Collections
open Extraction
open Helpers
open Crawler

type DynamicScraper<'T when 'T : equality>(extractors, ?Browser, ?Gate) =
    let browser = defaultArg Browser Phantom
    let gate = defaultArg Gate 5
    let dataStore = HashSet<'T>()
//    let failedRequests = ConcurrentQueue<string>()
    let log = ConcurrentQueue<string>()
    let mutable loggingEnabled = true
    let mutable pipelineFunc = fun (record:'T) -> ()

    let pipeline =
        MailboxProcessor.Start(fun inbox ->
            let rec loop() =
                async {
                    let! msg = inbox.Receive()
                    dataStore.Add msg
                    |> function
                    | false -> ()
                    | true -> pipelineFunc msg
                    return! loop()
                }
            loop()
        )
    
    let crawler = DynamicCrawler(browser, gate)

    let logger =
        MailboxProcessor.Start(fun inbox ->
            let rec loop() =
                async {
                    let! msg = inbox.Receive()
                    let time = DateTime.Now.ToString()
                    let msg' = time + " - " + msg
                    log.Enqueue msg'
                    match loggingEnabled with
                    | false -> ()
                    | true -> printfn "%s" msg'
                    return! loop()
                }
            loop()
        )

    let scrape html url =
        logger.Post <| "[Info] Scraping " + url
        let record = Utils.scrape<'T> html extractors url
        match record with
        | None -> ()
        | Some x -> pipeline.Post x

    let scrapeAll html url =
        logger.Post <| "Scraping " + url
        let records = Utils.scrapeAll<'T> html extractors url
        match records with
        | None -> ()
        | Some lst -> lst |> List.iter pipeline.Post

    member __.WithPipeline f = pipelineFunc <- f
    
    /// Loads the specified URL.
    member __.Get url = crawler.Get url

    /// Selects an element and types the specified text into it.
    member __.CssSendKeys cssSelector text = crawler.CssSendKeys cssSelector text

    /// Selects an element and types the specified text into it.
    member __.XpathSendKeys xpath text = crawler.XpathSendKeys xpath text

    /// Selects an element and clicks it.
    member __.CssClick cssSelector = crawler.CssClick cssSelector

    /// Selects an element and clicks it.
    member __.XpathClick xpath = crawler.XpathClick xpath

    /// Closes the browsers drived by the scraper.
    member __.Quit() = crawler.Quit()

    /// Posts a new log message to the scraper's logging agent.
    member __.Log msg = logger.Post msg

    /// Returns the scraper's log.
    member __.LogData = log.ToArray()

    /// Enables or disables printing log messages.
    member __.WithLogging enabled = loggingEnabled <- enabled

    /// Scrapes a single data item from the specified URL or HTML code.
    member __.Scrape(urls) = crawler.Crawl(scrape, urls)

    member __.ScrapeAll(urls) = crawler.Crawl(scrapeAll, urls)

    /// Scrapes a single data item from the specified URL or HTML code.
    member __.Scrape() = crawler.Scrape scrape

    member __.ScrapeAll() = crawler.Scrape scrapeAll

    /// Returns the data stored so far by the scraper.
    member __.Data =
        dataStore
        |> Seq.toArray

//    /// Returns the urls that scraper failed to download.
//    member __.FailedRequests = failedRequests.ToArray()
//
//    /// Stores a failed HTTP request, use this method when
//    /// handling HTTP requests by yourself and you want to
//    /// track errors.
//    member __.StoreFailedRequest url = failedRequests.Enqueue url

    /// Returns the data stored so far by the scraper in JSON format.
    member __.JsonData =
        JsonConvert.SerializeObject(dataStore, Formatting.Indented)

    /// Returns the data stored so far by the scraper as a Deedle data frame.
    member __.DataFrame = Frame.ofRecords dataStore

    /// Saves the data stored by the scraper in a CSV file.
    member __.SaveCsv(path) =
        let headers =
            FSharpType.GetRecordFields typeof<'T>
            |> Array.map (fun x -> x.Name)
//            |> fun x -> Array.append x [|"Source"|]
        let sw = File.CreateText(path)
        let csv = new CsvWriter(sw)
        headers
        |> Array.iter (fun x -> csv.WriteField x)
        csv.NextRecord()
        dataStore
        |> Seq.iter (fun x ->
            Utils.fields x
            |> Array.iter (fun x -> csv.WriteField x)
            csv.NextRecord()            
        )
        sw.Flush()
        sw.Dispose()

    /// Saves the data stored by the scraper in an Excel file.
    member __.SaveExcel(path) =
        let xl = XlApp.startHidden()
        let workbook = XlWorkbook.add xl
        let worksheet = XlWorksheet.byIndex workbook 1
        let headers =
            FSharpType.GetRecordFields typeof<'T>
            |> Array.map (fun x -> x.Name)
        let columnsCount = Array.length headers
        let rangeString = String.concat "" ["A1:"; string (char (columnsCount + 64)) + "1"]
        let rng = XlRange.get worksheet rangeString
        Array.toRange rng headers
        dataStore
        |> Seq.iteri (fun idx x ->
            let idxString = string <| idx + 2
            let rangeString = String.concat "" ["A"; idxString; ":"; string (char (columnsCount + 64)); idxString]
            let rng = XlRange.get worksheet rangeString
            Array.toRange rng (Utils.fields x)
        )
        XlWorkbook.saveAs workbook path
        XlApp.quit xl