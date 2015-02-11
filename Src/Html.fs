module XTract.Html

open Fizzler.Systems.HtmlAgilityPack
open HtmlAgilityPack
open System.Text.RegularExpressions
open System

/// Loads a HTML document and returns its root node.
let load html =
    let document = HtmlDocument()
    document.LoadHtml html
    document.DocumentNode

/// Retrieves the first element matching the CSS selector.
let cssSelect (htmlNode:HtmlNode) cssSelector =
    htmlNode.QuerySelector cssSelector
    |> function
    | null -> None
    | x -> Some x

/// Retrieves all the elements matching the CSS selector.
let cssSelectAll (htmlNode:HtmlNode) cssSelector =
    htmlNode.QuerySelectorAll cssSelector
    |> function
    | null -> None
    | x ->
        x
        |> Seq.toList
        |> Some

/// Retrieves the first element matching the CSS selector.
let xpathSelect (htmlNode:HtmlNode) xpath =
    htmlNode.SelectSingleNode xpath
    |> function
    | null -> None
    | x -> Some x

/// Retrieves all the elements matching the CSS selector.
let xpathSelectAll (htmlNode:HtmlNode) xpath =
    htmlNode.SelectNodes xpath
    |> function
    | null -> None
    | x ->
        x
        |> Seq.toList
        |> Some

// todo: mark as private
//let eltText html xpath =
//    let root = load html
//    xpathSelect root xpath
//    |> function
//    | None -> None
//    | Some htmlNode ->
//        htmlNode.InnerText
//        |> function
//        | "" -> None
//        | str ->
//            String.decodeHtml str
//            |> String.stripSpaces
//            |> Some

let title html =
    let root = load html
    xpathSelect root "//title"
    |> function
    | None -> None
    | Some htmlNode ->
        htmlNode.InnerText
        |> function
        | "" -> None
        | str ->
            String.decodeHtml str
            |> String.stripSpaces
            |> Some

type Meta =
    {
        HttpEquiv : string
        Name : string
        Content : string
    }
        
    static member New content httpEquiv name =
        {
            HttpEquiv = httpEquiv
            Name = name
            Content = content
        }

let meta html =
    let root = load html
    xpathSelectAll root "//meta"
    |> function
    | None -> None
    | Some lst ->
        lst
        |> List.map (fun x ->
            let content =
                x.GetAttributeValue("content", "")
                |> String.decodeHtml
                |> String.stripSpaces
            let httpEquiv = x.GetAttributeValue("http-equiv", "")
            let name = x.GetAttributeValue("name", "")
            Meta.New content httpEquiv name
        )
        |> Some

let metaDescription html =
    let root = load html
    xpathSelect root "//meta[@name='description']"
    |> function
    | None -> None
    | Some htmlNode ->
        htmlNode.GetAttributeValue("content", "")
        |> function
        | "" -> None
        | str ->
            String.decodeHtml str
            |> String.stripSpaces
            |> Some
        
let metaKeywords html =
    let root = load html
    xpathSelect root "//meta[@name='keywords']"
    |> function
    | None -> None
    | Some htmlNode ->
        htmlNode.GetAttributeValue("content", "")
        |> function
        | "" -> None
        | str ->
            String.decodeHtml str
            |> String.stripSpaces
            |> Some

type Heading =
    {
        Level : int
        Text : string
    }

    static member New level text =
        {
            Level = level
            Text = text
        }  

let headings html =
    let root = load html
    xpathSelectAll root "//*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5 or self::h6]"
    |> function
    | None -> None
    | Some lst ->
        lst
        |> List.map (fun x ->
            let level = x.Name.[1..] |> int
            let text =
                x.InnerText
                |> String.decodeHtml
                |> String.stripSpaces
            Heading.New level text
        )
        |> Some

type Hyperlink =
    {
        Href: string
        Anchor: string
        Type: LinkType
        Rel: Rel
    }

    static member New href anchor linkType rel =
        {
            Href = href
            Anchor = anchor
            Type = linkType
            Rel = rel
        }

and LinkType = External | Internal

and Rel = Follow | NoFollow

let internal baseUri root url =
    xpathSelect root "//base"
    |> function
    | None -> url
    | Some htmlNode ->
        htmlNode.GetAttributeValue("href", "")
        |> function
        | "" -> url
        | x -> x
    |> fun x ->
        Uri.TryCreate(x, UriKind.Absolute)
        |> function
        | false, _ -> None
        | true, uri -> Some uri

let internal makeLink host (href:string) (rel:string) anchor =
    let linkType =
        match href.Contains host with
        | false -> External
        | true -> Internal
    let rel' =
        match rel.ToLowerInvariant().Contains("nofollow") with
        | false -> Follow
        | true -> NoFollow
    Hyperlink.New href anchor linkType rel'

let internal aTags root =
    xpathSelectAll root "//a"

let internal linkAttrs (htmlNode:HtmlNode) =
    let href = htmlNode.GetAttributeValue("href", "")
    let rel = htmlNode.GetAttributeValue("rel", "")
    let anchor =
        htmlNode.InnerText
        |> String.stripHtml
        |> String.decodeHtml
        |> String.stripSpaces
    href, rel, anchor

let private makeAbsolute (baseUri:Uri) (href:string) rel anchor =
    Uri.TryCreate(baseUri, href)
    |> function
    | false, _ -> None
    | true, uri -> Some (uri.ToString(), rel, anchor)

let private checkAbsolute href rel anchor =
    Uri.TryCreate(href, UriKind.Absolute)
    |> function
    | false, _ -> None
    | true, uri -> Some (uri.ToString(), rel, anchor)

let links html url =
    let root = load html
    aTags root
    |> function
    | None -> None
    | Some htmlNodes ->
        let links =
            htmlNodes
            |> List.map linkAttrs
            |> List.filter (fun (href, _, _) ->
                not <| Regex("(?i)^(mailto:|location\.|javascript:)").IsMatch href)
        let absolute, relative =
            links
            |> List.partition (fun (href, _, _) -> Regex("^http").IsMatch href)
        let absolute'= absolute |> List.choose (fun (href, rel, anchor) -> checkAbsolute href rel anchor)
        let ``base`` = baseUri root url
        let relative' =
            match ``base`` with
            | None -> relative
            | Some baseUri ->
                relative
                |> List.choose (fun (href, rel, anchor) ->
                    makeAbsolute baseUri href rel anchor)
        Uri.TryCreate(url, UriKind.Absolute)
        |> function
        | false, _ -> None
        | true, uri ->
            let host = uri.Host
            List.append relative' absolute'
            |> List.map (fun (href, rel, anchor) -> makeLink host href rel anchor)
            |> Some