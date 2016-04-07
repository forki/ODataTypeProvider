namespace FSharp.Data.TypeProviders.DesignTime

open FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open Vipr.Reader.OData.v4
open System
open Vipr.Core
open System.IO

[<TypeProvider>]
type ODataProviders (cfg : TypeProviderConfig) as tp = 
  inherit TypeProviderForNamespaces ()
  let ns = "FSharp.Data.TypeProviders"
  let asm = typeof<ODataProviders>.Assembly
  let odataV4Container = new ProvidedTypeDefinition(asm, ns, "ODataV4", Some typeof<obj>)
  let (|ValidODataTxt|) (args : obj array) : TextFile =
    match args with
    | [| (:? string as s) |] ->
      if Uri.IsWellFormedUriString(s, UriKind.Absolute) then
        let file = Path.GetTempFileName()
        use wc = new System.Net.WebClient()
        wc.DownloadFile(s, file)
        new TextFile("$metadata", File.ReadAllText(file))
      else failwithf "Cannot determine the location of %s" s
    | _ -> failwithf "Expected a string representing the URL or path of an OData endpoint, but got %A" args
  let createODataV4 (tyName : string) (ValidODataTxt txt) =
    let odcm = new OdcmReader()
    let model = odcm.GenerateOdcmModel([|txt|])
    let namespaces = model.Namespaces |> Seq.filter (fun n -> not <| n.Name.Equals("edm", StringComparison.OrdinalIgnoreCase))

    let odataV4 = new ProvidedTypeDefinition(asm, ns, tyName, None)
    odataV4
  do
    let path = new ProvidedStaticParameter("Path", typeof<string>)
    odataV4Container.DefineStaticParameters([path], createODataV4)

    tp.AddNamespace(ns, [odataV4Container])

[<assembly:TypeProviderAssembly>]
do ()
