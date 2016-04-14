
#I __SOURCE_DIRECTORY__
//#r "./bin/Debug/ODataTypeProvider.dll"
open FSharp.Data.TypeProviders
type O = ODataV4<"http://services.odata.org/V4/TripPinService">
O.Functions


let num = Library.hello 42
printfn "%i" num



(* 
type Orders = SqlTP<"myconn">.Tables.Orders // DataConnection :: IQueryable<Order*>
let o = new Orders(1)
printf "%A" o.OrderId // o.GetColumn("OrderId")

type public Orders = SqlEntity                     // Order* ~~ SqlEntity (DynamicObject)
*)
