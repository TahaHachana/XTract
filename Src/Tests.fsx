#r @"C:\Users\AHMED\Documents\GitHub\XTract\Src\bin\Release\XTract.dll"
#r @"C:\Users\AHMED\Documents\GitHub\XTract\Src\packages\Deedle.1.0.6\lib\net40\Deedle.dll"
#r @"C:\Users\AHMED\Documents\GitHub\XTract\Src\packages\HtmlAgilityPack.1.4.9\lib\Net40\HtmlAgilityPack.dll"

open XTract

Settings.XTractSettings.chromeDriverDirectory <- @"C:\Users\AHMED\Desktop\chromedriver_win32"

let b = CustomSingleDynamicScraper<obj>()

b.Get "https://www.jobsbank.gov.sg/ICMSPortal/portlets/JobBankHandler/AdvancedSearch.do"

b.Quit()


ChromeOptions options = new ChromeOptions();
options.AddUserProfilePreference(string preferenceName, object preferenceValue); 

ChromeOptions options = new ChromeOptions();
options.AddUserProfilePreference("printing.print_preview_sticky_settings.appState", "{\"version\":2,\"isGcpPromoDismissed\":false,\"selectedDestinationId\":\"Save as PDF\");

firefox_profile = webdriver.FirefoxProfile()
firefox_profile.set_preference('permissions.default.stylesheet', 2)
firefox_profile.set_preference('permissions.default.image', 2)
firefox_profile.set_preference('dom.ipc.plugins.enabled.libflashplayer.so', 'false')
