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
open System.Net.Http
open XTract

let client = new HttpClient()

client.DefaultRequestHeaders.Add
    ("user-agent", 
     "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36")

let fetch (url : string) = 
    client.GetAsync url
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> fun x -> x.Content.ReadAsStringAsync()
    |> Async.AwaitTask
    |> Async.RunSynchronously

let url = "http://fsharp-hub.apphb.com/"
let html = fetch url

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

// Scrape a single item
let firstMatch = scraper.Scrape html

// Or scrape all the items
let allMatches = scraper.ScrapeAll html

// Scrape multiple pages and let the scraper handle storing
// the records, the get the data as an array or in JSON format.
let data = scraper.GetData()

let jsonData = scraper.GetJsonData()
```

Contact
-------

* Email: tahahachana@gmail.com
* [@TahaHachana](https://twitter.com/TahaHachana "Twitter")
* [Blog](http://fsharp-code.blogspot.com/)
* [+Taha](https://plus.google.com/103826666258148033768/ "Google+")