module Maps

open Feliz
open Feliz.PigeonMaps
open Feliz.Popover
open Feliz.UseDeferred
open Feliz.Recharts
open Shared
open System


type SiteChartProps = {
    Site: Site
    PreviewOpened: bool
}

[<ReactComponent>]
let siteDataChart (props: SiteChartProps)  =
    let loadSiteData = async {
        if not props.PreviewOpened
        then return [ ]
        else return! Server.api.SiteData {
            SiteId = props.Site.Id
            FromDate =  DateTimeOffset.MinValue
            ToDate = DateTimeOffset.Now
            Timezone = props.Site.Timezone
        }
    }

    let siteData = React.useDeferred(loadSiteData, [| box props.PreviewOpened |])

    match siteData with
    | Deferred.Resolved data ->
        let groupedByYear =
            data
            |> List.sortBy (fun datapoint -> datapoint.Timestamp)
            |> List.groupBy (fun datapoint -> datapoint.Timestamp.Year)
            |> List.map (fun (year, yearData) -> {
                Timestamp = DateTimeOffset(DateTime(year, 1, 1))
                Value =
                    yearData
                    |> List.sumBy (fun datapoint -> defaultArg datapoint.Value 0.0m)
                    |> Some
            })

        Recharts.barChart [
            barChart.width 220
            barChart.height 150
            barChart.data groupedByYear
            barChart.children [
                Recharts.cartesianGrid [ cartesianGrid.strokeDasharray(3, 3) ]
                Recharts.xAxis [ xAxis.dataKey (fun point -> point.Timestamp.Year) ]
                Recharts.tooltip [ ]
                Recharts.bar [
                    bar.dataKey (fun point -> Option.map float point.Value)
                    bar.fill color.lightGreen
                    bar.fillOpacity 0.8
                ]
            ]
        ]

    | _ ->
        Html.div [
            prop.style [ style.textAlign.center; style.paddingTop 10; style.margin.auto ]
            prop.children [
                Html.i [
                    prop.classes [ Icons.Fa; Icons.FaSpinner; Icons.FaSpin; Icons.Fa2X ]
                ]
            ]
        ]

type MarkerProps = {
    Site: Site
    Hovered: bool
}

[<ReactComponent>]
let markerWithPopover (marker: MarkerProps) =
    let (popoverOpen, toggleOpen) = React.useState false
    Popover.popover [
        popover.body [
            Html.div [
                prop.style [
                    style.backgroundColor.black
                    style.padding 10
                    style.borderRadius 5
                    style.color.lightGreen
                ]

                prop.children [
                    Html.span marker.Site.Name

                    siteDataChart {
                        Site = marker.Site
                        PreviewOpened =  popoverOpen
                    }
                ]
            ]
        ]
        popover.isOpen popoverOpen
        popover.enterExitTransitionDurationMs 0
        popover.disableTip
        popover.onOuterAction (fun _ -> toggleOpen(false))
        popover.children [
            Html.i [
                prop.key marker.Site.Name
                prop.className [ Icons.Fa; Icons.FaMapMarkerAlt; Icons.Fa2X ]
                prop.onClick (fun _ -> toggleOpen(not popoverOpen))
                prop.style [
                    if marker.Hovered then style.cursor.pointer
                    if popoverOpen then style.color.mediumPurple
                ]
            ]
        ]
    ]

let renderMarker (site: Site) =
    PigeonMaps.marker [
        marker.anchor(site.Latitude, site.Longitude)
        marker.offsetLeft 15
        marker.offsetTop 30
        marker.render (fun marker -> [
            markerWithPopover {
                Site = site
                Hovered = marker.hovered
            }
        ])
    ]

[<ReactComponent>]
let sites() =
    let (zoom, setZoom) = React.useState 13
    let (center, setCenter) = React.useState ((51.812565, 5.837226))
    let sitesFromServer = React.useDeferred(Server.api.Sites(), [| |])

    // update map center when data comes in
    React.useEffect((fun () ->
        match sitesFromServer with
        | Deferred.Resolved sites when not sites.IsEmpty ->
            setCenter(sites.[0].Latitude, sites.[0].Longitude)
        | _ ->
            ignore()
    ), [| box sitesFromServer |])

    Html.div [
        prop.style [ style.height (length.vh 95) ]
        prop.children [
            PigeonMaps.map [
                map.center center
                map.zoom zoom
                map.onBoundsChanged (fun args -> setZoom (int args.zoom); setCenter args.center)
                map.markers [
                    match sitesFromServer with
                    | Deferred.Resolved sites -> yield! [ for site in sites -> renderMarker site ]
                    | _ -> ()
                ]
            ]
        ]
    ]