namespace XTract

open Fizzler.Systems.HtmlAgilityPack
open HtmlAgilityPack
open Newtonsoft.Json
open System.Collections.Generic
open System.Text.RegularExpressions

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

type Scraper(extractors) =

    member __.Scrape html =
        let doc = HtmlDocument()
        doc.LoadHtml html
        let root = doc.DocumentNode

        let enums =
            [
                for x in extractors ->
                    let selection = root.QuerySelector(x.selector)
                    match selection with
                    | null -> x, None
                    | _ -> x, Some selection
            ]
//            |> List.map (fun (p, e) ->
//                match e with
//                | None -> p, e, false
//                | Some x -> p, e, x)

        let d = Dictionary<string, string>()

        for p, e in enums do
            match e with
            | None ->
                let attrs = p.attributes
                attrs
                |> List.exists (fun x -> x = "text")
                |> function
                    | false -> ()
                    | true -> d.Add(p.name + "-text", "")
                attrs
                |> List.filter (fun x -> x <> "text")
                |> List.iter (fun x -> d.Add(p.name + "-" + x, ""))
            | Some htmlNode ->
                let attrs = p.attributes
//                let htmlNode = (Option.get e).Current
                attrs
                |> List.exists (fun x -> x = "text")
                |> function
                    | false -> ()
                    | true ->
                        let text =
                            let ``match`` = Regex(p.pattern).Match(htmlNode.InnerText)
                            match ``match``.Success with
                            | false -> ""
                            | true -> ``match``.Groups.[2].Value
                        d.Add(p.name + "-text", text)
                attrs
                |> List.filter (fun x -> x <> "text")
                |> List.iter (fun x -> d.Add(p.name + "-" + x, htmlNode.GetAttributeValue(x, "")))
        JsonConvert.SerializeObject(d, Formatting.Indented)

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

        let rec scrapeProperties (enums:(Extractor * IEnumerator<HtmlNode> option) list) (acc:Dictionary<string, string> list) =
            let lst =
                enums
                |> List.map (fun (p, e) ->
                    match e with
                    | None -> p, e, false
                    | Some x -> p, e, x.MoveNext())
            let lst' =
                lst
                |> List.filter (fun (p, e, n) -> n = true)
            let lst'' =
                lst
                |> List.map (fun (p, e, n) -> p, e)
            match lst' with
            | [] -> acc
            | _ ->
                let acc' =
                    [
                            let d = Dictionary<string, string>()
                            for (p, e, n) in lst do
                                match n with
                                | false ->
                                    let attrs = p.attributes
                                    attrs
                                    |> List.exists (fun x -> x = "text")
                                    |> function
                                        | false -> ()
                                        | true -> d.Add(p.name + "-text", "")
                                    attrs
                                    |> List.filter (fun x -> x <> "text")
                                    |> List.iter (fun x -> d.Add(p.name + "-" + x, ""))
                                | true ->
                                    let attrs = p.attributes
                                    let htmlNode = (Option.get e).Current
                                    attrs
                                    |> List.exists (fun x -> x = "text")
                                    |> function
                                        | false -> ()
                                        | true ->
                                            let text =
                                                let ``match`` = Regex(p.pattern).Match(htmlNode.InnerText)
                                                match ``match``.Success with
                                                | false -> ""
                                                | true -> ``match``.Groups.[2].Value
                                            d.Add(p.name + "-text", text)
                                    attrs
                                    |> List.filter (fun x -> x <> "text")
                                    |> List.iter (fun x -> d.Add(p.name + "-" + x, htmlNode.GetAttributeValue(x, "")))
                            yield d]
                scrapeProperties lst'' (acc @ acc')

        scrapeProperties enums []
        |> fun x -> JsonConvert.SerializeObject(x, Formatting.Indented)


