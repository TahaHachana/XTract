namespace XTract

open CsvHelper
open Deedle
open Fizzler.Systems.HtmlAgilityPack
open HtmlAgilityPack
open Microsoft.FSharp.Reflection
open Newtonsoft.Json
open OpenQA.Selenium.PhantomJS
open Settings
open SpreadSharp
open SpreadSharp.Collections
open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.IO
open System.Net
open System.Text.RegularExpressions

type Selector = Css of string | Xpath of string

//type 
/// A type for describing a property to scrape.
/// Fields:
/// selector: CSS selector,
/// pattern: regex pattern to match against the selected element's inner text,
/// the default pattern is "^()(.*?)()$" and the value of the second group is retained,
/// attributes: the HTML attributes to scrape from the selected element ("text" is the default).
type Extractor = 
    {
        selector: Selector
        pattern: string
        attributes: string list
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

module CodeGen =

    let recordCode extractors =
        extractors
        |> List.mapi (fun idx x ->
            let idx' = string <| idx + 1
            let attrs = x.attributes
            match attrs with
            | [_] ->
                match x.many with
                | false -> "        Field" + idx' + ": string"
                | true -> "        Field" + idx' + ": string list"
            | _ ->
                match x.many with
                | false -> "        Field" + idx' + ": Map<string, string>"
                | true -> "        Field" + idx' + ": Map<string, string> list"
        )
        |> String.concat "\n"
        |> fun x ->
            [
                "type RecordName ="
                "    {"
                x
                "    }"
            ]
        |> String.concat "\n"

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
            |> WebUtility.HtmlDecode
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, value) 

    let falseSingleEmpty (dictionary:Dictionary<string, obj>) =
//        let value =
//            match extractor.attributes.Head with
//                | "text" ->
//                    let text =
//                        let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
//                        match ``match``.Success with
//                        | false -> ""
//                        | true -> ``match``.Groups.[2].Value
//                    text 
//                | x -> htmlNode.GetAttributeValue(x, "")
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, "") 

    // many false & multiple attrs
    let falseMany (htmlNode:HtmlNode) extractor (dictionary:Dictionary<string, obj>) =
        let map : Map<string, string> = Map.empty
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
                    |> WebUtility.HtmlDecode
                map.Add(attr, value)
            ) map
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, value)   

    let falseManyEmpty extractor (dictionary:Dictionary<string, obj>) =
        let map : Map<string, string> = Map.empty
        let value =
            extractor.attributes
            |> List.fold (fun (map:Map<string, string>) attr ->
//                let value =
//                    match attr with
//                        | "text" ->
//                            let text =
//                                let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
//                                match ``match``.Success with
//                                | false -> ""
//                                | true -> ``match``.Groups.[2].Value
//                            text 
//                        | x -> htmlNode.GetAttributeValue(x, "")
                map.Add(attr, "")
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
                |> WebUtility.HtmlDecode
            )
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, lst)

    let trueSingleEmpty (dictionary:Dictionary<string, obj>) =
//        let lst =
//            nodes
//            |> List.map (fun htmlNode ->
//                match extractor.attributes.Head with
//                    | "text" ->
//                        let text =
//                            let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
//                            match ``match``.Success with
//                            | false -> ""
//                            | true -> ``match``.Groups.[2].Value
//                        text 
//                    | x -> htmlNode.GetAttributeValue(x, "")
//            )
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, [])

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
                        |> WebUtility.HtmlDecode
                    map.Add(attr, value)
                ) map)
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, lst)

    let trueManyEmpty (dictionary:Dictionary<string, obj>) =
