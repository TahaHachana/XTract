module XTract.Settings

type XTractSettings private () =

    static let mutable chromeDir : string option = None

    /// The directory containing ChromeDriver.exe.
    static member chromeDriverDirectory 
        with get () = Option.get chromeDir
        and set dir = chromeDir <- Some dir