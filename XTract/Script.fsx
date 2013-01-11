#I @"bin\Release\"
#r "XTract"

open XTract

let html =
    use client = new System.Net.WebClient()
    client.DownloadString "http://fssnip.net"

let document = Document html

let credits =
    document.NodeText "/html/body/div/div[4]/div[3]/p"
    |> String.stripSpaces