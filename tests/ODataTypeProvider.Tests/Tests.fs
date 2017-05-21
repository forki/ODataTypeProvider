module ODataTypeProvider.Tests

open NUnit.Framework
open FSharp.Data.TypeProviders

type O = ODataV4<"http://services.odata.org/V4/TripPinService/">
type A = O.``Microsoft.OData.SampleService.Models.TripPin``
[<Test>]
let ``can resolve enum`` () =
  Assert.AreEqual(0, A.PersonGender.Male)
  Assert.AreEqual(1, A.PersonGender.Female)
  Assert.AreEqual(2, A.PersonGender.Unknown)