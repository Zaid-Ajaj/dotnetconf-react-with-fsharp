module Components

open Feliz
open Feliz.Router
open Feliz.UseDeferred
open Shared

[<ReactComponent>]
let counter() =
    let (count, setCount) = React.useState(0)
    Html.div [
        Html.h1 count
        Html.button [
            prop.text "Increment"
            prop.onClick (fun _ -> setCount(count + 1))
        ]
    ]

// equivalent to React code
//function Counter() {
//  const [count, setCount] = useState(0);
//
//  return (
//    <div>
//      <h1>{count}</h1>
//      <button onClick={() => setCount(count + 1)}>
//        Increment
//      </button>
//    </div>
//  );
//}


[<ReactComponent>]
let counterWithInitialState (initial: int) =
    let (count, setCount) = React.useState(initial)
    Html.div [
        Html.h1 count
        Html.button [
            prop.text "Increment"
            prop.onClick (fun _ -> setCount(count + 1))
        ]
    ]

// Loading data from the backend
// asynchronous data has multiple states (initial, in-progress, failed or resolved)
// handle each case as a union
[<ReactComponent>]
let counterFromServer() =
    let data = React.useDeferred(Server.api.Counter(), [| |])
    match data with
    | Deferred.HasNotStartedYet -> Html.none
    | Deferred.InProgress -> Html.h1 "Loading"
    | Deferred.Failed error -> Html.span error.Message
    | Deferred.Resolved counter -> Html.h3 counter.value


// Simple routing solution using Feliz.Router
// transforms URL /path/to/path into segments [ "path"; "to"; "page" ]
[<ReactComponent>]
let routing() =
    let (currentUrl, updateCurrentUrl) = React.useState(Router.currentUrl())
    React.router [
        router.onUrlChanged updateCurrentUrl
        router.children [
            match currentUrl with
            | [ ] -> Html.h1 "Index"
            | [ "home" ] -> Html.h1 "Home page"
            | [ "counter" ] -> counter()
            | [ "server" ] -> counterFromServer()
            | [ "counter"; Route.Int count ] -> counterWithInitialState count
            | _ -> Html.h1 "Not found"
        ]
    ]


[<ReactComponent>]
let application() =
    let (currentUrl, updateCurrentUrl) = React.useState(Router.currentUrl())
    let (navbarOpen, updateNavbarOpen) = React.useState(false)
    let mainLayoutPadding =
        if navbarOpen
        then Constants.fullNavigationBarWidth
        else Constants.miniNavigationBarWidth

    Html.div [
        Navigation.navigationBar {
            isOpen = navbarOpen
            onOpen = fun _ -> updateNavbarOpen(not navbarOpen)
            hoverToOpen = false
            currentUrl = currentUrl
        }

        Html.main [
            prop.style [
                style.marginLeft mainLayoutPadding
                style.padding Constants.fontSize
                style.transitionDurationMilliseconds Constants.transitionSpeed
            ]

            prop.children [
                React.router [
                    router.onUrlChanged updateCurrentUrl
                    router.children [
                        match currentUrl with
                        | [ ] -> Html.h1 "Home"
                        | [ "counter" ] -> counter()
                        | [ "counter-from-server" ] -> counterFromServer()
                        | [ "maps" ] -> Maps.sites()
                        | _ -> Html.h2 "Not found :("
                    ]
                ]
            ]
        ]
    ]