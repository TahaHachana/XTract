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
let select (htmlNode:HtmlNode) cssSelector =
    htmlNode.QuerySelector cssSelector
    |> function
    | null -> None
    | x -> Some x

/// Retrieves all the elements matching the CSS selector.
let selectAll (htmlNode:HtmlNode) cssSelector =
    htmlNode.QuerySelectorAll cssSelector
    |> function
    | null -> None
    | x ->
        x
        |> Seq.toList
        |> Some
