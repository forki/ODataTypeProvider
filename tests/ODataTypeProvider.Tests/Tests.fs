module ODataTypeProvider.Tests

open NUnit.Framework
open FSharp.Data.TypeProviders
type O = ODataV4<"http://services.odata.org/V4/TripPinService">


[<Test>]
let ``basics`` () =
  O.Functions.
  Assert.Inconclusive()