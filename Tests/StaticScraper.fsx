#load @"../Src/packages/XTract.0.3.6/XTractBootstrap.fsx"

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

scraper'.ScrapeAll url'

let html' =
    Http.get url'
    |> Option.get

scraper'.ScrapeAllHtml html' url'

scraper'.SaveExcel path


let n =
    Css "div:nth-child(5).anchor > div:nth-child(2) > div.row.data-row > div.col-md-5 > div:nth-child(1).media > div:nth-child(2).media-body > h4:nth-child(1).media-heading > a:nth-child(1)"
    |> Extractor.New

let dlds =
    Css "div:nth-child(5).anchor > div:nth-child(2) > div.row.data-row > div.col-md-5 > div:nth-child(1).media > div:nth-child(2).media-body > p:nth-child(3) > span:nth-child(1).badge"
    |> Extractor.New

let tags =
    Css "div:nth-child(5).anchor > div:nth-child(2) > div.row.data-row > div.col-md-5 > div:nth-child(1).media > div:nth-child(2).media-body > div:nth-child(4) > span.label.label-info"
    |> Extractor.New
    |> Extractor.WithMany true FirstParent
    |> Extractor.WithAttributes ["text"; "class"]

let extractors'' = [n; dlds; tags]

let code' = CodeGen.recordCode extractors''

type FSHPkg =
    {
        Name: string
        Downloads: string
        Tags: Map<string, string> list
        ItemUrl: string
    }

let scraper'' = Scraper<FSHPkg> extractors''

scraper''.ScrapeAll "http://fsharp-hub.apphb.com/"

let html'' = Http.get "http://fsharp-hub.apphb.com/" |> Option.get

scraper''.ScrapeAllHtml html'' "http://fsharp-hub.apphb.com/"

scraper''.SaveExcel path
