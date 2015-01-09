#I @"bin\Release\"
#r "XTract.dll"

open System
open System.IO
open XTract

let name =
    "div:nth-child(5).anchor > div:nth-child(2) > div.row.data-row > div.col-md-5 > div:nth-child(1).media > div:nth-child(2).media-body > h4:nth-child(1).media-heading > a:nth-child(1)"
    |> Extractor.New
    |> Extractor.WithAttributes ["text"] //; "href"]

let tags =
    "div:nth-child(5).anchor > div:nth-child(2) > div.row.data-row > div.col-md-5 > div:nth-child(1).media > div:nth-child(2).media-body > div:nth-child(4) > span.label.label-info"
    |> Extractor.New
    |> Extractor.WithMany true
    |> Extractor.WithAttributes ["text"]

let extractors = [name; tags]
let code = CodeGen.recordCode extractors

type Pkg =
    {
        details: string
        tags: string list
    }

let scraper = Scraper<Pkg>([name; tags])
let test = scraper.ScrapeAll "http://fsharp-hub.apphb.com/"
let test1 = scraper.Scrape "http://fsharp-hub.apphb.com/"

//let html =
//    Http.get "http://fsharp-hub.apphb.com/"
//    |> Option.get
//
//#r @"C:\Users\AHMED\Documents\GitHub\XTract\Src\packages\Newtonsoft.Json.6.0.7\lib\net40\Newtonsoft.Json.dll"
//
//Newtonsoft.Json.JsonConvert.DeserializeObject(test, typeof<Pkg>)
//
//open System.Collections.Generic
//
//let d = Dictionary<string, obj>()
//
//d.Add("1", "Streams.CSharp 0.2.8")
//d.Add("2", ["F#/C#";"Streams"])
//open Microsoft.FSharp.Reflection
//
//let record = FSharpValue.MakeRecord(typeof<Pkg>, d.Values |> Seq.toArray)



// Define the data extractors.
// avatar field
let avatar =
    "div:nth-child(1).anchor > div:nth-child(2) > div.row.data-row > div.col-md-5 > div:nth-child(1).media > a:nth-child(1).media-left > img:nth-child(1).avatar.lazy"
    |> Extractor.New
    |> Extractor.WithAttributes ["data-original"]

// screenName and account fields
let screenName =
    "div > div > div.media-body.twitter-media-body > h4.media-heading > a"
    |> Extractor.New
    |> Extractor.WithAttributes ["text"; "href"]

// tweet text field
let tweet =
    "div > div > div > div.media-body.twitter-media-body > p"
    |> Extractor.New

let extractors = [avatar; screenName; tweet]

// Get some help to generate the record code that matches the extractors
let code = CodeGen.recordCode extractors

// Describe the data model.
type Tweet =
    {
        avatar: string
        screenName: Map<string, string>
        tweet: string
    }

// Initialize a scraper
let scraper = Scraper<Tweet> extractors

let url = "http://fsharp-hub.apphb.com/"

// Scrape a single item
let firstMatch = scraper.Scrape url

// Or scrape all the items
let allMatches = scraper.ScrapeAll url

// Scrape multiple pages and let the scraper handle storing
// the records, then get the data as an array or in JSON format.
let data = scraper.Data()

let jsonData = scraper.JsonData()

// Save as CSV 
let desktop = Environment.GetFolderPath Environment.SpecialFolder.Desktop
let path = Path.Combine(desktop, "data.csv")
scraper.SaveCsv(path)

// Save an Excel workbook
let path' = Path.Combine(desktop, "data.xlsx")
scraper.SaveExcel path'


