namespace XTract

open System
open System.IO
open System.Net
open System.Text.RegularExpressions
open System.Globalization

module String =
    
    /// <summary>Removes line breaks and white space exceeding one character from a string.</summary>
    /// <param name="str">The string to process.</param>
    /// <returns>A new string stripped from extra white space.</returns>
    let stripSpaces str =
        let regex = Regex.compile "(\n|\r)"
        let regex' = Regex.compile @"\s+"
        regex.Replace(str, " ")
        |> fun x -> regex'.Replace(x, " ").Trim()

    let stripInlineJsCss inputString = Regex.remove "(?is)(<script.*?</script>|<style.*?</style>)" inputString

    let stripTags inputString = Regex.remove "(?s)<.+?>" inputString

    let decodeHtml str = WebUtility.HtmlDecode str

    let stripHtml str =    
        stripSpaces str
        |> stripInlineJsCss
        |> stripTags
        |> decodeHtml
        |> fun x -> x.Trim()

    let removePunctuation inputString = Regex.remove "\\p{P}+" inputString

    let checkEmpty =
        function
            | ""  -> None
            | str ->
                stripSpaces str
                |> stripInlineJsCss
                |> decodeHtml
                |> fun x -> x.Trim()
                |> function
                    | ""  -> None
                    | str -> Some str

    let validPath path =
        let regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars())
        let regex = new Regex(String.Format("[{0}]", Regex.Escape(regexSearch)))
        regex.Replace(path, "-")

    /// <summary>Converts the specified string to titlecase.</summary>
    /// <param name="str">the input string.</param>
    let toTitleCase (str:string) = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower())

    /// <summary>Returns a copy of the input string with the first
    /// letter converted to uppercase.</summary>
    /// <param name="str">the input string.</param>
    let firstCharToUpper (str:string) =
        match str.Length with
        | 0 -> ""
        | 1 -> str.ToUpper()
        | _ -> 
            Char.ToUpper str.[0]
            |> fun x -> string x + str.Substring 1




