#load "../Src/packages/XTract.0.2.6/XTractBootstrap.fsx"

open XTract

let profileUrl = "http://www.nuget.org/profiles/Taha"

type PackageListing =
    {
        name: string
        url : string
    }

let extractor =
    "li > section > div > h1 > a"
    |> Extractor.New
    |> Extractor.WithAttributes ["text"; "href"]
    
let scraper = Scraper<PackageListing>([extractor])

let pkgs = scraper.ScrapeAll profileUrl


