module Navigation

open Feliz
open Feliz.Router

type NavigationItemProps = {
    title: string
    icon: string
    titleVisible: bool
    url : string
    active : bool
}

type NavigationBarOpenerProps = {
    toggleOpened : unit -> unit
    isOpen : bool
}

[<ReactComponent>]
let navigationLink (props: NavigationItemProps) =
    let (hovered, setHovered) = React.useState(false)

    Html.li [
        prop.onMouseEnter (fun _ -> setHovered(true))
        prop.onMouseLeave (fun _ -> setHovered(false))
        prop.style [ style.width (length.percent 100) ]

        prop.children [
            Html.div [
                prop.style [
                    style.borderLeftWidth 3
                    style.borderLeftStyle borderStyle.solid
                    style.transitionDurationMilliseconds Constants.transitionSpeed
                    if props.active then style.borderLeftColor Constants.navbarTextColor
                    else if hovered then style.borderLeftColor Constants.navbarHoverBackgroundColor
                    else style.borderLeftColor Constants.navbarBackgroundColor
                ]

                prop.children [
                    Html.a [
                        prop.href props.url
                        prop.style [
                            style.display.flex
                            style.alignItems.center
                            style.height (5 * Constants.fontSize)
                            style.textDecoration.none
                            style.transitionDurationMilliseconds Constants.transitionSpeed
                            style.color.white

                            if hovered || props.active then
                                style.color Constants.navbarTextColor
                                style.backgroundColor Constants.navbarHoverBackgroundColor
                                style.filter.grayscale 0
                                style.opacity 1.0
                            else
                                style.filter.grayscale 100
                                style.opacity 0.7
                        ]

                        prop.children [
                            Html.i [
                                prop.style [ style.margin(0, 24) ]
                                prop.className [ sprintf "fa %s fa-2x" props.icon ]
                            ]

                            Html.span [
                                prop.style [ if not props.titleVisible then style.display.none ]
                                prop.text props.title
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

[<ReactComponent>]
let navigationBarOpener (props: NavigationBarOpenerProps) =
    let (hovered, setHovered) = React.useState(false)

    Html.li [
        prop.onMouseEnter (fun _ -> setHovered(true))
        prop.onMouseLeave (fun _ -> setHovered(false))
        prop.style [ style.width (length.percent 100); style.marginTop length.auto ]
        prop.onClick (fun _ -> props.toggleOpened())
        prop.children [
            Html.div [
                prop.style [
                    style.borderLeftWidth 3
                    style.borderLeftStyle borderStyle.solid
                    style.transitionDurationMilliseconds Constants.transitionSpeed
                    if hovered then style.borderLeftColor Constants.navbarHoverBackgroundColor
                    else style.borderLeftColor Constants.navbarBackgroundColor
                ]

                prop.children [
                    Html.a [
                        prop.style [
                            style.display.flex
                            style.alignItems.center
                            style.height Constants.miniNavigationBarWidth
                            style.textDecoration.none
                            style.transitionDurationMilliseconds Constants.transitionSpeed
                            style.color.white

                            if hovered then
                                style.backgroundColor Constants.navbarHoverBackgroundColor
                                style.color Constants.navbarTextColor
                                style.filter.grayscale 0
                                style.opacity 1.0
                            else
                                style.filter.grayscale 100
                                style.opacity 0.7
                        ]

                        prop.children [
                            Html.i [
                                prop.style [ style.margin(0, length.auto) ]
                                prop.className [ if props.isOpen then "fa fa-chevron-left fa-2x" else "fa fa-chevron-right fa-2x"]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]


type NavigationBarProps = {
    isOpen : bool
    hoverToOpen : bool
    onOpen : bool -> unit
    currentUrl : string list
}

[<ReactComponent>]
let navigationBar (props: NavigationBarProps) =
    let (hovered, setHovered) = React.useState(false)
    let navigationBarWidth = if hovered || props.isOpen then Constants.fullNavigationBarWidth else Constants.miniNavigationBarWidth
    Html.nav [
        prop.style [
            style.backgroundColor Constants.navbarBackgroundColor
            style.width navigationBarWidth
            style.height (length.vh(100))
            style.position.fixedRelativeToWindow
            style.transitionDurationMilliseconds Constants.transitionSpeed
            style.transitionTimingFunction.ease
        ]

        prop.onMouseEnter (fun _ -> if props.hoverToOpen then setHovered(true))
        prop.onMouseLeave (fun _ -> if props.hoverToOpen then setHovered(false))

        prop.children [
            Html.ul [
                prop.style [
                    style.padding 0
                    style.margin 0
                    style.display.flex
                    style.listStyleType.none
                    style.flexDirection.column
                    style.alignItems.center
                    style.height (length.vh(100))
                ]

                prop.children [
                    navigationLink {
                        title = "Home"
                        icon = Icons.FaHome
                        url = Router.format "/"
                        active = props.currentUrl = [ ]
                        titleVisible = hovered || props.isOpen
                    }

                    navigationLink {
                        title = "Counter"
                        icon = Icons.FaPlus
                        url = Router.format "counter"
                        active = props.currentUrl = [ "counter" ]
                        titleVisible = hovered || props.isOpen
                    }

                    navigationLink {
                        title = "Sites"
                        icon = Icons.FaMapMarkerAlt
                        url = Router.format "maps"
                        active = props.currentUrl = [ "maps" ]
                        titleVisible = hovered || props.isOpen
                    }

                    navigationBarOpener {
                        isOpen = props.isOpen
                        toggleOpened = fun _ -> props.onOpen(not props.isOpen)
                    }
                ]
            ]
        ]
    ]