//        let lst =
//            nodes
//            |> List.map (fun htmlNode ->
//                let map : Map<string, string> = Map.empty
//                extractor.attributes
//                |> List.fold (fun (map:Map<string, string>) attr ->
//                    let value =
//                        match attr with
//                            | "text" ->
//                                let text =
//                                    let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
//                                    match ``match``.Success with
//                                    | false -> ""
//                                    | true -> ``match``.Groups.[2].Value
//                                text 
//                            | x -> htmlNode.GetAttributeValue(x, "")
//                    map.Add(attr, "")
//                ) map)
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, [])

    type Selection = SingleNode of HtmlNode | NodesList of HtmlNode list | SelectionFailed

    let scrape selection extractor dictionary =
        match selection with
        | SingleNode htmlNode ->
            match extractor.attributes with
            | [_] -> falseSingle htmlNode extractor dictionary
            | _ -> falseMany htmlNode extractor dictionary
        | NodesList nodes ->
            match extractor.attributes with
            | [_] -> trueSingle nodes extractor dictionary
            | _ -> trueMany nodes extractor dictionary
        | SelectionFailed ->
            match extractor.many with
            | false ->
                match extractor.attributes with
                | [_] -> falseSingleEmpty dictionary
                | _ -> falseManyEmpty extractor dictionary
            | true ->
                match extractor.attributes with
                | [_] -> trueSingleEmpty dictionary
                | _ -> trueManyEmpty dictionary
            

    let selection (root:HtmlNode) extractor =
        let selection =
            match extractor.selector with
            | Css x ->
                root.QuerySelectorAll x
                |> function
                | null -> None
                | enum -> match Seq.isEmpty enum with false -> Some enum | true -> None            
            | Xpath x ->
                root.SelectNodes x
                |> function
                | null -> None
                | enums ->
                    let enumsSeq = Seq.cast<HtmlNode> enums
                    match Seq.isEmpty enumsSeq with false -> Some enumsSeq | true -> None
//            | x -> Some x
        let groups =
            match extractor.many with
            | false -> []
            | true ->
                match selection with
                | None -> []
                | Some x ->
                    x
                    |> Seq.groupBy (fun n -> n.ParentNode)
                    |> Seq.map snd
                    |> Seq.toList
        extractor, selection, groups

    let selection' (extractor, selection, groups) idx =
        // todo: rewrite match
        match extractor.many with
        | false ->
            match selection with
            | None -> extractor, SelectionFailed
            | Some nodes ->
                try
                    extractor, SingleNode (Seq.nth idx nodes)
                with _ -> extractor, SelectionFailed
        | true ->
            match selection with
            | None -> extractor, SelectionFailed
            | _ ->
                
                let nodes =
                    try
                        groups
                        |> Seq.nth idx
                        |> Seq.toList
                    with _ -> []
//                extractor, NodesList nodes
                match nodes with
                | [] -> extractor, SelectionFailed
                | _ -> extractor, NodesList nodes

    let scrapeSingle extractors (root:HtmlNode) =
        let dictionary = Dictionary<string, obj>()
        extractors
        |> List.map (fun x -> selection root x)
//        |> List.head

