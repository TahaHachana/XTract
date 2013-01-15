namespace XTract

#if INTERACTIVE
#r ""
#endif

open System
open HtmlAgilityPack

module private Utilities =

    let attributes (htmlNode : HtmlNode) =
        htmlNode.Attributes
        |> Seq.cast<HtmlAttribute>
        |> Seq.toList

    let metaContent namePattern htmlNode =
        let nodeAttributes = attributes htmlNode
        nodeAttributes
        |> List.exists (fun x -> Regex.isMatch "(?i)name" x.Name && Regex.isMatch namePattern x.Value)
        |> function
            | false -> None
            | true  ->
                nodeAttributes
                |> List.tryFind (fun x -> Regex.isMatch "(?i)content" x.Name)
                |> function None -> None | Some attr -> String.checkEmpty attr.Value

    let metaTag nodesListOption name =
        nodesListOption |> function
            | None           -> None
            | Some nodesList -> List.tryPick (fun x -> metaContent name x) nodesList

module Node =

    /// <summary>Returns the first node that matches the specified XPath expression.</summary>
    /// <param name="xpath">The XPath expression.</param>
    /// <param name="htmlDocument">The HtmlDocument object.</param>
    /// <returns>The selected HtmlNode object.</returns>
    let select (htmlDocument : HtmlDocument) xpath =
        htmlDocument.DocumentNode.SelectSingleNode xpath
        |> function null -> None | node -> Some node

    /// <summary>Returns the list of nodes that match the specified XPath expression.</summary>
    /// <param name="xpath">The XPath expression.</param>
    /// <param name="htmlDocument">The HtmlDocument object.</param>
    /// <returns>The HtmlNode list.</returns>
    let selectList (htmlDocument : HtmlDocument) xpath =
        htmlDocument.DocumentNode.SelectNodes xpath
        |> function
            | null  -> None
            | nodes ->
                nodes
                |> Seq.cast<HtmlNode>
                |> Seq.toList
                |> Some

    /// <summary>Returns the inner text of a HTML node.</summary>
    /// <param name="htmlNodeOption">The HtmlNode option object.</param>
    /// <returns>The inner text of the HTML node.</returns>
    let innerText (htmlNodeOption : HtmlNode option) =
        htmlNodeOption
        |> function
            | None      -> None
            | Some node ->
                node.InnerText.Trim()
                |> String.checkEmpty

module Html =

    /// <summary>Loads the specified string as a HtmlDocument.</summary>
    /// <param name="htmlString">The string representing the HTML.</param>
    /// <returns>The HtmlDocument object.</returns>
    let load htmlString =
        let htmlDocument = HtmlDocument()
        htmlDocument.LoadHtml htmlString
        htmlDocument

    /// <summary>Returns the title of a HTML document.</summary>
    /// <param name="htmlDocument">The HtmlDocument object.</param>
    /// <returns>The title of the document.</returns>
    let title htmlDocument =
        Node.select htmlDocument "/html/head/title"
        |> Node.innerText
 
    /// <summary>Returns the list of HTML meta tags found in the specified document.</summary>
    /// <param name="htmlDocument">The HtmlDocument object.</param>
    /// <returns>The meta tags list.</returns>
    let metas htmlDocument = Node.selectList htmlDocument "/html/head/meta"

    /// <summary>Returns the meta description of a HTML document.</summary>
    /// <param name="htmlDocument">The HtmlDocument object.</param>
    /// <returns>The meta description as a string option.</returns>
    let metaDescription htmlDocument =
        metas htmlDocument
        |> function
            | None -> None
            | Some nodesList -> nodesList |> List.tryPick (fun x -> Utilities.metaContent "(?i)description" x)

    /// <summary>Returns the meta keywords of a HTML document.</summary>
    /// <param name="htmlDocument">The HtmlDocument object.</param>
    /// <returns>The meta keywords as a string option.</returns>
    let metaKeywords htmlDocument =
        metas htmlDocument
        |> function
            | None -> None
            | Some nodesList -> nodesList |> List.tryPick (fun x -> Utilities.metaContent "(?i)keywords" x)

    /// <summary>Returns the value of the href attribute of the base tag in a HTML document.</summary>
    /// <param name="htmlDocument">The HtmlDocument object.</param>
    /// <returns>The base URI of the document.</returns>
    let baseUri htmlDocument =
        Node.select htmlDocument "/html/head/base"
        |> function
            | None      -> None
            | Some node ->
                Utilities.attributes node
                |> List.tryFind (fun x -> Regex.isMatch "(?i)href" x.Name)
                |> function
                    | None      -> None
                    | Some attr ->
                        attr.Value
                        |> fun x -> Uri.TryCreate(x, UriKind.Absolute)
                        |> function
                            | false, _   -> None
                            | true , uri -> Some uri

//    let html =
//        use client = new System.Net.WebClient ()
//        client.DownloadString "http://www.intel.com/content/www/us/en/homepage.html"
//        
//    let doc = load html
//    let bU = baseUri doc





type DataExtractor(htmlString) =

    let htmlDocument = Html.load htmlString
    let metaTagsOption = Html.metas htmlDocument
    let metaTag' = Utilities.metaTag metaTagsOption

    /// <summary>Returns the inner text of a HTML node.</summary>
    /// <param name="xpath">The XPath expression of the HTML node.</param>
    /// <returns>The inner text of the HTML node.</returns>
    member __.NodeText xpath =
        Node.select htmlDocument xpath
        |> Node.innerText

    /// <summary>Returns the title of the HTML document.</summary>
    /// <returns>The title of the document.</returns>
    member __.Title htmlDocument =
        Node.select htmlDocument "/html/head/title"
        |> Node.innerText

    /// <summary>Returns the meta description of a HTML document.</summary>
    /// <param name="htmlDocument">The HtmlDocument object.</param>
    /// <returns>The meta description as a string option.</returns>
    member __.MetaDescription = metaTag' "(?i)description"

    /// <summary>Returns the meta description of a HTML document.</summary>
    /// <param name="htmlDocument">The HtmlDocument object.</param>
    /// <returns>The meta description as a string option.</returns>
    member __.MetaKeywords = metaTag' "(?i)keywords"

    /// <summary>Returns the value of the href attribute of the base tag in a HTML document.</summary>
    /// <param name="htmlDocument">The HtmlDocument object.</param>
    /// <returns>The base URI of the document.</returns>
    member __.BaseUri =
        Node.select htmlDocument "/html/head/base"
        |> function
            | None      -> None
            | Some node ->
                Utilities.attributes node
                |> List.tryFind (fun x -> Regex.isMatch "(?i)href" x.Name)
                |> function
                    | None      -> None
                    | Some attr ->
                        attr.Value
                        |> fun x -> Uri.TryCreate(x, UriKind.Absolute)
                        |> function
                            | false, _   -> None
                            | true , uri -> Some uri
    