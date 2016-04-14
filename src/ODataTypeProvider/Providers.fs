namespace FSharp.Data.TypeProviders.DesignTime

open FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System
open System.IO

[<TypeProvider>]
type ODataProviders (_cfg : TypeProviderConfig) as tp = 
  inherit TypeProviderForNamespaces ()
  let ns = "FSharp.Data.TypeProviders"
  let asm = typeof<ODataProviders>.Assembly
  let odataV4Container = new ProvidedTypeDefinition(asm, ns, "ODataV4", Some typeof<obj>)
  let (|ValidODataPath|) (args : obj array) : string =
    match args with
    | [| (:? string as s) |] ->
      if Uri.IsWellFormedUriString(s, UriKind.Absolute) then s
      elif File.Exists(s) then s
      else failwithf "Expected a valid URL or file path, but got: %s" s
    | _ -> failwithf "Expected a string representing the URL or path of an OData endpoint, but got %A" args

  let createODataV4 (tyName : string) (ValidODataPath path) =
    //System.Diagnostics.Debugger.Launch()
    let odataV4 = new ProvidedTypeDefinition(asm, ns, tyName, Some typeof<obj>)
    OData(path, odataV4).AppendTo ()

  do
    let path = new ProvidedStaticParameter("Path", typeof<string>)
    odataV4Container.DefineStaticParameters([path], createODataV4)
    tp.AddNamespace(ns, [odataV4Container])

[<assembly:TypeProviderAssembly>]
do ()
