[<AutoOpen>]
module XTract.Csv

open System.IO
open CsvHelper

let readRecords<'T> (path:string) =
    use streamReader = new StreamReader(path)
    use csvReader = new CsvReader(streamReader)
    csvReader.GetRecords<'T>()
    |> Seq.toList

let writeRecords path records =
    use streamWriter = File.CreateText(path)
    use csv = new CsvWriter(streamWriter)
    csv.Configuration.QuoteAllFields <- true
    csv.WriteRecords records
    streamWriter.Flush()


