namespace FSharp.Data.Experimental.ODataProvider

open System
open FSharp.Data
open ProviderImplementation.ProvidedTypes
open FSharp.Quotations
open Relay.Prelude
open System.Xml.Linq
type Edm = XmlProvider<Schema=ODataSchemas.Edm>
type Edmx = XmlProvider<Schema=ODataSchemas.Edmx>

module ODataParser =
  let inline tryFind (schemaNamespace : string) (elements : ^e array) (name : string) =
    let isNameMatch (e : ^e) =
      let localName = (^e : (member Name: string) e)
      let nominalTypeName = sprintf "%s.%s" schemaNamespace localName
      nominalTypeName = name
    elements |> Array.tryFind isNameMatch
  let parseMetadata metadata =
    try
      match Edmx.Parse(metadata).Edmx with
      | Some metadata when metadata.Version = 4.0m || metadata.Version = 4.01m ->
        Success metadata.DataServices
      | Some _ -> Failure "Only support version 4.0"
      | _ -> Failure "Metadata invalid"
    with ex -> Failure (ex.Message)
  let parseSchemas (dataSvcs : Edmx.DataServices) =
    dataSvcs.Schemas
    |> Array.map (fun s -> new ProvidedTypeDefinition(s.Namespace, Some typeof<obj>))

  

type OData (dataSvcs : Edmx.DataServices, container : ProvidedTypeDefinition) =
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
  let schema = dataSvcs.Schemas.[0]
  let rec mapEntityType typeName : ProvidedTypeDefinition option =
    ODataParser.tryFind schema.Namespace schema.EntityTypes typeName
    |> Option.map (fun entityTy ->
                   let ty = ProvidedTypeDefinition(entityTy.Name, Some typeof<obj>)
                   entityTy.Properties
                   |> Array.iter (fun p -> ty.AddMember(ProvidedProperty(p.Name, mapType p.Type p.Nullable, GetterCode = fun _ -> <@@ obj() @@>)))
                   ty.AddMember(ProvidedConstructor([]))
                   ty)
  and mapComplexType typeName : ProvidedTypeDefinition option =
    ODataParser.tryFind schema.Namespace schema.ComplexTypes typeName
    |> Option.map (fun complexTy -> ProvidedTypeDefinition(complexTy.Name, None))
  and mapEnumType typeName : ProvidedTypeDefinition option =
    ODataParser.tryFind schema.Namespace schema.EnumTypes typeName
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
  let mkFunction (fi : Edmx.FunctionImport) : ProvidedMethod option =
    ODataParser.tryFind schema.Namespace schema.Functions fi.Function
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
                        //let url   = Expr.Value url
                        //let fName = Expr.Value m.Name
                        <@@ obj() @@>) // QUESTION: how to return Airport here (erased)?
                   m)

  // kinds: EntitySet, Singleton, FunctionImport
  member x.AppendTo () =
    let functionsTy = ProvidedTypeDefinition("Functions", None)
    (*
    let entityContainer = schema.EntityContainers.[0] // exactly one of these by spec

    let functions = entityContainer.XElement.Elements(edmName "FunctionImport")
                    |> Seq.map Edmx.FunctionImport
                    |> Seq.choose mkFunction
                    |> Seq.toList
    functionsTy.AddMembers(functions)
    *)
    container.AddMember(functionsTy)
    container