module Server

open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Shared
open Npgsql.FSharp
open NodaTime


/// An implementation of the Shared IServerApi protocol.
/// Can require ASP.NET injected dependencies in the constructor and uses the Build() function to return value of `IServerApi`.
type ServerApi(logger: ILogger<ServerApi>, config: IConfiguration) =
    member this.Counter() =
        async {
            logger.LogInformation("Executing {Function}", "counter")
            do! Async.Sleep 1000
            return { value = 10 }
        }

    member this.Sites() =
        async {
            let! sites =
                config.["DATABASE"]
                |> Sql.connect
                |> Sql.query "SELECT * FROM sites"
                |> Sql.executeAsync (fun read -> {
                    Id = read.int "site_id"
                    Name = read.text "site_name"
                    Latitude = read.double "latitude"
                    Longitude = read.double "longitude"
                    Timezone = read.text "timezone"
                })

            match sites with
            | Ok records ->
                return records
            | Error error ->
                logger.LogError(error, "Error while calling sites")
                return [ ]
        }

    member this.SiteData(parameters: SiteDataParameters) =
        let clientTimezone = DateTimeZoneProviders.Tzdb.[parameters.Timezone];

        async {
            let! siteData =
                config.["DATABASE"]
                |> Sql.connect
                |> Sql.query "SELECT * FROM site_data WHERE site_id = @site_id AND timestamp >= @from AND timestamp < @to"
                |> Sql.parameters [
                    "@site_id", Sql.int parameters.SiteId
                    "@from", Sql.timestamptz parameters.FromDate
                    "@to", Sql.timestamptz parameters.ToDate
                ]
                |> Sql.executeAsync (fun read ->
                    // read as utc from database
                    let timestamp = read.datetimeOffset "timestamp"
                    // convert to the requested timezone before sending back to client
                    let withinZone = ZonedDateTime(Instant.FromDateTimeOffset timestamp, clientTimezone)
                    {
                        Timestamp = withinZone.ToDateTimeOffset()
                        Value = read.decimalOrNone "value"
                    })

            match siteData with
            | Ok records ->
                return records
            | Error error ->
                logger.LogError(error, "Error while calling site data")
                return [ ]
        }

    member this.Build() : IServerApi =
        {
            Counter = this.Counter
            Sites = this.Sites
            SiteData = this.SiteData
        }