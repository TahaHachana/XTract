namespace XTract

open System.Text.RegularExpressions

module String =
    
    let private compileRegex pattern = Regex(pattern, RegexOptions.Compiled)

    /// <summary>Removes line breaks and white space of 2 or more characters from a string.</summary>
    /// <param name="str">The string to process.</param>
    /// <returns>A new string stripped from extra white space.</returns>
    let stripSpaces str =
        let regex = compileRegex "(\n|\r)"
        let regex' = compileRegex " {2,}"
        regex.Replace(str, " ")
        |> fun x -> regex'.Replace(x, " ")