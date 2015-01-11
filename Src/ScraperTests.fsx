#I @"bin\Release\"
#r "XTract.dll"
#r @"C:\Users\AHMED\Documents\GitHub\XTract\Src\packages\Deedle.1.0.6\lib\net40\Deedle.dll"

open System
open System.IO
open XTract

let name =
    "div > div > div > h1 > span"
    |> Extractor.New

let location =
    """//*[@id="root"]/div[3]/div[2]/div/div[2]/div/div[2]/div[1]/div/div[2]/div/div[1]/div/div[2]/div/div[3]/div/div[2]/div/span/a"""
    |> Extractor.New
    |> Extractor.WithType Xpath

let roles =
    """//div[@data-field="tags_roles"]/span"""
    |> Extractor.New
    |> Extractor.WithType Xpath
    |> Extractor.WithMany true

let linkedin =
    ".fontello-linkedin"
    |> Extractor.New
    |> Extractor.WithAttributes ["href"]

let twitter =
    ".fontello-twitter"
    |> Extractor.New
    |> Extractor.WithAttributes ["href"]

let facebook =
    ".fontello-facebook"
    |> Extractor.New
    |> Extractor.WithAttributes ["href"]

let website =
    """//a[@data-field="online_bio_url"]"""
    |> Extractor.New
    |> Extractor.WithAttributes ["href"]
    |> Extractor.WithType Xpath

let extractors = [name; location; roles; linkedin; twitter; facebook; website]

type Investor =
    {
        Name: string
        Location: string
        Roles: string list
        Linkedin: string
        Twitter: string
        Facebook: string
        Website: string
    }

let scraper = Scraper<Investor> extractors

scraper.Scrape "https://angel.co/asenkut"

scraper.Scrape "https://angel.co/moskov"










































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

// Get some help to generate the record type that matches the extractors
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
// the records, then get the data as an array, in JSON format
// or as a Deedle data frame.
let data = scraper.Data

let jsonData = scraper.JsonData

let df = scraper.DataFrame

// Save as CSV 
let desktop = Environment.GetFolderPath Environment.SpecialFolder.Desktop
let path = Path.Combine(desktop, "data.csv")
scraper.SaveCsv(path)

// Save an Excel workbook
let path' = Path.Combine(desktop, "data.xlsx")
scraper.SaveExcel path'


