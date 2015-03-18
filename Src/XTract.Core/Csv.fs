module XTract.Csv

open System.IO
open CsvHelper

/// <summary>
/// Reads all the records from the supplied CSV file
/// and converts each one to the specified F# record type.
/// </summary>
/// <param name="path">The complete CSV file path.</param>
let readRecords<'T> (path: string) =
    use streamReader = new StreamReader(path)
    use csvReader = new CsvReader(streamReader)
    csvReader.GetRecords<'T>()
    |> Seq.toList

/// <summary>
/// Writes the supplied F# records to the specified
/// CSV file.
/// </summary>
/// <param name="records">The F# records collection.</param>
/// <param name="path">The complete CSV file path.</param>
let writeRecords records path =
    use streamWriter = File.CreateText(path)
    use csv = new CsvWriter(streamWriter)
    csv.Configuration.QuoteAllFields <- true
    csv.WriteRecords records
    streamWriter.Flush()