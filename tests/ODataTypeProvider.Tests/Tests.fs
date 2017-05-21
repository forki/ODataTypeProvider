module ODataTypeProvider.Tests

open System
open NUnit.Framework
open FSharp.Data.TypeProviders

type O = ODataV4<"http://services.odata.org/V4/TripPinService/">
type A = O.``Microsoft.OData.SampleService.Models.TripPin``
[<Test>]
let ``can resolve enum`` () =
  Assert.AreEqual(0, int A.PersonGender.Male)
  Assert.AreEqual(1, int A.PersonGender.Female)
  Assert.AreEqual(2, int A.PersonGender.Unknown)
  Assert.IsTrue(A.PersonGender.Female = (Enum.Parse(typeof<A.PersonGender>, "Female") |> unbox))
  // Note this doesn't work because FSharp.Core.Operators.enum
  // because, I believe, doesn't support non-int32 enum types
  // Assert.AreEqual(enum 2, A.PersonGener.Unknown)