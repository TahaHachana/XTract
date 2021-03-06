﻿module XTract.Links

open HtmlAgilityPack
open System
open System.Text.RegularExpressions

/// An HTML link.
type Hyperlink =
    {
        uri: Uri
        anchor: string
        destination: Destination
        nofollow: bool
        referrer: string
    }

    static member New uri anchor destination nofollow referrer =
        {
            uri = uri
            anchor = anchor
            destination = destination
            nofollow = nofollow
            referrer = referrer
        }

and Destination = External | Internal

module private Helpers =

    /// Selects the nodes matching the "//a" XPath expression
    /// from the root node.
    let selectLinks (root:HtmlNode) =
        root.SelectNodes "//a"
        |> function
        | null -> None
        | htmlNodes ->
            Seq.toList htmlNodes
            |> Some

    let tryCreaTeUri uriString =
        match Uri.TryCreate(uriString, UriKind.Absolute) with
        | false, _ -> None
        | true, uri -> Some uri

    /// Returns the base URL for all relative URLs in a document.
    /// If the document doesn't specify one then the request
    /// URL is used.
    let getBaseUri (root:HtmlNode) requestUri =
        root.SelectSingleNode "//base"
        |> function
        | null -> tryCreaTeUri requestUri
        | htmlNode ->
            match htmlNode.GetAttributeValue("href", "") with
            | "" -> tryCreaTeUri requestUri
            | href ->
                match tryCreaTeUri href with
                | None -> tryCreaTeUri requestUri
                | Some uri -> Some uri

    /// Returns the destination address of a link as a URI.
    let hyperlinkUri (linkNode: HtmlNode) (baseUri: Uri option) =
        let href = linkNode.GetAttributeValue("href", "")
        match Regex("(?i)^(mailto:|location\.|javascript:)").IsMatch href with
        | false ->
            let isAbsolute = href.StartsWith "http"
            match isAbsolute with
            | false ->
                match baseUri with
                | None -> None
                | Some uri ->
                    match Uri.TryCreate(uri, href) with
                    | false, _ -> None
                    | true, uri -> Some uri
            | true -> tryCreaTeUri href
        | true -> None

    /// Returns the anchor of a link.
    let getAnchor (linkNode:HtmlNode) = String.stripHtml linkNode.InnerText

    /// Determines the destination of a link's address.
    let getDestination (uri:Uri) host =
        match uri.Host = host with
        | false-> External
        | _ -> Internal

    /// Checks if the link's rel attribute has the "nofollow" value.
    let isNofollow (linkNode: HtmlNode) =
        let rel = linkNode.GetAttributeValue("rel", "")
        rel.ToLower().Contains("nofollow")

    let makeHyperlink baseUri host requestUri linkNode =
        let uriOption = hyperlinkUri linkNode baseUri
        match uriOption with
        | None -> None
        | Some uri ->
            let anchor = getAnchor linkNode
            let destination = getDestination uri host
            let nofollow = isNofollow linkNode
            Hyperlink.New uri anchor destination nofollow requestUri
            |> Some

open Helpers

/// <summary>
/// Retrieves a hyperlinks list from the supplied HTML document.
/// The referrer field is set to the specified request URL.
/// </summary>
/// <param name="html">The HTML string.</param>
/// <param name="requestUrl">The HTTP request URL.</param>
let fromHtml html requestUrl =
    let root = Html.loadRoot html
    let links = selectLinks root
    match links with
    | None -> None
    | _ ->
        let baseUri = getBaseUri root requestUrl
        match baseUri with
        | None -> None
        | _ ->
            let host = (Option.get baseUri).Host
            Option.get links
            |> List.choose (makeHyperlink baseUri host requestUrl)
            |> function
            | [] -> None
            | lst -> Some lst