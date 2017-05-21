﻿module ParserTests
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

(*
  This would ideally parse as an actual CLI enum, but
  https://github.com/fsharp/FSharp.Data/issues/37
  Maybe this would be possible via
  https://msdn.microsoft.com/en-us/library/system.reflection.emit.modulebuilder.defineenum.aspx
*)
open System.Reflection
[<Test>]
let ``should parse EnumType as static read-only property`` () =
  for s in tripPinMetadata.Schemas.[0].EnumTypes do
    let t = ODataParser.parseEnum s
    Assert.AreEqual(Seq.length s.Members, Seq.length <| t.GetProperties())
    Assert.IsTrue (t.GetProperties() |> Seq.forall (fun p -> not <| p.CanWrite))
    Assert.IsTrue (t.GetProperties() |> Seq.forall (fun p -> p.GetMethod.IsStatic))