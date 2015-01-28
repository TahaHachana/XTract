namespace XTract

open System.Net

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

