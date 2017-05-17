module ODataTypeProvider.Tests

open NUnit.Framework
open FSharp.Data.TypeProviders
open System.IO

type O = ODataV4<"http://services.odata.org/V4/TripPinService">

(*
[<Test>]
let ``can retrieve a simple entity`` () =
  let a = O.Functions
  Assert.IsNotEmpty a.Name
  *)
