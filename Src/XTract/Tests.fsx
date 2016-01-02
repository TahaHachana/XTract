
#r @"C:\Users\AHMED\Documents\GitHub\XTract\Src\XTract\bin\Release\XTract.Core.dll"
#r @"C:\Users\AHMED\Documents\GitHub\XTract\Src\XTract\bin\Release\XTract.dll"

open XTract

let asyncs =
    Array.init 500 (fun _ -> "http://fsharp.org/")
    |> Array.map (fun x ->
        async {
            printfn "%s" x
            let! html = Http.getAsync x
            ()
        }
    )

let throttler = new ThrottlingAgent()

throttler.Work asyncs
|> Async.RunSynchronously



//// Business data fields extractors
//let name =
//    Css "div > section > div > h1 > span"
//    |> Extractor.New
//
//let address =
//    Css "section.contentBlock.contentBlockBasic > div.businessNameLocationRating > div.itemAddress.h3 > span > span:nth-child(1)"
//    |> Extractor.New
//
//let phone =
//    Xpath """//*[@id="businessDetailsPrimary"]/div[2]/meta[1]"""
//    |> Extractor.New
//    |> Extractor.WithAttributes ["content"]
//
//let services =
//    Xpath """//dt[text()='Services']/following-sibling::dd/ul/li"""
//    |> Extractor.New
//    |> Extractor.WithMany true GroupBy.FifthParent
//
//// Describes a business listed on the directory
//type Business =
//    {
//        Name: string
//        Address: string
//        Phone: string
//        Services: string list
//        ItemUrl: string
//    }
//
//let extractors = [name; address; phone; services]
//
//let bScraper = Scraper<Business> extractors
//// Disable logging
//bScraper.WithLogging true
//
//bScraper.Scrape "http://yellow.co.nz/y/my-computer-whangarei?c=550"
//
