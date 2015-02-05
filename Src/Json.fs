module XTract.Json

open Newtonsoft.Json

let toRecords<'T> json =
    JsonConvert.DeserializeObject(json, typeof<seq<'T>>)
    :?> seq<'T>

let serialize value = JsonConvert.SerializeObject(value, Formatting.Indented)

/// Converts '<' and '>' unicode characters withing a JSON string. 
let decodeHtml (json:string) =
    json.Replace("""\""", "")
        .Replace("u003C", "<")
        .Replace("u003E", ">")
