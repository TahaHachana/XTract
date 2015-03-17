module XTract.Html

open Fizzler.Systems.HtmlAgilityPack
open HtmlAgilityPack
open System

let private optionFromPotentialNull =
    function
    | null -> None
    | x -> Some x

/// <summary>
/// Loads an HTML document from the specified string
/// and returns its root node.
/// </summary>
/// <param name="html">The HTML string.</param>
let loadRoot html =
    let document = HtmlDocument()
    document.LoadHtml html
    document.DocumentNode

/// <summary>
/// Retrieves the first element node from descendants
/// of the starting element node that matches any selector
/// within the supplied selector strings.
/// </summary>
/// <param name="startingNode">The starting HTML element node.</param>
/// <param name="selector">The CSS selector string.</param>
let cssSelect selector (startingNode: HtmlNode) =
    startingNode.QuerySelector selector
    |> optionFromPotentialNull

/// <summary>
/// Returns the text between the start and end tags of the
/// supplied node after decoding HTML encoded characters and removing
/// extra white-space and newlines.
/// </summary>
/// <param name="htmlNode"></param>
let innerText (htmlNode: HtmlNode) =
    String.htmlDecode htmlNode.InnerText
    |> String.stripSpaces

/// <summary>
/// Returns the inner text of the first element node
/// from descendants of the starting element node that
/// matches any selector withing the supplied selector strings.
/// </summary>
/// <param name="selector">The CSS selector string.</param>
/// <param name="startingNode">The starting HTML element node.</param>
let cssSelectInnerText selector (startingNode: HtmlNode) =
    startingNode.QuerySelector selector
    |> function
    | null -> String.Empty
    | x -> innerText x

/// <summary>
/// Retrieves all element nodes from descendants
/// of the starting element node that match any selector
/// within the supplied selector strings.
/// </summary>
/// <param name="selector">The CSS selector string.</param>
/// <param name="startingNode">The starting HTML element node.</param>
let cssSelectAll selector (startingNode: HtmlNode) =
    startingNode.QuerySelectorAll selector
    |> function
    | null -> None
    | x ->
        Seq.toList x
        |> Some

/// <summary>
/// Retrieves the first element node from descendants
/// of the starting element node that matches the supplied
/// XPath expression.
/// </summary>
/// <param name="xpath">The XPath expression.</param>
/// <param name="startingNode">The starting HTML element node.</param>
let xpathSelect xpath (startingNode: HtmlNode) =
    startingNode.SelectSingleNode xpath
    |> optionFromPotentialNull

/// <summary>
/// Returns the inner text of the first element node
/// from descendants of the starting element node that
/// matches the supplied XPath expression.
/// </summary>
/// <param name="xpath">The XPath expression.</param>
/// <param name="startingNode">The starting HTML element node.</param>
let xpathSelectInnerText xpath (startingNode: HtmlNode) =
    startingNode.SelectSingleNode xpath
    |> function
    | null -> String.Empty
    | x -> innerText x

/// <summary>
/// Retrieves all element nodes from descendants
/// of the starting element node that match the supplied
/// XPath expression.
/// </summary>
/// <param name="xpath">The XPath expression.</param>
/// <param name="startingNode">The starting HTML element node.</param>
let xpathSelectAll xpath (startingNode: HtmlNode) =
    startingNode.SelectNodes xpath
    |> function
    | null -> None
    | x ->
        Seq.toList x
        |> Some

/// <summary>
/// Returns the title of the supplied HTML string.
/// </summary>
/// <param name="html">The HTML string.</param>
let title html =
    loadRoot html
    |> xpathSelect "//title"
    |> function
    | None -> None
    | Some htmlNode ->
        innerText htmlNode
        |> Some

/// Describes an HTML meta tag.
type Meta =
    {
        name : string
        content : string
        httpEquiv : string
    }
        
    static member New name content httpEquiv =
        {
            name = name
            content = content
            httpEquiv = httpEquiv
        }

/// <summary>
/// Returns the meta tags of the supplied HTML string.
/// </summary>
/// <param name="html">The HTML string.</param>
let meta html =
    loadRoot html
    |> xpathSelectAll "//meta"
    |> function
    | None -> None
    | Some lst ->
        lst
        |> List.map (fun x ->
            let name = x.GetAttributeValue("name", "")
            let content =
                x.GetAttributeValue("content", "")
                |> String.htmlDecode
                |> String.stripSpaces
            let httpEquiv = x.GetAttributeValue("http-equiv", "")
            Meta.New name content httpEquiv
        )
        |> Some

/// <summary>
/// Returns the meta description of the supplied HTML string.
/// </summary>
/// <param name="html">The HTML string.</param>
let metaDescription html =
    loadRoot html
    |> xpathSelect "//meta[@name='description']"
    |> function
    | None -> None
    | Some htmlNode ->
        htmlNode.GetAttributeValue("content", "")
        |> function
        | "" -> None
        | str ->
            String.htmlDecode str
            |> String.stripSpaces
            |> Some
    
/// <summary>
/// Returns the meta keywords of the supplied HTML string.
/// </summary>
/// <param name="html">The HTML string.</param>
let metaKeywords html =
    loadRoot html
    |> xpathSelect "//meta[@name='keywords']"
    |> function
    | None -> None
    | Some htmlNode ->
        htmlNode.GetAttributeValue("content", "")
        |> function
        | "" -> None
        | str ->
            String.htmlDecode str
            |> String.stripSpaces
            |> Some

/// An HTML heading tag.
type Heading =
    {
        level : int
        text : string
    }

    static member New level text =
        {
            level = level
            text = text
        }  

/// <summary>
/// Returns the headings of the supplied HTML string.
/// </summary>
/// <param name="html">The HTML string.</param>
let headings html =
    loadRoot html
    |> xpathSelectAll "//*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5 or self::h6]"
    |> function
    | None -> None
    | Some lst ->
        lst
        |> List.map (fun x ->
            let level = x.Name.[1] |> int
            let text = innerText x
            Heading.New level text
        )
        |> Some