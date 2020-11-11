module Shared
open System

/// Defines how routes are generated on server and mapped from client
let routerPaths typeName method = sprintf "/api/%s" method

type Site = {
    Id: int
    Name: string
    Latitude: float
    Longitude: float
    Timezone: string
}

type SiteData = {
    Timestamp: DateTimeOffset
    Value: decimal option
}

type SiteDataParameters = {
    SiteId: int
    FromDate: DateTimeOffset
    ToDate: DateTimeOffset
    Timezone: string
}

type Counter = { value : int }

/// A type that specifies the communication protocol between client and server
/// to learn more, read the docs at https://zaid-ajaj.github.io/Fable.Remoting/src/basics.html
type IServerApi = {
    Counter : unit -> Async<Counter>
    Sites : unit -> Async<Site list>
    SiteData : SiteDataParameters -> Async<SiteData list>
}