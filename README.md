# Pre-alpha and useless ATM

## TODO

- [ ] function composition
- [ ] bound functions
- [ ] actions
- [ ] queries

## New Plan

- Create an [ASP.NET Web API that exposes an OData v4 service](https://blogs.msdn.microsoft.com/webdev/2014/03/13/getting-started-with-asp-net-web-api-2-2-for-odata-v4-0/) for testing
- Create a checklist of OData v4 features
- Create a query approach similar to the Azure Storage Provider for Tables
- Erase all types and queries to REST calls

http://stackoverflow.com/questions/12339205/pattern-matching-xml-in-f

## Old Plan

I propose to create an OData v4 type provider using [Vipr](https://github.com/Microsoft/Vipr)

I'm going to leverage the `ODataReader.v4.IOdcmReader`implementation for converting
OData v4 metadata into an OdcmModel, then mapping the OdcmModel to provided types.


The recently announced [MS Graph SDK](https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator) uses it.
I'm hoping to create an MS Graph Type Provider as a follow-on to this work.

## Background

FSharp.Data.TypeProviders was [extracted to its own project](https://github.com/fsprojects/FSharp.Data.TypeProviders/)
pursuant to [this issue](https://github.com/Microsoft/visualfsharp/issues/441)

However it still just uses [DataSvcUtil.exe](https://msdn.microsoft.com/en-us/library/dd756369.aspx)
to [generate C# code](https://github.com/fsprojects/FSharp.Data.TypeProviders/blob/c44490c92c3ef2b856952328ee720276b86c2d86/src/FSharp.Data.TypeProviders/TypeProvidersImpl.fs#L575)

Unfortunately, that utility doesn't support OData V4. Jamie had problems with unsupported OData versions [once upon a time](https://jamessdixon.wordpress.com/2014/01/07/setting-up-an-odata-service-on-webapi2-to-be-used-by-f-type-providers/)


