#I @"bin\Release\"
#r "XTract.dll"

open System
open System.IO
open XTract

// Define some data extractors
let avatar =
    "div:nth-child(1).anchor > div:nth-child(2) > div.row.data-row > div.col-md-5 > div:nth-child(1).media > a:nth-child(1).media-left > img:nth-child(1).avatar.lazy"
    |> Extractor.New
    |> Extractor.WithAttributes ["data-original"]

let screenName =
    "div > div > div.media-body.twitter-media-body > h4.media-heading > a"
    |> Extractor.New
    |> Extractor.WithAttributes ["text"; "href"]

let tweet =
    "div > div > div > div.media-body.twitter-media-body > p"
    |> Extractor.New
    |> Extractor.WithAttributes ["text"]

// Work with the data in a strongly-typed fashion,
// the fields must match the extractors order.
type Tweet =
    {
        // First extractor attributes
        avatar: string
        // Second extractor attributes
        screenName: string
        account: string
        // Third extractor attributes
        tweet: string
    }

// Initialize a scraper
let scraper = Scraper<Tweet> [avatar; screenName; tweet]

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
