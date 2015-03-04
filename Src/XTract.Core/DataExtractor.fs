[<AutoOpen>]
module XTract.DataExtractor

open System
open HtmlAgilityPack

module private Utilities = do ()

//    let attributes (htmlNode : HtmlNode) =
//        htmlNode.Attributes
//        |> Seq.cast<HtmlAttribute>
//        |> Seq.toList
//
////    let metaContent namePattern htmlNode =
////        let nodeAttributes = attributes htmlNode
////        nodeAttributes
////        |> List.exists (fun x -> Regex.isMatch "(?i)name" x.Name && Regex.isMatch namePattern x.Value)
////        |> function
////            | false -> None
////            | true  ->
////                nodeAttributes
////                |> List.tryFind (fun x -> Regex.isMatch "(?i)content" x.Name)
////                |> function None -> None | Some attr -> String.cleanHtml attr.Value |> Some
////
////    let metaTag nodesListOption name =
////        nodesListOption |> function
////            | None           -> None
////            | Some nodesList -> List.tryPick (fun x -> metaContent name x) nodesList
//
//    let attributeHasValue (htmlNode : HtmlNode) namePattern valuePattern =
//        attributes htmlNode
//        |> List.exists (fun attribute ->
//            Regex.isMatch namePattern attribute.Name
//            &&
//            Regex.isMatch valuePattern attribute.Value)
//
//    let attributeValue (htmlNode : HtmlNode) namePattern =
//        attributes htmlNode
//        |> List.tryFind (fun x -> Regex.isMatch namePattern x.Name)
//        |> function
//            | None -> None
//            | Some attribute -> attribute.Value.Trim() |> Some
//
//    let tryCreateUri =
//        function
//            | None           -> None
//            | Some uriString ->
//                Uri.TryCreate(uriString, UriKind.Absolute)
//                |> function
//                    | false, _   -> None
//                    | true , uri -> Some uri
//
//    let tryCreateUri' hrefValue baseUri =
//        Uri.TryCreate(hrefValue, UriKind.Absolute)
//        |> function
//            | false, _   ->
//                Uri.TryCreate(baseUri, hrefValue)
//                |> function
//                    | false, _   -> None
//                    | true , uri -> Some uri
//            | true , uri -> Some uri
//
//
//module Node =
//
//    /// <summary>Returns the first node that matches the specified XPath expression.</summary>
//    /// <param name="xpath">The XPath expression.</param>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The selected HtmlNode object.</returns>
//    let select (htmlDocument : HtmlDocument) xpath =
//        htmlDocument.DocumentNode.SelectSingleNode xpath
//        |> function null -> None | node -> Some node
//
//    /// <summary>Returns the list of nodes that match the specified XPath expression.</summary>
//    /// <param name="xpath">The XPath expression.</param>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The HtmlNode list.</returns>
//    let selectList (htmlDocument : HtmlDocument) xpath =
//        htmlDocument.DocumentNode.SelectNodes xpath
//        |> function
//            | null  -> None
//            | nodes ->
//                nodes
//                |> Seq.cast<HtmlNode>
//                |> Seq.toList
//                |> Some
//
//    /// <summary>Returns the inner text of a HTML node.</summary>
//    /// <param name="htmlNodeOption">The HtmlNode option object.</param>
//    /// <returns>The inner text of the HTML node.</returns>
//    let innerText (htmlNodeOption : HtmlNode option) =
//        htmlNodeOption
//        |> function
//            | None      -> None
//            | Some node ->
//                node.InnerHtml
//                |> String.cleanHtml
//                |> Some
//    
//module Html =
//
//    /// <summary>Loads the specified string as a HtmlDocument.</summary>
//    /// <param name="htmlString">The string representing the HTML.</param>
//    /// <returns>The HtmlDocument object.</returns>
//    let load htmlString =
//        let htmlDocument = HtmlDocument()
//        htmlDocument.LoadHtml htmlString
//        htmlDocument
//
//    /// <summary>Returns the title of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The title of the document.</returns>
//    let title htmlDocument =
//        Node.select htmlDocument "//title"
//        |> Node.innerText
// 
//    /// <summary>Returns the list of HTML meta tags found in the specified document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The meta tags list.</returns>
//    let metas htmlDocument = Node.selectList htmlDocument "//meta"
//
//    let private metaContent htmlDocument name =
//        let xpath = "//meta[@name='" + name + "']"
//        Node.select htmlDocument xpath
//        |> function
//            | None -> None
//            | Some htmlNode -> Utilities.attributeValue htmlNode "content"
//
//    /// <summary>Returns the meta description of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The meta description as a string option.</returns>
//    let metaDescription htmlDocument = metaContent htmlDocument "description"
//
//    /// <summary>Returns the meta keywords of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The meta keywords as a string option.</returns>
//    let metaKeywords htmlDocument = metaContent htmlDocument "keywords"
//
//    /// <summary>Returns the value of the href attribute of the base tag in a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The base URI of the document.</returns>
//    let baseUri htmlDocument =
//        Node.select htmlDocument "//base"
//        |> function
//            | None      -> None
//            | Some node ->
//                Utilities.attributeValue node "(?i)href"
//                |> Utilities.tryCreateUri
//
//    /// <summary>Returns the link tags of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The link tags as a HtmlNode list.</returns>
//    let linkTags htmlDocument = Node.selectList htmlDocument "//link"
//
//    /// <summary>Returns canonical URI of a HTML document if it was specified.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The canonical URI.</returns>
//    let canonicalUri htmlDocument =
//        linkTags htmlDocument
//        |> function
//            | None           -> None
//            | Some htmlNodes ->
//                htmlNodes
//                |> List.tryFind (fun htmlNode ->
//                    Utilities.attributeHasValue htmlNode "(?i)rel" "(?i)canonical")
//                |> function
//                    | None          -> None
//                    | Some htmlNode ->
//                        Utilities.attributeValue htmlNode "(?i)href"
//                        |> Utilities.tryCreateUri
//
//    /// <summary>Returns the external stylesheets associated with a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <param name="baseUri">The URI that generated the HTML.</param>
//    /// <returns>The stylesheets URI list.</returns>    
//    let stylesheets htmlDocument baseUri =
//        linkTags htmlDocument
//        |> function
//            | None           -> None
//            | Some htmlNodes ->
//                htmlNodes
//                |> List.filter (fun htmlNode ->
//                    Utilities.attributeHasValue htmlNode "(?i)rel" "(?i)stylesheet")
//                |> function
//                    | [] -> None
//                    | htmlNodes ->
//                        htmlNodes
//                        |> List.choose (fun x -> Utilities.attributeValue x "(?i)href")
//                        |> List.choose (fun x -> Utilities.tryCreateUri' x baseUri)
//                        |> function
//                            | []       -> None
//                            | urisList -> Some urisList
//
//    /// <summary>Returns the external script files associated with a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <param name="baseUri">The URI that generated the HTML.</param>
//    /// <returns>The scripts URI list.</returns>
//    let scripts htmlDocument baseUri =
//        Node.selectList htmlDocument "//script"
//        |> function
//            | None           -> None
//            | Some htmlNodes ->
//                htmlNodes
//                |> List.choose (fun htmlNode -> Utilities.attributeValue htmlNode "(?i)src")
//                |> List.choose (fun x -> Utilities.tryCreateUri' x baseUri)
//                |> function
//                    | []       -> None
//                    | urisList -> Some urisList
//
//    type Heading =
//        | H1 of string
//        | H2 of string
//        | H3 of string
//        | H4 of string
//        | H5 of string
//        | H6 of string
//
//    let makeHeading (htmlNode : HtmlNode) =
//        let innerTextOption = Node.innerText <| Some htmlNode
//        match innerTextOption with
//            | None -> None
//            | Some innerText ->
//                let nodeName = htmlNode.Name
//                let level = Regex.patternGroup "\d" 0 nodeName
//                match level with
//                    | "1" -> H1 innerText
//                    | "2" -> H2 innerText
//                    | "3" -> H3 innerText
//                    | "4" -> H4 innerText
//                    | "5" -> H5 innerText
//                    | _   -> H6 innerText
//                |> Some
//
//    /// <summary>Returns the H1 heading of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The H1 heading.</returns>
//    let h1 htmlDocument =
//        Node.select htmlDocument "//h1"
//        |> function None -> None | Some h1Node -> makeHeading h1Node
//
//    /// <summary>Returns the headings of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The headings list.</returns>
//    let headings htmlDocument =
//        Node.selectList htmlDocument "//*[starts-with(name(),'h') and string-length(name()) = 2 and number(substring(name(), 2)) <= 6]"
//        |> function
//            | None -> None
//            | Some nodesList ->
//                List.choose makeHeading nodesList
//                |> function [] -> None | headingsList -> Some headingsList
//
//    let private uriOptionFromAttribute attributeName baseUri htmlNode =
//        Utilities.attributeValue htmlNode attributeName
//        |> function
//            | None                -> None
//            | Some attributeValue -> Utilities.tryCreateUri' attributeValue baseUri
//
//    let private href baseUri htmlNode = uriOptionFromAttribute "(?i)href" baseUri htmlNode
//
//    type LinkRel = Follow | NoFollow
//
//    let private rel htmlNode =
//        Utilities.attributeValue htmlNode "(?i)rel"
//        |> function
//            | None -> Follow
//            | Some relAttribute ->
//                Regex.isMatch "(?i)nofollow" relAttribute
//                |> function
//                    | false -> Follow
//                    | true  -> NoFollow 
//
//    type LinkDestination = External | Internal
//
//    type Link =
//        {
//            Href : Uri
//            Rel : LinkRel
//            Anchor : string option
//            Destination : LinkDestination
//        }
//    
//    let private makeLink host htmlNode (uriOption : Uri option) =
//        let href = uriOption.Value
//        let rel = rel htmlNode
//        let anchor = Node.innerText <| Some htmlNode
//        let destination = href.Host = host |> function false -> External | true -> Internal 
//        {
//            Href = href
//            Rel = rel
//            Anchor = anchor
//            Destination = destination
//        }
//
//    /// <summary>Returns the links of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <param name="baseUri">The URI that generated the HTML.</param>
//    /// <returns>The links list.</returns>
//    let links htmlDocument baseUri =
//        let href' = href baseUri
//        let host = baseUri.Host
//        let makeLink' = makeLink host
//        Node.selectList htmlDocument "//a"
//        |> function
//            | None -> None
//            | Some nodesList ->
//                nodesList
//                |> List.map (fun htmlNode -> htmlNode, href' htmlNode)
//                |> List.filter (fun (_, uriOption) -> uriOption.IsSome)
//                |> function
//                    | []  -> None
//                    | lst ->
//                        lst
//                        |>List.map (fun (htmlNode, uriOption) -> makeLink' htmlNode uriOption)
//                        |> Some
//                        
//    let private src baseUri htmlNode = uriOptionFromAttribute "(?i)src" baseUri htmlNode
//
//    type Image =
//        {
//            Src : Uri
//            Alt : string option
//            Height : string option
//            Width : string option
//        }
//    
//    let private makeImage src alt height width =
//        {
//            Src = src
//            Alt = alt
//            Height = height
//            Width = width
//        }
//
//    /// <summary>Returns the images of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <param name="baseUri">The URI that generated the HTML.</param>
//    /// <returns>The images list.</returns>
//    let images htmlDocument baseUri =
//        let src' = src baseUri
//        Node.selectList htmlDocument "//img"
//        |> function
//            | None           -> None
//            | Some nodesList ->
//                nodesList
//                |> List.map (fun htmlNode -> htmlNode, src' htmlNode)
//                |> List.filter (fun (_, uriOption) -> uriOption.IsSome)
//                |> function
//                    | []  -> None
//                    | lst ->
//                        lst
//                        |> List.map (fun (htmlNode, uriOption) ->
//                            let alt = Utilities.attributeValue htmlNode "(?i)alt"
//                            let height = Utilities.attributeValue htmlNode "(?i)height"
//                            let width = Utilities.attributeValue htmlNode "(?i)width"
//                            makeImage uriOption.Value alt height width)
//                        |> Some
//
//    /// <summary>Returns the value of a HtmlNode attribute.</summary>
//    /// <param name="attribute">The name of the attribute</param>
//    /// <param name="htmlNodeOption">The HtmlNode object option.</param>
//    /// <returns>The attribute value</returns>
//    let attributeValue attribute (htmlNodeOption : HtmlNode option) =
//        match htmlNodeOption with
//            | None          -> None
//            | Some htmlNode -> Utilities.attributeValue htmlNode attribute
//
//type DataExtractor(htmlString) =
//
//    let htmlDocument = Html.load htmlString
//    let metaTagsOption = Html.metas htmlDocument
//
//    /// <summary>Returns the first node that matches the specified XPath expression.</summary>
//    /// <param name="xpath">The XPath expression.</param>
//    /// <returns>The selected HtmlNode object.</returns>
//    member __.Select xpath = Node.select htmlDocument xpath
//
//    /// <summary>Returns the list of nodes that match the specified XPath expression.</summary>
//    /// <param name="xpath">The XPath expression.</param>
//    /// <returns>The HtmlNode list.</returns>
//    member __.SelectList xpath = Node.selectList htmlDocument xpath
//
//    /// <summary>Returns the inner text of a HTML node.</summary>
//    /// <param name="xpath">The XPath expression of the HTML node.</param>
//    /// <returns>The inner text of the HTML node.</returns>
//    member __.NodeText xpath =
//        Node.select htmlDocument xpath
//        |> Node.innerText
//
//    /// <summary>Returns the title of the HTML document.</summary>
//    /// <returns>The title of the document.</returns>
//    member __.Title= Html.title htmlDocument
//
//    /// <summary>Returns the meta description of a HTML document.</summary>
//    /// <returns>The meta description as a string option.</returns>
//    member __.MetaDescription = Html.metaDescription htmlDocument
//
//    /// <summary>Returns the meta description of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The meta description as a string option.</returns>
//    member __.MetaKeywords = Html.metaKeywords htmlDocument
//
//    /// <summary>Returns the value of the href attribute of the base tag in a HTML document.</summary>
//    /// <returns>The base URI of the document.</returns>
//    member __.BaseUri = Html.baseUri htmlDocument
//
//    /// <summary>Returns canonical URI of a HTML document if it was specified.</summary>
//    /// <returns>The canonical URI.</returns>
//    member __.CanonicalUri = Html.canonicalUri htmlDocument
//
//    /// <summary>Returns the external stylesheets associated with a HTML document.</summary>
//    /// <returns>The stylesheets URI list.</returns>    
//    member __.StyleSheets baseUri = Html.stylesheets htmlDocument baseUri
//
//    /// <summary>Returns the external script files associated with a HTML document.</summary>
//    /// <param name="baseUri">The URI that generated the HTML.</param>
//    /// <returns>The scripts URI list.</returns>
//    member __.Scripts baseUri = Html.scripts htmlDocument baseUri
//
//    /// <summary>Returns the H1 heading of a HTML document.</summary>
//    /// <returns>The H1 heading.</returns>
//    member __.H1 = Html.h1 htmlDocument
//
//    /// <summary>Returns the headings of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <returns>The headings list.</returns>
//    member __.Headings = Html.headings htmlDocument
//
//    /// <summary>Returns the links of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <param name="baseUri">The URI that generated the HTML.</param>
//    /// <returns>The links list.</returns>
//    member __.Links baseUri = Html.links htmlDocument baseUri
//
//    /// <summary>Returns the images of a HTML document.</summary>
//    /// <param name="htmlDocument">The HtmlDocument object.</param>
//    /// <param name="baseUri">The URI that generated the HTML.</param>
//    /// <returns>The images list.</returns>
//    member __.Images baseUri = Html.images htmlDocument baseUri
//



