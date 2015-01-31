module XTract.Http

open System.Net.Http

let private getReq (client:HttpClient) (url:string) =
    client.GetAsync url
    |> Async.AwaitTask

let private readAsString (response:HttpResponseMessage) =
    response.Content.ReadAsStringAsync()
    |> Async.AwaitTask

let private setUserAgent (client:HttpClient)=
    client.DefaultRequestHeaders.Add
        ("user-agent", 
            "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36")

let getAsync (url:string) =
    async {
        try
            let client = new HttpClient()
            setUserAgent client
            let! response = getReq client url
            let! html = readAsString response
            return Some html
        with _ -> return None
    }
    |> Async.timeout 60000.

let get url =
    getAsync url
    |> Async.RunSynchronously