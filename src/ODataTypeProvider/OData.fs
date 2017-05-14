namespace FSharp.Data.TypeProviders.DesignTime

open System
open FSharp.Data
open ProviderImplementation.ProvidedTypes
open System.Xml.Linq
open FSharp.Quotations
open Relay.Prelude

type Metadata = XmlProvider<"Metadata.xml">

module ODataParser =
  let inline tryFind (schemaNamespace : string) (elements : ^e array) (name : string) =
    let isNameMatch (e : ^e) =
      let localName = (^e : (member Name: string) e)
      let nominalTypeName = sprintf "%s.%s" schemaNamespace localName
      nominalTypeName = name
    elements |> Array.tryFind isNameMatch
    
type OData (url : string, container : ProvidedTypeDefinition) =
  let url' = if IO.File.Exists(url) then url // TODO: file scenario not supportable without also specifying service URL, remove?
             elif url.EndsWith("/$metdata") then url
             elif url.EndsWith("/") then url + "$metadata"
             else url + "/$metadata"
  let dataSvcs = Metadata.Load(url').DataServices

  let entity f =
    let d = Collections.Generic.Dictionary<string,ProvidedTypeDefinition option>(HashIdentity.Structural)
    fun n ->
      if not <| d.ContainsKey n then
        let e = f n
        d.[n] <- e
        if e.IsSome then
          container.AddMember(e.Value)
        e
      else d.[n]

  let rec mapEntityType typeName : ProvidedTypeDefinition option =
    ODataParser.tryFind dataSvcs.Schema.Namespace dataSvcs.Schema.EntityTypes typeName
    |> Option.map (fun entityTy ->
                   let ty = ProvidedTypeDefinition(entityTy.Name, Some typeof<obj>)
                   entityTy.Properties
                   |> Array.iter (fun p -> ty.AddMember(ProvidedProperty(p.Name, mapType p.Type p.Nullable, GetterCode = fun _ -> <@@ obj() @@>)))
                   ty.AddMember(ProvidedConstructor([]))
                   ty)
  and mapComplexType typeName : ProvidedTypeDefinition option =
    ODataParser.tryFind dataSvcs.Schema.Namespace dataSvcs.Schema.ComplexTypes typeName
    |> Option.map (fun complexTy -> ProvidedTypeDefinition(complexTy.Name, None))
  and mapEnumType typeName : ProvidedTypeDefinition option =
    ODataParser.tryFind dataSvcs.Schema.Namespace dataSvcs.Schema.EnumTypes typeName
    |> Option.map (fun enumTy -> ProvidedTypeDefinition(enumTy.Name, None))
  and mapSvcDefinedType typeName : Type option =
    choice {
        return entity mapEntityType typeName
        return mapComplexType typeName
        return mapEnumType typeName
    } |> Option.map (fun ty -> upcast ty)
  and mapType typeName (Default true isNullable) =
    let mkTy t =
      if isNullable then
        typedefof<option<_>>.MakeGenericType([|t|])
      else t
    match typeName with
    | "Edm.Binary"   -> typeof<byte[]>
    | "Edm.Boolean"  -> typeof<bool>
    | "Edm.DateTime" -> typeof<DateTime>
    | "Edm.Double"   -> typeof<double>
    | "Edm.Guid"     -> typeof<Guid>
    | "Edm.Int32"    -> typeof<int>
    | "Edm.Int64"    -> typeof<Int64>
    | "Edm.String"   -> typeof<string>
    | s              -> defaultArg (mapSvcDefinedType s) typeof<obj>
    |> mkTy
 
  let edmName name = XName.Get(name, "http://docs.oasis-open.org/odata/ns/edm")
  let mkFunction (fi : Metadata.FunctionImport) : ProvidedMethod option =
    ODataParser.tryFind dataSvcs.Schema.Namespace dataSvcs.Schema.Functions fi.Function
    |> Option.map (fun f ->
                   let ps = f.Parameters
                            |> Array.map (fun p -> ProvidedParameter(p.Name, mapType p.Type p.Nullable))
                            |> Array.toList
                   let m = ProvidedMethod(f.Name, ps, mapType f.ReturnType.Type f.ReturnType.Nullable)
                   m.IsStaticMethod <- true
                   m.InvokeCode <-
                     (fun args ->
                        let paramsStr =
                          let hd::tl =
                            ps |> List.mapi (fun idx p ->
                                             let v = Expr.Coerce(args.[idx], typeof<obj>)
                                             let paramName = Expr.Value p.Name
                                             <@@ sprintf "%s = %A" %%paramName %%v @@>)
                          tl |> List.fold (fun s p -> <@@ sprintf "%s, %s" %%s %%p @@>) hd
                        let url   = Expr.Value url
                        let fName = Expr.Value m.Name
                        <@@ 
                           let requestUrl = sprintf "%s/%s(%s)" %%url %%fName %%paramsStr
                           printfn "Requesting %s" requestUrl
                           use client = new Net.WebClient()
                           let response = client.DownloadString(requestUrl)
                           printfn "%s" response
                           obj() @@>) // QUESTION: how to return Airport here (erased)?
                   m)

  // kinds: EntitySet, Singleton, FunctionImport
  member x.AppendTo () =
    let functionsTy = ProvidedTypeDefinition("Functions", None)
    let entityContainer = dataSvcs.Schema.EntityContainer // exactly one of these by spec

    let functions = entityContainer.XElement.Elements(edmName "FunctionImport")
                    |> Seq.map Metadata.FunctionImport
                    |> Seq.choose mkFunction
                    |> Seq.toList
    functionsTy.AddMembers(functions)
    container.AddMember(functionsTy)
    container