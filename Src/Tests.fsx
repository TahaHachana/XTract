#I @"bin\Release\"
#r """.\packages\Newtonsoft.Json.6.0.7\lib\net40\Newtonsoft.Json.dll"""
#r "System.Net.Http.dll"
#r "XTract.dll"

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

let avatar =
    Extractor.New "avatar" "div:nth-child(1).anchor > div:nth-child(2) > div.row.data-row > div.col-md-5 > div:nth-child(1).media > a:nth-child(1).media-left > img:nth-child(1).avatar.lazy" //".lazy"
    |> Extractor.WithAttributes ["data-original"]

let screenName =  
    Extractor.New "screenName" "div > div > div.media-body.twitter-media-body > h4.media-heading > a"
    |> Extractor.WithAttributes ["text"]

let tweet =
    Extractor.New "tweet" "div > div > div > div.media-body.twitter-media-body > p"
    |> Extractor.WithAttributes ["text"]

let scraper = Scraper [avatar; screenName; tweet]

let json = scraper.Scrape html
let json' = scraper.ScrapeAll html

type Tweet =
    {
      ``avatar-data-original``: string
      ``screenName-text``: string
      ``tweet-text``: string
    }

let tweetRecord =
    JsonConvert.DeserializeObject(json, typeof<Tweet>)
    :?> Tweet

let tweetRecords =
    JsonConvert.DeserializeObject(json', typeof<Tweet list>)
    :?> Tweet list

tweetRecords.Length
