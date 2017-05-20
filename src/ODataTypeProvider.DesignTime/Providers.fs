namespace FSharp.Data.Experimental.ODataProvider

open FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System
open System.IO
open System.Reflection

exception ParseMetadataException of Exception

module Fetch =
  open Relay.Prelude

  let (|ValidODataPath|) (args : obj array) : string =
    match args with
    | [| (:? string as s) |] ->
      if Uri.IsWellFormedUriString(s, UriKind.Absolute) then s
      elif File.Exists(s) then s
      else failwithf "Expected a valid URL or file path, but got: %s" s
    | _ -> failwithf "Expected a string representing the URL or path of an OData endpoint, but got %A" args

  let downloadMetadata url =
    try
      let url' = if File.Exists(url) then url // TODO: file scenario not supportable without also specifying service URL, remove?
                 elif url.EndsWith("/$metdata") then url
                 elif url.EndsWith("/") then url + "$metadata"
                 else url + "/$metadata"
      //let functionRequestUrl = sprintf "%s/%s(%s)" %%url %%fName %%paramsStr
      printfn "Requesting %s" url'
      use client = new Net.WebClient()
      let metadata = client.DownloadString(url')
      Success metadata
    with ex -> Failure ex

open Fetch
open ODataParser
open Relay.Prelude

[<TypeProvider>]
type ODataProviders (cfg : TypeProviderConfig) as tp = 
  inherit TypeProviderForNamespaces ()
  let ns = "FSharp.Data.TypeProviders"
  let asm = Assembly.LoadFrom cfg.RuntimeAssembly
  let odataV4Container = new ProvidedTypeDefinition(asm, ns, "ODataV4", Some typeof<obj>)
  do
    let path = new ProvidedStaticParameter("Path", typeof<string>)
    let createODataV4 (typeName : string) (ValidODataPath path) =
      let odataV4 = new ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)
      match downloadMetadata path with
      | Success m ->
        match parseMetadata m with
        | Success dataSvcs -> OData(dataSvcs, odataV4).AppendTo ()
        | Failure f -> failwith f
      | Failure f -> raise (ParseMetadataException f)
    odataV4Container.DefineStaticParameters([path], createODataV4)
    tp.AddNamespace(ns, [odataV4Container])