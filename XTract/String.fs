namespace XTract

open System.Net

module String =
    
    /// <summary>Removes line breaks and white space exceeding one character from a string.</summary>
    /// <param name="str">The string to process.</param>
    /// <returns>A new string stripped from extra white space.</returns>
    let stripSpaces str =
        let regex = Regex.compile "(\n|\r)"
        let regex' = Regex.compile " {2,}"
        regex.Replace(str, " ")
        |> fun x -> regex'.Replace(x, " ")

    let decodeHtml str = WebUtility.HtmlDecode str
    
    let checkEmpty = function "" -> None | text -> stripSpaces text |> decodeHtml |> Some

