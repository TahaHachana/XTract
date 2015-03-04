module XTract.Http

open System
open System.Net
open System.Net.Http

/// Describes a HTTP response received after sending a GET web request.
type HttpResponse =
    {
        requestUri: Uri
        statusCode: HttpStatusCode
        contentType: string
        isHtml: bool
        html: string option
//        hyperlinks: Hyperlink list
    }


type HC = HttpClient

/// Wraps the System.Net.Http.HttpClient class.
type HttpClient() =
    // Init a System.Net.Http.HttpClient
    let httpClient = new HC()
    // Pretend to be Googlebot
    do httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)")

    /// Sends an asynchronous GET request to the specified Uri.
    member private __.GetResponseAsync (requestUri: Uri) =
        httpClient.GetAsync(requestUri)
        |> Async.AwaitTask

    /// Reads the HTTP content as a string.
    member private __.ReadAsStringAsync (content:HttpContent) =
        content.ReadAsStringAsync()
        |> Async.AwaitTask

    /// Reads the HTTP content as a string if its content type is HTML.
    member private __.ReadAsync statusCode isHtml content =
        async {
            match statusCode with
            | HttpStatusCode.OK ->
                match isHtml with
                | false -> return None
                | true ->
                    let! html = __.ReadAsStringAsync content
                    return Some html
            | _ -> return None
        }
    
    /// Collects a hyperlinks list from the specified HTML document
    /// using the request URL as a referrer.
//    member private __.LinksFromHtml html requestUri =
//        match html with
//        | None -> []
//        | Some html ->
//            let links = Links.fromHtml html requestUri
//            match links.IsNone with
//            | false -> Option.get links
//            | true -> []

    /// Creates a new HttpResponse instance from the HTTP response message.
    member private __.MakeHttpResponse (httpResponseMsg: HttpResponseMessage) =
        async {
            let requestUri = httpResponseMsg.RequestMessage.RequestUri
            let statusCode = httpResponseMsg.StatusCode
            let content = httpResponseMsg.Content
            let contentType = content.Headers.ContentType.MediaType
            let isHtml = contentType.ToLower().Contains "html"
            let! html = __.ReadAsync statusCode isHtml content
//            let links = __.LinksFromHtml html <| requestUri.ToString()
            let httpResponse =
                {
                    requestUri = requestUri
                    statusCode = statusCode
                    contentType = contentType
                    isHtml = isHtml
                    html = html
//                    hyperlinks = links
                }
            return httpResponse
        }

    /// Sends an asynchronous GET request to the specified URI and
    /// returns a HttpResponse option.
    member __.GetAsync(requestUri) =
        async {
            try
                let! httpResponseMsg = __.GetResponseAsync requestUri
                let! httpResponse = __.MakeHttpResponse httpResponseMsg
                return Some httpResponse
            with _ -> return None
        }
        |> Async.StartWithTimeout 60000.

    interface IDisposable with
        member __.Dispose() = httpClient.Dispose()

let getAsync url =
    async {
        use client = new HttpClient()
        let! resp = client.GetAsync(Uri url)
        match resp with
        | None -> return None
        | Some x -> return x.html
    }    

let get url =
    getAsync url
    |> Async.RunSynchronously