#load @"..\Src\packages\XTract.0.3.0\XTractBootstrap.fsx"

//============
// Single page
//============

open XTract
open XTract.Extraction

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
    Css ".owners > li"
    |> Extractor.New
    |> Extractor.WithMany true

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

scraper.Scrape urls.[4]

