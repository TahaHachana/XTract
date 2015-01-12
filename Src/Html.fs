module XTract.Html

open Fizzler.Systems.HtmlAgilityPack
open HtmlAgilityPack

/// Loads a HTML document.
let load html =
    let document = HtmlDocument()
    document.LoadHtml html

/// Gets the root node of a document.
let root (htmlDocument:HtmlDocument) = htmlDocument.DocumentNode 

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
