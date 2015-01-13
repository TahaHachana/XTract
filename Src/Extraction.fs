[<AutoOpen>]
module XTract.Extraction

type Selector = Css of string | Xpath of string

type GroupBy = FirstParent | SecondParent | ThirdParent | FourthParent | FifthParent

//type 
/// A type for describing a property to scrape.
/// Fields:
/// selector: CSS selector,
/// pattern: regex pattern to match against the selected element's inner text,
/// the default pattern is "^()(.*?)()$" and the value of the second group is retained,
/// attributes: the HTML attributes to scrape from the selected element ("text" is the default).
type Extractor = 
    {
        selector: Selector
        pattern: string
        attributes: string list
        many: bool
        groupBy: GroupBy option
    }
    
    static member New selector = 
        {
            selector = selector
            pattern = "^()(.*?)()$"
            attributes = [ "text" ]
            many = false
            groupBy = None
        }
    
    static member WithPattern pattern property =
        {
            property with
                pattern = pattern
        }

    static member WithAttributes attributes property =
        {
            property with
                attributes = attributes
        }

    static member WithMany many groupBy property =
        {
            property with
                many = many
                groupBy = Some groupBy
        }