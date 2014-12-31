namespace XTract

open Fizzler.Systems.HtmlAgilityPack
open HtmlAgilityPack
open Newtonsoft.Json
open System.Collections.Generic
open System.Text.RegularExpressions

/// A type for describing a property to scrape.
/// Fields:
/// name: property name,
/// selector: CSS query selector,
/// pattern: regex pattern to match against the selected element's inner HTML,
/// attributes: the HTML attributes to scrape from the selected element.
type Extractor =
    {
        name: string
        selector: string
        pattern: string
        attributes: string list
    }

    static member New name selector =
        {
            name = name
            selector = selector
            pattern = "^()(.*?)()$"
            attributes = ["text"]
        }

    static member WithPattern pattern property =
        {
            property with
                pattern = pattern
        }

    static member WithAttributes attributes property =
        {
            property with
                attributes = attributes
        }

module private Utils =
    
    let private noneAttrs attrs (dictionary:Dictionary<string, string>) extractor =
        attrs
        |> List.exists (fun x -> x = "text")
        |> function
            | false -> ()
            | true -> dictionary.Add(extractor.name + "-text", "")
        attrs
        |> List.filter (fun x -> x <> "text")
        |> List.iter (fun x -> dictionary.Add(extractor.name + "-" + x, ""))

    let private someAttrs attrs extractor (htmlNode:HtmlNode) (dictionary:Dictionary<string, string>) =
        attrs
        |> List.exists (fun x -> x = "text")
        |> function
            | false -> ()
            | true ->
                let text =
                    let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
                    match ``match``.Success with
                    | false -> ""
                    | true -> ``match``.Groups.[2].Value
                dictionary.Add(extractor.name + "-text", text)
        attrs
        |> List.filter (fun x -> x <> "text")
        |> List.iter (fun x -> dictionary.Add(extractor.name + "-" + x, htmlNode.GetAttributeValue(x, "")))

    let extract extractor htmlNode dictionary =
        let attrs = extractor.attributes
        match htmlNode with
        | None -> noneAttrs attrs dictionary extractor
        | Some htmlNode -> someAttrs attrs extractor htmlNode dictionary

    let rec extractAll (acc:Dictionary<string, string> list) (enums:(Extractor * IEnumerator<HtmlNode> option) list) =
        let enums' =
            enums
            |> List.map (fun (extrator, enumerator) ->
                match enumerator with
                | None -> extrator, enumerator, None
                | Some x ->
                    let htmlNode =
                        match x.MoveNext() with
                        | false -> None
                        | true -> Some x.Current
                    extrator, enumerator, htmlNode)
        match enums' |> List.exists (fun (_, _, htmlNode) ->
            htmlNode.IsSome) with
        | false -> acc
        | true ->
            let acc' =
                [
                    let dictionary = Dictionary<string, string>()
                    for (extractor, _, htmlNode) in enums' do
                        extract extractor htmlNode dictionary
                    yield dictionary
                ]
            enums'
            |> List.map (fun (extractor, enumerator, _) -> extractor, enumerator)
            |> extractAll (acc @ acc')

type Scraper(extractors) =

    member __.Scrape html =
        let doc = HtmlDocument()
        doc.LoadHtml html
        let root = doc.DocumentNode
        let dictionary = Dictionary<string, string>()
        extractors
        |> List.map (fun x ->
            let selection = root.QuerySelector(x.selector)
            match selection with
            | null -> x, None
            | _ -> x, Some selection        
        )
        |> List.iter (fun (x, htmlNode) -> Utils.extract x htmlNode dictionary)
        JsonConvert.SerializeObject(dictionary, Formatting.Indented)

    member __.ScrapeAll html =
        let doc = HtmlDocument()
        doc.LoadHtml html
        let root = doc.DocumentNode
        let enums =
            [
                for x in extractors ->
                    let selection = root.QuerySelectorAll(x.selector)
                    match selection with
                    | null -> x, None
                    | _ -> x, Some <| selection.GetEnumerator()
            ]
        Utils.extractAll [] enums
        |> fun x -> JsonConvert.SerializeObject(x, Formatting.Indented)


