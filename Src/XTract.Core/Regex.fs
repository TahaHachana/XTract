module XTract.Regex

open System.Text.RegularExpressions

/// <summary>Compiles a new instance of the
/// System.Text.RegularExpressions.Regex class for
/// the specified regular expression pattern.</summary>
/// <param name="pattern">The regular expression pattern to match.</param>
let compile pattern = Regex(pattern, RegexOptions.Compiled)

/// <summary>Searches the input string for the first
/// occurrence of the specified regular expression.</summary>
/// <param name="input">The input string.</param>
/// <param name="regex">The regular expression.</param>
let ``match`` input (regex: Regex)= regex.Match input

/// <summary>Searches the input string for all
/// occurrences of the specified regular expression.</summary>
/// <param name="input">The input string.</param>
/// <param name="regex">The regular expression.</param>
let matches input (regex: Regex) =
    regex.Matches input
    |> Seq.cast<Match>
    |> Seq.toList

/// <summary>Compiles a new regular expression for the specified pattern
/// and searches the input string for its first occurrence.</summary>
/// <param name="pattern">The regular expression pattern.</param>
/// <param name="input">The input string.</param>
let patternMatch pattern input =
    compile pattern
    |> fun regex -> ``match`` input regex 

/// <summary>Compiles a new regular expression for the specified pattern
/// and searches the input string for all its occurrences.</summary>
/// <param name="pattern">The regular expression pattern.</param>
/// <param name="input">The input string.</param>
let patternMatches pattern input =
    compile pattern
    |> fun x -> matches input x

/// <summary>Gets the value of the nth group matched
/// matched by the regular expression.</summary>
/// <param name="match">The regular expression match.</param>
/// <param name="index">The group index.</param>
let nthGroupValue (``match``: Match) (index: int) = ``match``.Groups.[index].Value

/// <summary>Indicates whether the specified regular expression pattern
/// finds a match in the supplied input string.</summary>
/// <param name="pattern">The regular expression pattern.</param>
/// <param name="input">The input string.</param>
let isMatch pattern input = Regex(pattern).IsMatch input

/// <summary>Removes all strings that match a regular expression
/// pattern within the specified input string.</summary>
/// <param name="pattern">The regular expression pattern.</param>
/// <param name="input">The input string.</param>
let remove pattern input =
    let regex = compile pattern
    regex.Replace(input, "")

module CommonPatterns =
        
    let name = "[a-zA-Z''-'\s]{1,40}"
    let ssn = "\d{3}-\d{2}-\d{4}"
    let phone = "[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}"
    let email = "(?(\")(\".+?\"@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))"
    let uri = "(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?"
    let zip = "(\d{5}-\d{4}|\d{5}|\d{9})$|^([a-zA-Z]\d[a-zA-Z] \d[a-zA-Z]\d)"
    let currency = "(-)?\d+(\.\d\d)?"

    let nameRegex = compile name
    let ssnRegex = compile ssn
    let phoneRegex = compile phone
    let emailRegex = compile email
    let uriRegex = compile uri
    let zipRegex = compile zip
    let currencyRegex = compile currency