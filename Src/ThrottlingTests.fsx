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


// Throttling tests
let asyncs =
    List.replicate 10 url
    |> List.map (fun x -> async { scraper.ScrapeAll x |> ignore; ()})

Throttler.throttle asyncs 4 (async {printfn "Done!"})

scraper.Data().Length = 500