//        [
//            for x in extractors -> 
//                x, root.QuerySelectorAll(x.selector)
//        ]
        |> List.map (fun x -> selection' x 0) //idx)
//        
//        (fun (extractor, selection, groups) ->
//            match extractor.many with
//            | false -> extractor, SingleNode (Seq.nth 0 selection)
//            | true ->
//                let nodes =
//                    groups
//                    |> Seq.nth 0
//                    |> Seq.toList
//                extractor, NodesList nodes
//        )
        |> List.iter (fun (extractor, selection) ->
            scrape selection extractor dictionary
        )
        dictionary
    
    let selections enums =
        let rec f acc idx =
            try
                let lst =
                    enums
                    |> List.map (fun x -> selection' x idx)
                f (acc @ [lst]) (idx + 1)
            with _ -> acc
        f [] 0

    let scrapeSelections enums =
        enums
        |> selections
        |> List.fold (fun state lst ->
            let dictionary = Dictionary<string, obj>()
            lst
            |> List.iter (fun (extractor, selection) ->
                scrape selection extractor dictionary
            )
            state @ [dictionary]
        ) []

    let scrapeAll extractors (root:HtmlNode) =
        extractors
        |> List.map (fun x -> selection root x)
        |> scrapeSelections

type Source = Html | Url of string

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

    let scrape<'T> html extractors =
        let root = htmlRoot html
        let record =
            Scrapers.scrapeSingle extractors root
            |> makeRecord<'T>
//        dataStore.Add record
        Some record

    let scrapeAll<'T> html extractors = //(dataStore:ConcurrentBag<'T>) =
        let root = htmlRoot html
        Scrapers.scrapeAll extractors root
        |> List.map (fun x ->
            let record = makeRecord<'T> x
//            dataStore.Add record
            record)
        |> Some

    let lstString (lst:'T list) =
        lst
        |> List.map (fun x -> x.ToString())
        |> String.concat "; "

    let fields (source, record) =
        let source' =
            match source with
            | Source.Html -> [|"HTML Code"|]
            | Source.Url x -> [|x|]
        FSharpValue.GetRecordFields record
        |> Array.map (fun x ->
            match x with
            | :? (string list) as lst -> lstString lst
            | :? Map<string, string> as map ->  lstString <| Map.toList map
            | :? (Map<string, string> list) as lst ->
                lst
                |> List.map Map.toList
                |> List.map lstString
                |> String.concat "\n"
            | _ -> string x
        )
        |> fun x -> Array.append x source'

type Scraper<'T when 'T : equality>(extractors) = 
    let dataStore = ConcurrentBag<Source * 'T>()
    let failedRequests = ConcurrentQueue<string>()
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
    member __.LogData = log.ToArray()

    /// Enables or disables printing log messages.
    member __.WithLogging enabled = loggingEnabled <- enabled

    /// Scrapes a single data item from the specified URL or HTML code.
    member __.Scrape input =
        match input with
        | Utils.Html ->
            logger.Post "Scraping data from HTML code"
            let record = Utils.scrape<'T> input extractors
            match record with
            | None -> None
            | Some x ->
                dataStore.Add(Html, x)
                record
        | Utils.Url ->
            logger.Post <| "Scraping data from " + input
            let html = Http.get input
            match html with
            | None ->
                failedRequests.Enqueue input
                None
            | Some html ->
                let record = Utils.scrape<'T> html extractors
                match record with
                | None -> None
                | Some x ->
                    dataStore.Add(Url input, x)
                    record

    /// Scrapes all the data items from the specified URL or HTML code.
    member __.ScrapeAll input = 
        match input with
        | Utils.Html ->
            logger.Post <| "Scraping data from HTML code"                
            let records = Utils.scrapeAll<'T> input extractors
            match records with
            | None -> None
            | Some lst ->
                lst |> List.iter (fun x -> dataStore.Add(Html, x))
                records
        | Utils.Url ->
            logger.Post <| "Scraping data from " + input
            let html = Http.get input
            match html with
            | None ->
                failedRequests.Enqueue input
                None
            | Some html ->
                let records = Utils.scrapeAll<'T> html extractors
                match records with
                | None -> None
                | Some lst ->
                    lst |> List.iter (fun x -> dataStore.Add(Url input, x))
                    records

    /// Throttles scraping a single data item from the specified URLs
    /// by sending 5 concurrent requests and executes the async computation
    /// once done.
    member __.ThrottleScrape urls doneAsync =
        let asyncs =
            urls
            |> Seq.map (fun x ->
                async {
                    let! html = Http.getAsync x
                    match html with
                    | None ->
                        failedRequests.Enqueue x
                    | Some html ->
                        let record = Utils.scrape<'T> html extractors
                        match record with
                        | None -> ()
                        | Some r ->
                            dataStore.Add(Url x, r)
                }                
            )
        Throttler.throttle asyncs 5 doneAsync

    /// Throttles scraping all the data items from the specified URLs
    /// by sending 5 concurrent requests and executes the async computation
    /// once done.
    member __.ThrottleScrapeAll urls doneAsync = 
        let asyncs =
            urls
            |> Seq.map (fun x ->
                async {
                    let! html = Http.getAsync x
                    match html with
                    | None ->
                        failedRequests.Enqueue x
                    | Some html ->
                        let records = Utils.scrapeAll<'T> html extractors
                        match records with
                        | None -> ()
                        | Some lst ->
                            lst |> List.iter (fun r -> dataStore.Add(Url x, r))
                }                
            )
        Throttler.throttle asyncs 5 doneAsync

    /// Returns the data stored so far by the scraper.
    member __.Data =
        dataStore
        |> Seq.distinctBy snd
        |> Seq.toArray

    /// Returns the urls that scraper failed to download.
    member __.FailedRequests = failedRequests.ToArray()

    /// Stores a failed HTTP request, use this method when
    /// handling HTTP requests by yourself and you want to
    /// track errors.
    member __.StoreFailedRequest url = failedRequests.Enqueue url

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
            |> fun x -> Array.append x [|"Source"|]
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
        let columnsCount = Array.length headers + 1
        let rangeString = String.concat "" ["A1:"; string (char (columnsCount + 64)) + "1"]
        let rng = XlRange.get worksheet rangeString
        Array.toRange rng (Array.append headers [|"Source"|])
        dataStore
        |> Seq.iteri (fun idx x ->
            let idxString = string <| idx + 2
            let rangeString = String.concat "" ["A"; idxString; ":"; string (char (columnsCount + 64)); idxString]
            let rng = XlRange.get worksheet rangeString
            Array.toRange rng (Utils.fields x)
        )
        XlWorkbook.saveAs workbook path
        XlApp.quit xl

type DynamicScraper<'T when 'T : equality>(extractors) =
//    let driver = new ChromeDriver(XTractSettings.chromeDriverDirectory)
    let dataStore = ConcurrentBag<Source * 'T>()
    let failedRequests = ConcurrentQueue<string>()
    let log = ConcurrentQueue<string>()
    let mutable loggingEnabled = true
    let f html url =
        let record = Utils.scrape<'T> html extractors
        match record with
        | None -> ()
        | Some x -> dataStore.Add(Url url, x)
    
    let crawler = Crawler.Crawler f

//    let rec waitComplete() =
//        let state = driver.ExecuteScript("return document.readyState;").ToString()
//        match state with
//        | "complete" -> ()
//        | _ -> waitComplete()

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
//    member __.Get url =
//        driver.Url <- url
//        waitComplete()
//
//    /// Selects an element and types the specified text into it.
//    member __.CssSendKeys cssSelector text =
//        let elem = driver.FindElementByCssSelector cssSelector
//        printfn "%A" elem.Displayed
//        elem.SendKeys(text)
//
//    /// Selects an element and types the specified text into it.
//    member __.XpathSendKeys xpath text =
//        let elem = driver.FindElementByXPath xpath
//        printfn "%A" elem.Displayed
//        elem.SendKeys(text)
//
//    /// Selects an element and clicks it.
//    member __.CssClick cssSelector =
//        driver.FindElementByCssSelector(cssSelector).Click()
//        waitComplete()
//
//    /// Selects an element and clicks it.
//    member __.XpathClick xpath =
//        driver.FindElementByXPath(xpath).Click()
//        waitComplete()
//
//    /// Closes the browser drived by the scraper.
//    member __.Quit() = driver.Quit()

    /// Posts a new log message to the scraper's logging agent.
    member __.Log msg = logger.Post msg

    /// Returns the scraper's log.
    member __.LogData = log.ToArray()

    /// Enables or disables printing log messages.
    member __.WithLogging enabled = loggingEnabled <- enabled

    /// Scrapes a single data item from the specified URL or HTML code.
    member __.Scrape(urls) = crawler.Crawl urls
//        let f html url =
//            let record = Utils.scrape<'T> html extractors
//            match record with
//            | None -> ()
//            | Some x -> dataStore.Add(Url url, x)
//        Crawler.crawl urls browsers f doneAsync
//        let url = driver.Url
//        logger.Post <| "Scraping data from " + url
//        let html = driver.PageSource
//        let record = Utils.scrape<'T> html extractors
//        match record with
//        | None -> None
//        | Some x ->
//            dataStore.Add(Url url, x)
//            record

    /// Scrapes a single data item from the specified URL or HTML code.
//    member __.Scrape() =
//        let url = driver.Url
//        logger.Post <| "Scraping data from " + url
//        let html = driver.PageSource
//        let record = Utils.scrape<'T> html extractors
//        match record with
//        | None -> None
//        | Some x ->
//            dataStore.Add(Url url, x)
//            record

    /// Scrapes all the data items from the specified URL or HTML code.    
//    member __.ScrapeAll() =
//        let url = driver.Url
//        logger.Post <| "Scraping data from " + url
//        let html = driver.PageSource
//        let records = Utils.scrapeAll<'T> html extractors
//        match records with
//        | None -> None
//        | Some lst ->
//            lst |> List.iter (fun x -> dataStore.Add(Url url, x))
//            records

    /// Returns the data stored so far by the scraper.
    member __.Data =
        dataStore
        |> Seq.distinctBy snd
        |> Seq.toArray

    /// Returns the urls that scraper failed to download.
    member __.FailedRequests = failedRequests.ToArray()

    /// Stores a failed HTTP request, use this method when
    /// handling HTTP requests by yourself and you want to
    /// track errors.
    member __.StoreFailedRequest url = failedRequests.Enqueue url

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
            |> fun x -> Array.append x [|"Source"|]
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
        let columnsCount = Array.length headers + 1
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
