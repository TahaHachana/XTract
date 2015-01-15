#load @"../Src/packages/XTract.0.3.5/XTractBootstrap.fsx"

open XTract

// Data extractors
let name =
    Css "div > div > div > div > h1"
    |> Extractor.New

let version =
    Css "div > div > div > div > h2"
    |> Extractor.New

let downloads =
    Css "div > div > div > div:nth-child(1).stat > p:nth-child(1).stat-number"
    |> Extractor.New

let project =
    Xpath """//*[@id="sideColumn"]/nav/ul/li[1]/a"""
    |> Extractor.New
    |> Extractor.WithAttributes ["href"]

let owners =
    Css ".owner-name"
    |> Extractor.New
    |> Extractor.WithMany true ThirdParent

let extractors = [name; version; downloads; project; owners]

// Get some help when mapping the extractors to
// an F# record type
let recordCode = CodeGen.recordCode extractors

// Data model
type Pkg =
    {
        name: string
        version: string
        downloads: string
        project: string
        owners: string list
        url: string
    }

// Initiliaze a scraper
let scraper = Scraper<Pkg>(extractors)

// Target urls
let urls =
    [
        "http://www.nuget.org/packages/WebSharper/"
        "http://www.nuget.org/packages/FSharp.Data.Toolbox.Twitter/"
        "http://www.nuget.org/packages/R.NET.Community.FSharp/"
        "http://www.nuget.org/packages/FSharp.Data/"
        "http://www.nuget.org/packages/FsLab/"
        "http://www.nuget.org/packages/XPlot.GoogleCharts/"
        "http://www.nuget.org/packages/XTract/"
        "http://www.nuget.org/packages/PerfUtil/"
        "http://www.nuget.org/packages/Deedle/"
        "http://www.nuget.org/packages/Paket/"
    ]

let url = urls.[0]

// Scrape a single URL
scraper.Scrape url

// Handle HTTP requests yourself then scrape the HTML code
let html =
    Http.get url
    |> Option.get

scraper.ScrapeHtml html url

// Add a pipeline if you want to further process
// the scraped data or may be store it in a database
scraper.WithPipeline (fun pkg ->
    scraper.Log <| sprintf "Storing %s in database" pkg.name)

// Scrape multiple urls
let doneAsync = async {printfn "Done!"}
scraper.ThrottleScrape urls doneAsync

// Data as an array
scraper.Data

// Data as JSON
scraper.JsonData

// Data as data frame
scraper.DataFrame

// Save as an Excel workbook
open System
open System.IO
open XTract.Helpers

let desktop = Environment.GetFolderPath Environment.SpecialFolder.Desktop
let path = Path.Combine(desktop, "Data.xlsx")
scraper.SaveExcel path

// Failed HTTP requests
scraper.FailedRequests

// Log
scraper.LogData

// Save as CSV
let path' = Path.Combine(desktop, "Data.csv")
scraper.SaveCsv path'


//==================================
// Many items per page

let pkgName =
    Css "li > section > div > h1 > a"
    |> Extractor.New
    |> Extractor.WithAttributes ["text"; "href"]

let pkgDescription =
    Css "ul > li > section > div > p"
    |> Extractor.New
    |> Extractor.WithPattern "(?s)^()(.+?)()$"

let extractors' = [pkgName; pkgDescription]

let code = CodeGen.recordCode  extractors'

type PkgListing =
    {
        Details: Map<string, string>
        Description: string
        ItemUrl: string
    }

let scraper' = Scraper<PkgListing>(extractors')

let url' = "http://www.nuget.org/profiles/Taha"

scraper'.ScrapeAll url

let html' =
    Http.get url'
    |> Option.get

scraper'.ScrapeAllHtml html' url'

open XTract.Helpers
open XTract.Helpers.Scrapers
open XTract.Html

Utils.scrapeAll<PkgListing> html' extractors' url'


let root = XTract.Helpers.Utils.htmlRoot html'
Scrapers.scrapeAll extractors' root


let selections enums =
    let rec f acc idx =
//        try
            let lst =
                enums
                |> List.map (fun x -> selection' x idx)
            let test = lst |> List.forall (fun (_, x) -> x = SelectionFailed)
            match test with
            | false -> f (acc @ [lst]) (idx + 1)
            | true -> acc
//        with _ -> acc
    f [] 0

open System.Collections.Generic

extractors'
|> List.map (fun x -> selection root x)
|> selections
|> List.fold (fun state lst ->
    let dictionary = Dictionary<string, obj>()
    lst
    |> List.iter (fun (extractor, selection) ->
        scrape selection extractor dictionary
    )
    state @ [dictionary]
) []

