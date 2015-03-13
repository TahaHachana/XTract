module XTract.String

open System
open System.IO
open System.Net
open System.Text.RegularExpressions
open System.Globalization

/// <summary>Removes line breaks and extra white space
/// from the supplied string.</summary>
/// <param name="input">The input string.</param>
let stripSpaces input =
    let lineBreakRegex = Regex.compile "(\n|\r)"
    let whiteSpaceRegex = Regex.compile @"\s+"
    lineBreakRegex.Replace(input, " ")
    |> fun x -> whiteSpaceRegex.Replace(x, " ")
    |> fun x -> x.Trim()

/// <summary>
/// Removes inline JavaScript and CSS from the specified
/// HTML string.
/// </summary>
/// <param name="input">The HTML string.</param>
let stripInlineJsCss input = Regex.remove "(?is)(<script.*?</script>|<style.*?</style>)" input

/// <summary>
/// Removes tags from the supplied HTML string.
/// </summary>
/// <param name="input">The HTML string.</param>
let stripTags input = Regex.remove "(?s)<.+?>" input

/// <summary>
/// Decodes a string that has been HTML-encoded for
/// HTTP transmission.
/// </summary>
/// <param name="input">The HTML-encoded string.</param>
let htmlDecode input = WebUtility.HtmlDecode input

/// <summary>
/// Converts the specified input string to an HTML-encoded one.
/// </summary>
/// <param name="input">The input string.</param>
let htmlEncode input = WebUtility.HtmlEncode input

/// <summary>
/// Removes JavaScript, CSS and tags from the supplied
/// HTML string and decodes HTML-encoded characters.
/// </summary>
/// <param name="input">The HTML string.</param>
let stripHtml input =    
    stripSpaces input
    |> stripInlineJsCss
    |> stripTags
    |> htmlDecode
    |> fun x -> x.Trim()

/// <summary>
/// Removes punctuation from the supplied string.
/// </summary>
/// <param name="input">The input string.</param>
let stripPunctuation input = Regex.remove "\\p{P}+" input

/// <summary>
/// Replaces characters that are not allowed in file
/// and path names within the specified string.
/// </summary>
/// <param name="input">The input string.</param>
/// <param name="replacement">The replacement string.</param>
let validPath input (replacement: string) =
    let pattern =
        [|
            yield! Path.GetInvalidFileNameChars()
            yield! Path.GetInvalidPathChars()
        |]
        |> fun x -> new string(x)
        |> fun x -> String.Format("[{0}]", Regex.Escape(x))
    let regex = Regex.compile pattern
    regex.Replace(input, replacement)

/// <summary>
/// Converts the specified string to titlecase.
/// </summary>
/// <param name="str">the input string.</param>
let toTitleCase (str:string) = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower())

/// <summary>
/// Returns a copy of the input string with the first
/// letter converted to uppercase.
/// </summary>
/// <param name="str">the input string.</param>
let firstCharToUpper (str:string) =
    match str.Length with
    | 0 -> ""
    | 1 -> str.ToUpper()
    | _ -> 
        Char.ToUpper str.[0]
        |> fun x -> string x + str.Substring 1