
#I __SOURCE_DIRECTORY__


//#r "./bin/Debug/ODataTypeProvider.dll"
open FSharp.Data.TypeProviders
type O = ODataV4<"https://graph.microsoft.com/v1.0/$metadata">
let num = Library.hello 42
printfn "%i" num
