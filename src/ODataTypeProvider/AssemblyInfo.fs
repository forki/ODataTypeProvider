namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("ODataTypeProvider")>]
[<assembly: AssemblyProductAttribute("ODataTypeProvider")>]
[<assembly: AssemblyDescriptionAttribute("an F# type provider for OData resources")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
    let [<Literal>] InformationalVersion = "1.0"
