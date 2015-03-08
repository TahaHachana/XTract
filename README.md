XTract
======

About
-----

F# screen scraping package.

NuGet
-----

	PM> Install-Package XTract

Usage
-----

```fsharp
#load @"../packages/XTract.0.4.0/XTractBootstrap.fsx"

open XTract

// Define the extractors using CSS selectors
// and/or XPath expressions.
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
// an F# record type.
let dataModel = CodeGen.recordCode extractors

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
let websharper = scraper.Scrape url

// Handle HTTP requests yourself then scrape the HTML code
let html =
    Http.get url
    |> Option.get

let websharper' = scraper.ScrapeHtml html url

// Add a pipeline if you want to further process
// the scraped data or may be store it in a database
scraper.WithPipeline (fun pkg ->
    scraper.Log <| sprintf "Storing %s in database" pkg.name
    pkg)

// Throttle scraping multiple urls
scraper.ThrottleScrape urls
|> Async.RunSynchronously

// Data as an array
scraper.Data

// Data as JSON
scraper.JsonData

// Save as an Excel workbook
open System
open System.IO

let desktop = Environment.GetFolderPath Environment.SpecialFolder.Desktop
let path = Path.Combine(desktop, "NuGet.xlsx")
scraper.SaveExcel path

// Failed HTTP requests
scraper.FailedRequests

// Log
scraper.LogData

// Save as CSV
let path' = Path.Combine(desktop, "NuGet.csv")
scraper.SaveCsv path'
```

Contact
-------

* Email: tahahachana@gmail.com
* [@TahaHachana](https://twitter.com/TahaHachana "Twitter")
* [Blog](http://fsharp-code.blogspot.com/)
* [+Taha](https://plus.google.com/103826666258148033768/ "Google+")