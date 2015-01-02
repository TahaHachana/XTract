module XTract.Http

open System.Net.Http

let getAsync (url:string) =
    async {
        try
            let client = new HttpClient()

            client.DefaultRequestHeaders.Add
                ("user-agent", 
                 "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36")

            let html =
                client.GetAsync url
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> fun x -> x.Content.ReadAsStringAsync()
                |> Async.AwaitTask
                |> Async.RunSynchronously
            return Some html
        with _ -> return None
    }

let get url =
    getAsync url
    |> Async.RunSynchronously