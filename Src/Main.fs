namespace XTract

open CsvHelper
open Fizzler.Systems.HtmlAgilityPack
open HtmlAgilityPack
open Microsoft.FSharp.Reflection
open Newtonsoft.Json
open OpenQA.Selenium.Chrome
open Settings
open SpreadSharp
open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions

/// A type for describing a property to scrape.
/// Fields:
/// selector: CSS selector,
/// pattern: regex pattern to match against the selected element's inner text,
/// the default pattern is "^()(.*?)()$" and the value of the second group is retained,
/// attributes: the HTML attributes to scrape from the selected element ("text" is the default).
type Extractor = 
    {
        selector : string
        pattern : string
        attributes : string list
    }
    
    static member New selector = 
        {
            selector = selector
            pattern = "^()(.*?)()$"
            attributes = [ "text" ]
        }
    
    static member WithPattern pattern property =
        {
            property with
                pattern = pattern
        }

    static member WithAttributes attributes property =
        {
            property with
                attributes = attributes
        }

module Utils = 
    let emptyAttrs attrs (dictionary : Dictionary<string, string>) = 
        attrs
        |> List.iter (fun x ->
            let key = string <| dictionary.Count + 1
            match x with
            | "text" -> dictionary.Add(key + "-text", "")
            | _ -> dictionary.Add(key + "-" + x, "")
        )
    
    let scrapeAttrs attrs extractor (htmlNode : HtmlNode) (dictionary : Dictionary<string, string>) = 
        attrs
        |> List.iter (fun x -> 
            let key = dictionary.Count + 1 |> string
            match x with
            | "text" -> 
                let text = 
                    let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
                    match ``match``.Success with
                    | false -> ""
                    | true -> ``match``.Groups.[2].Value
                dictionary.Add(key + "-text", text)
            | _ -> dictionary.Add(key + "-" + x, htmlNode.GetAttributeValue(x, "")))
    
    let extract extractor htmlNode dictionary = 
        let attrs = extractor.attributes
        match htmlNode with
        | None -> emptyAttrs attrs dictionary
        | Some htmlNode -> scrapeAttrs attrs extractor htmlNode dictionary
    
    let rec extractAll (acc : Dictionary<string, string> list) (enums : (Extractor * IEnumerator<HtmlNode> option) list) = 
        let enums' = 
            enums
            |> List.map (fun (extrator, enumerator) -> 
                match enumerator with
                | None -> extrator, enumerator, None
                | Some x -> 
                    let htmlNode = 
                        match x.MoveNext() with
                        | false -> None
                        | true -> Some x.Current
                    extrator, enumerator, htmlNode)
        enums'
        |> List.exists (fun (_, _, htmlNode) -> htmlNode.IsSome)
        |> function
        | false -> acc
        | true -> 
            let acc' = 
                [ let dictionary = Dictionary<string, string>()
                  for (extractor, _, htmlNode) in enums' do
                      extract extractor htmlNode dictionary
                  yield dictionary ]
            enums'
            |> List.map (fun (extractor, enumerator, _) -> extractor, enumerator)
            |> extractAll (acc @ acc')

    let urlRegex =
        let pattern = "^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?"
        Regex(pattern)

    let (|Html|Url|) input =
        match urlRegex.IsMatch(input) with
        | false -> Html
        | true -> Url

    let htmlRoot (html : string) =
        let document = HtmlDocument()
        document.LoadHtml html
        document.DocumentNode

    let makeRecord<'T> (dictionary : Dictionary<string, string>) =
        let values =
            dictionary.Values
            |> Seq.toArray
            |> Array.map box
        FSharpValue.MakeRecord(typeof<'T>, values) :?> 'T

    let scrape<'T> html extractors (dataStore:ConcurrentBag<'T>) =
        let root = htmlRoot html
        let dictionary = Dictionary<string, string>()
        extractors
        |> List.map (fun x -> 
                let selection = root.QuerySelector(x.selector)
                match selection with
                | null -> x, None
                | _ -> x, Some selection)
        |> List.iter (fun (x, htmlNode) -> extract x htmlNode dictionary)
        let record = makeRecord<'T> dictionary
        dataStore.Add record
        Some record
    
    let scrapeAll<'T> html extractors (dataStore:ConcurrentBag<'T>) =
        let root = htmlRoot html
        let enums = 
            [
                for x in extractors -> 
                    let selection = root.QuerySelectorAll(x.selector)
                    match selection with
                    | null -> x, None
                    | _ -> x, Some <| selection.GetEnumerator()
            ]
        extractAll [] enums
        |> List.map (fun x -> 
            let record = makeRecord<'T> x
            dataStore.Add record
            record)
        |> Some

