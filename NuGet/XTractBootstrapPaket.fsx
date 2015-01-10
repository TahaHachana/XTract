#I __SOURCE_DIRECTORY__
#r """../../packages/HtmlAgilityPack/lib/Net45/HtmlAgilityPack.dll"""
#r """../../packages/Fizzler.Systems.HtmlAgilityPack/lib/net35/Fizzler.Systems.HtmlAgilityPack.dll"""
#r """../../packages/Fizzler/lib/net35/Fizzler.dll"""
#r """../../packages/CsvHelper/lib/net40-client/CsvHelper.dll"""
#r """../../packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"""
#r """../../packages/Selenium.WebDriver/lib/net40/WebDriver.dll"""
#r """../../packages/SpreadSharp/lib/Net40/SpreadSharp.dll"""
#load """../../packages/Deedle/Deedle.fsx"""
#r """../../packages/XTract/Lib/net45/XTract.dll"""

open System.IO
open XTract.Settings

// set ChromeDriver.exe executable location
XTractSettings.chromeDriverDirectory <- Path.Combine(__SOURCE_DIRECTORY__, "./tools/")