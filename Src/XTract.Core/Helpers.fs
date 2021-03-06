﻿module XTract.Helpers

open Fizzler.Systems.HtmlAgilityPack
open HtmlAgilityPack
open Microsoft.FSharp.Reflection
open System.Collections.Generic
open System.Net
open System.Text.RegularExpressions
open Extraction

module Scrapers =

    // many false & single attr
    let falseSingle (htmlNode:HtmlNode) extractor (dictionary:Dictionary<string, obj>) =
        let value =
            match extractor.attributes.Head with
                | "text" ->
                    let text =
                        let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
                        match ``match``.Success with
                        | false -> ""
                        | true -> ``match``.Groups.[2].Value.Trim()
                    text 
                | x -> htmlNode.GetAttributeValue(x, "")
            |> WebUtility.HtmlDecode
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, value) 

    let falseSingleEmpty (dictionary:Dictionary<string, obj>) =
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, "") 

    // many false & multiple attrs
    let falseMany (htmlNode:HtmlNode) extractor (dictionary:Dictionary<string, obj>) =
        let map : Map<string, string> = Map.empty
        let value =
            extractor.attributes
            |> List.fold (fun (map:Map<string, string>) attr ->
                let value =
                    match attr with
                        | "text" ->
                            let text =
                                let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
                                match ``match``.Success with
                                | false -> ""
                                | true -> ``match``.Groups.[2].Value.Trim()
                            text 
                        | x -> htmlNode.GetAttributeValue(x, "")
                    |> WebUtility.HtmlDecode
                map.Add(attr, value)
            ) map
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, value)   

    let falseManyEmpty extractor (dictionary:Dictionary<string, obj>) =
        let map : Map<string, string> = Map.empty
        let value =
            extractor.attributes
            |> List.fold (fun (map:Map<string, string>) attr ->
                map.Add(attr, "")
            ) map
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, value)   

    // many true & single attr
    let trueSingle (nodes:HtmlNode list) extractor (dictionary:Dictionary<string, obj>) =
        let lst =
            nodes
            |> List.map (fun htmlNode ->
                match extractor.attributes.Head with
                    | "text" ->
                        let text =
                            let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
                            match ``match``.Success with
                            | false -> ""
                            | true -> ``match``.Groups.[2].Value.Trim()
                        text 
                    | x -> htmlNode.GetAttributeValue(x, "")
                |> WebUtility.HtmlDecode
            )
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, lst)

    let trueSingleEmpty (dictionary:Dictionary<string, obj>) =
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, [""])

    // many true & multiple attrs
    let trueMany (nodes:HtmlNode list) extractor (dictionary:Dictionary<string, obj>) =
        let lst =
            nodes
            |> List.map (fun htmlNode ->
                let map : Map<string, string> = Map.empty
                extractor.attributes
                |> List.fold (fun (map:Map<string, string>) attr ->
                    let value =
                        match attr with
                            | "text" ->
                                let text =
                                    let ``match`` = Regex(extractor.pattern).Match(htmlNode.InnerText)
                                    match ``match``.Success with
                                    | false -> ""
                                    | true -> ``match``.Groups.[2].Value.Trim()
                                text
                            | x -> htmlNode.GetAttributeValue(x, "")
                        |> WebUtility.HtmlDecode
                    map.Add(attr, value)
                ) map)
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, lst)

    let trueManyEmpty (dictionary:Dictionary<string, obj>) =
        let key = "key" + (string <| dictionary.Count + 1)     
        dictionary.Add(key, [""])

    type Selection = SingleNode of HtmlNode | NodesList of HtmlNode list | SelectionFailed

    let scrape selection extractor dictionary =
        match selection with
        | SingleNode htmlNode ->
            match extractor.attributes with
            | [_] -> falseSingle htmlNode extractor dictionary
            | _ -> falseMany htmlNode extractor dictionary
        | NodesList nodes ->
            match extractor.attributes with
            | [_] -> trueSingle nodes extractor dictionary
            | _ -> trueMany nodes extractor dictionary
        | SelectionFailed ->
            match extractor.many with
            | false ->
                match extractor.attributes with
                | [_] -> falseSingleEmpty dictionary
                | _ -> falseManyEmpty extractor dictionary
            | true ->
                match extractor.attributes with
                | [_] -> trueSingleEmpty dictionary
                | _ -> trueManyEmpty dictionary
            
    let selection (root:HtmlNode) extractor =
        let selection =
            match extractor.selector with
            | Css x ->
                root.QuerySelectorAll x
                |> function
                | null -> None
                | enum -> match Seq.isEmpty enum with false -> Some enum | true -> None            
            | Xpath x ->
                root.SelectNodes x
                |> function
                | null -> None
                | enums ->
                    let enumsSeq = Seq.cast<HtmlNode> enums
                    match Seq.isEmpty enumsSeq with false -> Some enumsSeq | true -> None
        let groups =
            match extractor.many with
            | false -> []
            | true ->
                match selection with
                | None -> []
                | Some x ->