type Scraper<'T>(extractors) = 
    let dataStore = ConcurrentBag<'T>()
    let log = ConcurrentQueue<string>()
    let mutable loggingEnabled = true

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
    
    /// Posts a new log message to the scraper's logging agent.
    member __.Log msg = logger.Post msg

    /// Returns the scraper's log.
    member __.LogData() = log.ToArray()

    /// Enables or disables printing log messages.
    member __.WithLogging enabled = loggingEnabled <- enabled

    /// Scrapes a single data item from the specified URL or HTML code.
    member __.Scrape input =
        match input with
        | Utils.Html ->
            let substring =
                match input.Length with
                | l when l < 50 -> input + "..."
                | _ -> input.Substring(0, 50) + "..."
            logger.Post <| "Scraping data from " + substring
            Utils.scrape<'T> input extractors dataStore
        | Utils.Url ->
            logger.Post <| "Downloading " + input
            let html = Http.get input
            match html with
            | None ->
                logger.Post <| "Failed to download " + input
                None
            | Some html ->
                logger.Post <| "Scraping data from " + input
                Utils.scrape<'T> html extractors dataStore

    /// Scrapes all the data items from the specified URL or HTML code.
    member __.ScrapeAll input = 
        match input with
        | Utils.Html ->
            let substring =
                match input.Length with
                | l when l < 50 -> input + "..."
                | _ -> input.Substring(0, 50) + "..."
            logger.Post <| "Scraping data from " + substring                
            Utils.scrapeAll<'T> input extractors dataStore
        | Utils.Url ->
            let html = Http.get input
            match html with
            | None ->
                logger.Post <| "Failed to download " + input
                None
            | Some html ->
                logger.Post <| "Scraping data from " + input                    
                Utils.scrapeAll<'T> html extractors dataStore

    /// Returns the data stored so far by the scraper.
    member __.Data() = dataStore.ToArray()

    /// Returns the data stored so far by the scraper in JSON format.
    member __.JsonData() =
        dataStore.ToArray()
        |> fun x -> JsonConvert.SerializeObject(x, Formatting.Indented)

    /// Saves the data stored by the scraper in a CSV file.
    member __.SaveCsv(path) =
        let sw = File.CreateText(path)
        let csv = new CsvWriter(sw)
        csv.WriteRecords(dataStore)
        sw.Flush()
        sw.Dispose()

    /// Saves the data stored by the scraper in an Excel file.
    member __.SaveExcel(path) =
        Records.saveAs dataStore typeof<'T> path

type DynamicScraper<'T>(extractors) =
    let driver = new ChromeDriver(XTractSettings.chromeDriverDirectory)
    let dataStore = ConcurrentBag<'T>()
    let log = ConcurrentQueue<string>()
    let mutable loggingEnabled = true

    let rec waitComplete() =
        let state = driver.ExecuteScript("return document.readyState;").ToString()
        match state with
        | "complete" -> ()
        | _ -> waitComplete()

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
    
    /// Loads the specified URL.
    member __.Get url =
        driver.Url <- url
        waitComplete()

    /// Selects an element and types the specified text into it.
    member __.SendKeys cssSelector text =
        let elem = driver.FindElementByCssSelector(cssSelector)
        printfn "%A" elem.Displayed
        elem.SendKeys(text)

    /// Selects an element and clicks it.
    member __.Click cssSelector =
        driver.FindElementByCssSelector(cssSelector).Click()
        waitComplete()

    /// Closes the browser drived by the scraper.
    member __.Quit() = driver.Quit()

    /// Posts a new log message to the scraper's logging agent.
    member __.Log msg = logger.Post msg

    /// Returns the scraper's log.
    member __.LogData() = log.ToArray()

    /// Enables or disables printing log messages.
    member __.WithLogging enabled = loggingEnabled <- enabled

    /// Scrapes a single data item from the specified URL or HTML code.
    member __.Scrape() =
        logger.Post <| "Scraping data from " + driver.Url
        let html = driver.PageSource
        Utils.scrape<'T> html extractors dataStore        

    /// Scrapes all the data items from the specified URL or HTML code.    
    member __.ScrapeAll() =
        logger.Post <| "Scraping data from " + driver.Url
        let html = driver.PageSource
        Utils.scrapeAll<'T> html extractors dataStore

    /// Returns the data stored so far by the scraper.
    member __.Data() = dataStore.ToArray()

    /// Returns the data stored so far by the scraper in JSON format.
    member __.JsonData() =
        dataStore.ToArray()
        |> fun x -> JsonConvert.SerializeObject(x, Formatting.Indented)

    /// Saves the data stored by the scraper in a CSV file.
    member __.SaveCsv(path) =
        let sw = File.CreateText(path)
        let csv = new CsvWriter(sw)
        csv.WriteRecords(dataStore)
        sw.Flush()
        sw.Dispose()

    /// Saves the data stored by the scraper in an Excel file.
    member __.SaveExcel(path) =
        Records.saveAs dataStore typeof<'T> path
