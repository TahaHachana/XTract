namespace XTract

open System.Text.RegularExpressions

module private Regex = do ()

//    /// <summary>Compiles a pattern into a Regex.</summary>
//    /// <param name="pattern">The regular expression pattern.</param>
//    /// <returns>The regex object.</returns>    
//    let compile pattern = Regex(pattern, RegexOptions.Compiled)
//
//    /// <summary>Returns the first regex match found in the specified input string.</summary>
//    /// <param name="regex">The Regex object.</param>
//    /// <param name="inputString">The input string.</param>
//    /// <returns>The first match object.</returns>    
//    let getMatch (regex : Regex) inputString = regex.Match inputString
//
//    /// <summary>Returns the list of regex matches found in the specified input string.</summary>
//    /// <param name="regex">The Regex object.</param>
//    /// <param name="inputString">The input string.</param>
//    /// <returns>The match list.</returns>
//    let matches (regex : Regex) inputString =
//        regex.Matches inputString
//        |> Seq.cast<Match>
//        |> Seq.toList
//
//    /// <summary>Returns the first regex match found in the input string using the specified pattern.</summary>
//    /// <param name="pattern">The regular expression pattern.</param>
//    /// <param name="inputString">The input string.</param>
//    /// <returns>The first match object.</returns>
//    let patternMatch pattern inputString =
//        compile pattern
//        |> fun x -> getMatch x inputString
//
//    /// <summary>Returns the regex matches found in the input string using the specified pattern.</summary>
//    /// <param name="pattern">The regular expression pattern.</param>
//    /// <param name="inputString">The input string.</param>
//    /// <returns>The match list.</returns>
//    let patternMatches pattern inputString =
//        compile pattern
//        |> fun x -> matches x inputString
//
//    /// <summary>Returns the value of a group in a regex match.</summary>
//    /// <param name="matchObj">The regex match object.</param>
//    /// <param name="group">The group index.</param>
//    /// <returns>The specified group value.</returns>    
//    let matchGroup (matchObj : Match) (group : int) = matchObj.Groups.[group].Value
//
//    /// <summary>Returns the value of a group after matching the specified regex with the input string.</summary>
//    /// <param name="regex">The regex object.</param>
//    /// <param name="group">The group index.</param>
//    /// <param name="inputString">The input string.</param>
//    /// <returns>The specified group value.</returns>    
//    let regexGroup regex group inputString =
//        getMatch regex inputString
//        |> fun x -> matchGroup x group
//
//    /// <summary>Returns the values of a group after matching the specified regex with the input string.</summary>
//    /// <param name="regex">The regex object.</param>
//    /// <param name="group">The group index.</param>
//    /// <param name="inputString">The input string.</param>
//    /// <returns>The specified group values.</returns>    
//    let regexGroups regex group inputString =
//        matches regex inputString
//        |> List.map (fun x -> matchGroup x group)
//
//    /// <summary>Returns the value of a group after compiling the specified pattern into a regex and matching it with the input string.</summary>
//    /// <param name="regex">The regex object.</param>
//    /// <param name="group">The group index.</param>
//    /// <param name="inputString">The input string.</param>
//    /// <returns>The specified group value.</returns>
//    let patternGroup pattern group inputString =
//        Regex pattern
//        |> fun x -> regexGroup x group inputString
//
//    /// <summary>Returns the values of a group after compiling the specified pattern into a regex and matching it with the input string.</summary>
//    /// <param name="regex">The regex object.</param>
//    /// <param name="group">The group index.</param>
//    /// <param name="inputString">The input string.</param>
//    /// <returns>The specified group values.</returns>
//    let patternGroups pattern group inputString =
//        Regex pattern
//        |> fun x -> regexGroups x group inputString
//
//    /// <summary>Checks whether a pattern is matched in the specified input string.</summary>
//    /// <param name="pattern">The regular expression pattern object.</param>
//    /// <param name="inputString">The input string.</param>
//    let isMatch pattern inputString = Regex(pattern).IsMatch inputString
//
//    /// <summary>Removes substrings that match a pattern from the specified input string.</summary>
//    /// <param name="pattern">The regular expression pattern object.</param>
//    /// <param name="inputString">The input string.</param>
//    let remove pattern inputString =
//        let regex = compile pattern
//        regex.Replace(inputString, "")
//
//    module CommonPatterns =
//        
//        let Name     = "[a-zA-Z''-'\s]{1,40}"
//        let SSN      = "\d{3}-\d{2}-\d{4}"
//        let Phone    = "[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}"
//        let Email    = "(?(\")(\".+?\"@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))"
//        let URI      = "(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?"
//        let ZIP      = "(\d{5}-\d{4}|\d{5}|\d{9})$|^([a-zA-Z]\d[a-zA-Z] \d[a-zA-Z]\d)"
//        let Currency = "(-)?\d+(\.\d\d)?"
//
//        let NameRegex     = compile Name
//        let SSNRegex      = compile SSN
//        let PhoneRegex    = compile Phone
//        let EmailRegex    = compile Email
//        let URIRegex      = compile URI
//        let ZIPRegex      = compile ZIP
//        let CurrencyRegex = compile Currency