//                    let groups =
                    match extractor.groupBy.Value with
                    | FirstParent -> x |> Seq.groupBy (fun n -> n.ParentNode)
                    | SecondParent -> x |> Seq.groupBy (fun n -> n.ParentNode.ParentNode)
                    | ThirdParent -> x |> Seq.groupBy (fun n -> n.ParentNode.ParentNode.ParentNode)
                    | FourthParent -> x |> Seq.groupBy (fun n -> n.ParentNode.ParentNode.ParentNode.ParentNode)
                    | FifthParent -> x |> Seq.groupBy (fun n -> n.ParentNode.ParentNode.ParentNode.ParentNode.ParentNode)
                    |> Seq.map snd
                    |> Seq.toList                        
//                    x
//                    |> Seq.groupBy (fun n -> n.ParentNode)
//                    |> Seq.map snd
//                    |> Seq.toList
        extractor, selection, groups

    let selection' (extractor, selection, groups) idx =
        // todo: rewrite match
        match extractor.many with
        | false ->
            match selection with
            | None -> extractor, SelectionFailed
            | Some nodes ->
                try
                    extractor, SingleNode (Seq.nth idx nodes)
                with _ -> extractor, SelectionFailed
        | true ->
            match selection with
            | None -> extractor, SelectionFailed
            | _ ->
                
                let nodes =
                    try
                        groups
                        |> Seq.nth idx
                        |> Seq.toList
                    with _ -> []
                match nodes with
                | [] -> extractor, SelectionFailed
                | _ -> extractor, NodesList nodes

    let scrapeSingle extractors (root:HtmlNode) =
        let dictionary = Dictionary<string, obj>()
        extractors
        |> List.map (fun x -> selection root x)
        |> List.map (fun x -> selection' x 0) //idx)
        |> List.iter (fun (extractor, selection) ->
            scrape selection extractor dictionary
        )
        dictionary
    
    let selections enums =
        let rec f acc idx =
            let lst =
                enums
                |> List.map (fun x -> selection' x idx)
            lst
            |> List.forall (fun (_, x) -> x = SelectionFailed)
            |> function
            | false -> f (acc @ [lst]) (idx + 1)
            | true -> acc
        f [] 0

    let scrapeSelections enums =
        enums
        |> selections
        |> List.fold (fun state lst ->
            let dictionary = Dictionary<string, obj>()
            lst
            |> List.iter (fun (extractor, selection) ->
                scrape selection extractor dictionary
            )
            state @ [dictionary]
        ) []

    let scrapeAll extractors (root:HtmlNode) =
        extractors
        |> List.map (fun x -> selection root x)
        |> scrapeSelections

type Source = Html | Url of string

module Utils = 

//    let urlRegex =
//        let pattern = "^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?"
//        Regex(pattern)

//    let (|Html|Url|) input =
//        match urlRegex.IsMatch(input) with
//        | false -> Html
//        | true -> Url

    let htmlRoot (html : string) =
        let document = HtmlDocument()
        document.LoadHtml html
        document.DocumentNode

    let makeRecord<'T> (url:string) (dictionary : Dictionary<string, obj>) =
        let values =
            dictionary.Values
            |> Seq.toArray
        let valuesWithUrl = Array.append values [|url|]
        FSharpValue.MakeRecord(typeof<'T>, valuesWithUrl) :?> 'T

    let scrape<'T> html extractors url =
        let root = htmlRoot html
        let record =
            Scrapers.scrapeSingle extractors root
            |> makeRecord<'T> url
        Some record

    let scrapeAll<'T> html extractors url =
        let root = htmlRoot html
        Scrapers.scrapeAll extractors root
        |> List.map (fun x ->
            let record = makeRecord<'T> url x
            record)
        |> Some

    let lstString (lst:'T list) =
        lst
        |> List.map (fun x -> x.ToString())
        |> String.concat "; "

    let fields record =
//        let source' =
//            match source with
//            | Source.Html -> [|"HTML Code"|]
//            | Source.Url x -> [|x|]
        FSharpValue.GetRecordFields record
        |> Array.map (fun x ->
            match x with
            | :? (string list) as lst -> lstString lst
            | :? Map<string, string> as map ->  lstString <| Map.toList map
            | :? (Map<string, string> list) as lst ->
                lst
                |> List.map Map.toList
                |> List.map lstString
                |> String.concat "; "
            | _ -> string x
        )
//        |> fun x -> Array.append x source'