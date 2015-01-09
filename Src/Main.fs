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
        many: bool
    }
    
    static member New selector = 
        {
            selector = selector
            pattern = "^()(.*?)()$"
            attributes = [ "text" ]
            many = false
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

    static member WithMany many property =
        {
            property with
                many = many
        }

module Scrapers =

    // many false & single attr
    let falseSingle (htmlNode:HtmlNode) extractor (dictionary:Dictionary<string, obj>) =
        let value =
            match extractor.attributes.Head with
                | "text" ->
                    let text =
                        let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
                        match ``match``.Success with
                        | false -> ""
                        | true -> ``match``.Groups.[2].Value
                    text 
                | x -> htmlNode.GetAttributeValue(x, "")
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, value) 

    // many false & multiple attrs
    let falseMany (htmlNode:HtmlNode) extractor (dictionary:Dictionary<string, obj>) =
        let map : Map<string, string> = Map.empty //  Dictionary<string, string>()
        let value =
            extractor.attributes
            |> List.fold (fun (map:Map<string, string>) attr ->
                let value =
                    match attr with
                        | "text" ->
                            let text =
                                let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
                                match ``match``.Success with
                                | false -> ""
                                | true -> ``match``.Groups.[2].Value
                            text 
                        | x -> htmlNode.GetAttributeValue(x, "")
                map.Add(attr, value)
            ) map
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, value)   

    // many true & single attr
    let trueSingle (nodes:HtmlNode list) extractor (dictionary:Dictionary<string, obj>) =
        let lst =
            nodes
            |> List.map (fun htmlNode ->
                match extractor.attributes.Head with
                    | "text" ->
                        let text =
                            let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
                            match ``match``.Success with
                            | false -> ""
                            | true -> ``match``.Groups.[2].Value
                        text 
                    | x -> htmlNode.GetAttributeValue(x, "")
            )
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, lst)

    // many true & multiple attrs
    let trueMany (nodes:HtmlNode list) extractor (dictionary:Dictionary<string, obj>) =
        let lst =
            nodes
            |> List.map (fun htmlNode ->
                let map : Map<string, string> = Map.empty
                extractor.attributes
                |> List.fold (fun (map:Map<string, string>) attr ->
                    let value =
                        match attr with
                            | "text" ->
                                let text =
                                    let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
                                    match ``match``.Success with
                                    | false -> ""
                                    | true -> ``match``.Groups.[2].Value
                                text 
                            | x -> htmlNode.GetAttributeValue(x, "")
                    map.Add(attr, value)
                ) map)
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, lst)

    type Selection = SingleNode of HtmlNode | NodesList of HtmlNode list

    let scrapeSingle extractors (root:HtmlNode) =
        let dictionary = Dictionary<string, obj>()

        let enums = 
            [
                for x in extractors -> 
                    let selection = root.QuerySelectorAll(x.selector)
                    x, selection
            ]
        enums
        |> List.map (fun (extractor, selection) ->
            match extractor.many with
            | false -> extractor, SingleNode (Seq.nth 0 selection)
            | true ->
                let nodes =
                    selection
                    |> Seq.groupBy (fun n -> n.ParentNode)
                    |> Seq.nth 0
                    |> snd
                    |> Seq.toList
                extractor, NodesList nodes
        )
        |> List.iter (fun (extractor, selection) ->
            match selection with
            | SingleNode htmlNode ->
                match extractor.attributes with
                | [_] -> falseSingle htmlNode extractor dictionary
                | _ -> falseMany htmlNode extractor dictionary
            | NodesList nodes ->
                match extractor.attributes with
                | [_] -> trueSingle nodes extractor dictionary
                | _ -> trueMany nodes extractor dictionary
        )
        dictionary

    let scrape extractors (root:HtmlNode) =
        let enums = 
            [
                for x in extractors -> 
                    let selection = root.QuerySelectorAll(x.selector)
                    let groups =
                        match x.many with
                        | false -> []
                        | true ->
                            selection
                            |> Seq.groupBy (fun n -> n.ParentNode)
                            |> Seq.map snd
                            |> Seq.toList
                    x, selection, groups
            ]
        let rec loop acc idx =
            let dictionary = Dictionary<string, obj>()
            try
                enums
                |> List.map (fun (extractor, selection, groups) ->
                    match extractor.many with
                    | false -> extractor, SingleNode (Seq.nth idx selection)
                    | true ->
                        let nodes =
                            groups
                            |> Seq.nth idx
                            |> Seq.toList
                        extractor, NodesList nodes
                )
                |> List.iter (fun (extractor, selection) ->
                    match selection with
                    | SingleNode htmlNode ->
                        match extractor.attributes with
                        | [_] -> falseSingle htmlNode extractor dictionary
                        | _ -> falseMany htmlNode extractor dictionary
                    | NodesList nodes ->
                        match extractor.attributes with
                        | [_] -> trueSingle nodes extractor dictionary
                        | _ -> trueMany nodes extractor dictionary
                )
                loop (acc @ [dictionary]) (idx + 1)
            with _ -> acc
        loop [] 0

module Utils = 

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

    let makeRecord<'T> (dictionary : Dictionary<string, obj>) =
        let values =
            dictionary.Values
            |> Seq.toArray
        FSharpValue.MakeRecord(typeof<'T>, values) :?> 'T

    let scrape<'T> html extractors (dataStore:ConcurrentBag<'T>) =
        let root = htmlRoot html
        let record =
            Scrapers.scrapeSingle extractors root
            |> makeRecord<'T>
        dataStore.Add record
        Some record

    let scrapeAll<'T> html extractors (dataStore:ConcurrentBag<'T>) =
        let root = htmlRoot html
        Scrapers.scrape extractors root
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
