namespace XTract

#if INTERACTIVE
#r ""
#endif

open HtmlAgilityPack

module private Utilities =

    /// <summary>Loads the specified string as a HtmlDocument.</summary>
    /// <param name="htmlString">The string representing the HTML.</param>
    /// <returns>The HtmlDocument object.</returns>
    let load htmlString =
        let htmlDocument = HtmlDocument()
        htmlDocument.LoadHtml htmlString
        htmlDocument

type Document(htmlString) =

    let htmlDocument = Utilities.load htmlString

    /// <summary>Returns the inner text of a HTML node.</summary>
    /// <param name="xpath">The XPath expression of the HTML node.</param>
    /// <returns>The inner text of the HTML node.</returns>
    member __.NodeText xpath =
        htmlDocument.DocumentNode.SelectSingleNode xpath
        |> fun x -> x.InnerText.Trim()