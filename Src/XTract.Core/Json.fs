module XTract.Json

open Newtonsoft.Json
open System.IO

/// <summary>
/// Deserializes the supplied JSON to a sequence
/// of the specified F# record type.
/// </summary>
/// <param name="json">The JSON string.</param>
let deserializeRecords<'T> json =
    JsonConvert.DeserializeObject(json, typeof<seq<'T>>)
    :?> seq<'T>

/// <summary>
/// Serializes the supplied object to a JSON
/// string using indented formatting.
/// </summary>
/// <param name="value">The object to serialize.</param>
let serialize value = JsonConvert.SerializeObject(value, Formatting.Indented)



/// <summary>
/// Converts the '&lt;' and '&gt;' unicode characters withing a JSON string.
/// </summary>
/// <param name="json">The JSON string.</param>
let decodeHtml (json:string) =
    json.Replace("""\""", "")
        .Replace("u003C", "<")
        .Replace("u003E", ">")




/// <summary>Serializes the specified object to a JSON string and
/// writes it to the supplied file.</summary>
/// <param name="path">The file to write to.</param>
/// <param name="value">The object to serialize.</param>
let serializeWrite path value =
    serialize value
    |> fun json -> File.WriteAllText(path, json)

/// <summary>Reads a JSON string from the specified file and
/// deserializes it to the supplied F# record type.</summary>
/// <param name="path">The file to read from.</param>
let readRecords<'T> path =
    File.ReadAllText path
    |> deserializeRecords<'T>

