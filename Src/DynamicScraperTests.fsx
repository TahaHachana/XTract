#I @"bin\Release\"
#r "XTract.dll"

open System
open System.IO
open XTract
open XTract.Settings

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

// Dynamic scraper
let desktop = Environment.GetFolderPath Environment.SpecialFolder.Desktop
let path = Path.Combine(desktop, "chromedriver_win32")
XTractSettings.chromeDriverDirectory <- path

let dScraper = DynamicScraper<Tweet> [avatar; screenName; tweet]

dScraper.Get "http://fsharp-hub.apphb.com/"
dScraper.ScrapeAll() |> ignore
dScraper.Data().Length = 50

dScraper.Quit()

