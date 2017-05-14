module ODataTypeProvider.Tests

open NUnit.Framework
open FSharp.Data.TypeProviders
type O = ODataV4<"http://services.odata.org/V4/TripPinService">


[<Test>]
let ``can retrieve a simple entity`` () =
  let a = O.Functions.GetNearestAirport(5.,6.)
  Assert.IsNotEmpty a.Name