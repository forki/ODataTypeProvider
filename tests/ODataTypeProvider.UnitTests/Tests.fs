module ParserTests
open NUnit.Framework
open Relay.Prelude
open FSharp.Data.Experimental.ODataProvider

[<Test>]
let ``should fail if metadata is unavailable`` () =
  match Fetch.downloadMetadata(null) with
  | Failure _ -> Assert.Pass()
  | _ -> Assert.Fail()

[<Test>]
let ``should fail if metadata does not parse`` () =
  match ODataParser.parseMetadata "" with
  | Failure _ -> Assert.Pass()
  | _ -> Assert.Fail()


[<Test>]
let ``should fail if metadata is not v4.0 or v4.01`` () =
  match ODataParser.parseMetadata Inputs.version3 with
  | Failure _ -> Assert.Pass()
  | _ -> Assert.Fail()

[<Test>]
let ``should succeed if metadata is v4.0 or v4.01`` () =
  match ODataParser.parseMetadata Inputs.version4, ODataParser.parseMetadata Inputs.version401 with
  | Success _, Success _ -> Assert.Pass()
  | _ -> Assert.Fail()

let (Success tripPinMetadata) = ODataParser.parseMetadata Inputs.tripPinService
[<Test>]
let ``should provide schemas as a simple type`` () =
  match ODataParser.parseSchemas tripPinMetadata with
  | [s] -> Assert.IsNotEmpty(s.Name); printfn "%A" s
  | _ -> Assert.Fail()

[<Test>]
let ``should parse EnumType as an CLI enum of int64`` () =
  let schema = tripPinMetadata.Schemas.[0]
  let p = new SchemaParser(schema)
  for s in schema.EnumTypes do
    let t = p.TypeCache.[s.Name]
    Assert.IsTrue(t.IsEnum)
    Assert.AreSame(typeof<int64>, t.GetEnumUnderlyingType())
(*
[<Test>]
let ``should parse ComplexType and place in type cache`` () =
  for s in tripPinMetadata.Schemas.[0].ComplexTypes do
    let t = ODataParser.parseComplexType s
    Assert.AreEqual(Seq.length s.Members, Seq.length <| t.GetProperties())
    Assert.IsTrue (t.GetProperties() |> Seq.forall (fun p -> not <| p.CanWrite))
    Assert.IsTrue (t.GetProperties() |> Seq.forall (fun p -> p.GetMethod.IsStatic))
*) 