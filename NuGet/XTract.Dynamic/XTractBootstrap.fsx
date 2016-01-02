#I __SOURCE_DIRECTORY__
#r """../../packages/HtmlAgilityPack.1.4.9/lib/Net40/HtmlAgilityPack.dll"""
#r """../../packages/Fizzler.Systems.HtmlAgilityPack.1.0.0/lib/net35/Fizzler.Systems.HtmlAgilityPack.dll"""
#r """../../packages/Fizzler.1.0.0/lib/net35/Fizzler.dll"""
#r """../../packages/CsvHelper.2.10.0/lib/net40-client/CsvHelper.dll"""
#r """../../packages/Newtonsoft.Json.6.0.8/lib/net40/Newtonsoft.Json.dll"""
#r """../../packages/Selenium.WebDriver.2.44.0/lib/net40/WebDriver.dll"""
#r """../../packages/SpreadSharp.0.3.1/lib/Net40/Microsoft.Office.Interop.Excel.dll"""
#r """../../packages/SpreadSharp.0.3.1/lib/Net40/SpreadSharp.dll"""
#r """../../packages/XTract.Dynamic.0.4.0-Beta3/Lib/net40/XTract.Core.dll"""
#r """../../packages/XTract.Dynamic.0.4.0-Beta3/Lib/net40/XTract.Dynamic.dll"""

open System.IO
open XTract.Dynamic.Settings

// set ChromeDriver.exe executable location
XTractSettings.chromeDriverDirectory <- Path.Combine(__SOURCE_DIRECTORY__, "./tools/")