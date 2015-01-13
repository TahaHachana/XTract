#r @"..\Src\packages\Deedle.1.0.6\lib\net40\Deedle.dll"
#r @"../Src\bin\Release\XTract.dll"

//============
// Single page
//============

open XTract

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

let recordCode = CodeGen.recordCode extractors

type Pkg =
    {
        name: string
        version: string
        downloads: string
        project: string
        owners: string list
    }

let scraper = Scraper<Pkg>(extractors)

let url = urls.[0]

// Scrape a single URL
scraper.Scrape url

// Handle URL download then scrape
let html =
    Http.get url
    |> Option.get

scraper.Scrape html

// Scrape a list of URLs
let doneAsync = async {printfn "Done!"}
scraper.ThrottleScrape urls doneAsync

// Data as an array
scraper.Data

// Data as JSON
scraper.JsonData

// Data as data frame
scraper.DataFrame

// Save Excel
scraper.SaveExcel @"C:\Users\AHMED\Desktop\data.xlsx"


