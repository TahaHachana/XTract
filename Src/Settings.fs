module XTract.Settings

type XTractSettings private () =

    static let mutable chromeDir : string option = None
    static let mutable phantomDir : string option = None

    /// The directory containing ChromeDriver.exe.
    static member chromeDriverDirectory 
        with get () = Option.get chromeDir
        and set dir = chromeDir <- Some dir

    /// The directory containing PhantomJS.exe.
    static member phantomDriverDirectory 
        with get () = Option.get phantomDir
        and set dir = phantomDir <- Some dir