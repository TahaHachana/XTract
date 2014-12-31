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
open Newtonsoft.Json
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
    Extractor.New "avatar" "div:nth-child(1).anchor > div:nth-child(2) > div.row.data-row > div.col-md-5 > div:nth-child(1).media > a:nth-child(1).media-left > img:nth-child(1).avatar.lazy" //".lazy"
    |> Extractor.WithAttributes ["data-original"]

let screenName =  
    Extractor.New "screenName" "div > div > div.media-body.twitter-media-body > h4.media-heading > a"
    |> Extractor.WithAttributes ["text"]

let tweet =
    Extractor.New "tweet" "div > div > div > div.media-body.twitter-media-body > p"
    |> Extractor.WithAttributes ["text"]

// Initialize a scraper
let scraper = Scraper [avatar; screenName; tweet]

let firstMatch = scraper.Scrape html
let allMatches = scraper.ScrapeAll html

// Work with the data in a strongly-typed fashion
type Tweet =
    {
      ``avatar-data-original``: string
      ``screenName-text``: string
      ``tweet-text``: string
    }

let tweetRecord =
    JsonConvert.DeserializeObject(firstMatch, typeof<Tweet>)
    :?> Tweet

let tweetRecords =
    JsonConvert.DeserializeObject(allMatches, typeof<Tweet list>)
    :?> Tweet list
```

Contact
-------

* Email: tahahachana@gmail.com
* [@TahaHachana](https://twitter.com/TahaHachana "Twitter")
* [Blog](http://fsharp-code.blogspot.com/)
* [+Taha](https://plus.google.com/103826666258148033768/ "Google+")