module ODataTypeProvider.Tests

open NUnit.Framework
open FSharp.Data.TypeProviders
type O = ODataV4<"http://services.odata.org/V4/TripPinService">


[<Test>]
let ``hello world`` () =
  O.Functions.GetNearestAirport(5.,6.) |> ignore
  Assert.Inconclusive()