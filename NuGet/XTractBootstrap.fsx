#I __SOURCE_DIRECTORY__
#r """../../packages/HtmlAgilityPack.1.4.9/lib/Net45/HtmlAgilityPack.dll"""
#r """../../packages/Fizzler.Systems.HtmlAgilityPack.1.0.0/lib/net35/Fizzler.Systems.HtmlAgilityPack.dll"""
#r """../../packages/Fizzler.1.0.0/lib/net35/Fizzler.dll"""
#r """../../packages/CsvHelper.2.10.0/lib/net40-client/CsvHelper.dll"""
#r """../../packages/Newtonsoft.Json.6.0.7/lib/net45/Newtonsoft.Json.dll"""
#r """../../packages/Selenium.WebDriver.2.44.0/lib/net40/WebDriver.dll"""
#r """../../packages/SpreadSharp.0.2.79/lib/Net40/SpreadSharp.dll"""
#load """../../packages/Deedle.1.0.6/Deedle.fsx"""
#r """../../packages/XTract.0.3.3/Lib/net45/XTract.dll"""

open System.IO
open XTract.Settings

// set ChromeDriver.exe executable location
XTractSettings.chromeDriverDirectory <- Path.Combine(__SOURCE_DIRECTORY__, "./tools/")
XTractSettings.phantomDriverDirectory <- Path.Combine(__SOURCE_DIRECTORY__, "./tools/")