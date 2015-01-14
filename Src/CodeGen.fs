module XTract.CodeGen

open Extraction

let recordCode extractors =
    extractors
    |> List.mapi (fun idx x ->
        let idx' = string <| idx + 1
        let attrs = x.attributes
        match attrs with
        | [_] ->
            match x.many with
            | false -> "        Field" + idx' + ": string"
            | true -> "        Field" + idx' + ": string list"
        | _ ->
            match x.many with
            | false -> "        Field" + idx' + ": Map<string, string>"
            | true -> "        Field" + idx' + ": Map<string, string> list"
    )
    |> String.concat "\n"
    |> fun x ->
        [
            "type RecordName ="
            "    {"
            x
            "        ItemUrl: string"
            "    }"
        ]
    |> String.concat "\n"