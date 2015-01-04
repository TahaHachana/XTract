#I @"bin\Release\"
#r "XTract.dll"

open System
open System.IO
open XTract
open XTract.Settings

// Describe the data model.
type Tweet =
    {
        avatar: string
        screenName: string
        account: string
        tweet: string
    }

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
    |> Extractor.WithAttributes ["text"]

// Set the ChromeDriver.exe directory.
XTractSettings.chromeDriverDirectory <- __SOURCE_DIRECTORY__

// Initialize a dynamic scraper
let scraper = DynamicScraper<Tweet> [avatar; screenName; tweet]

scraper.Get "http://fsharp-hub.apphb.com/"
scraper.ScrapeAll() |> ignore
scraper.Data().Length = 50

// Dispose of the scraper safely.
scraper.Quit()