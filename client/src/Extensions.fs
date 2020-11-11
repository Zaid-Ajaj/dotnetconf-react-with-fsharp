﻿[<AutoOpen>]
module Extensions

open Elmish
open System
open Fable.Core

open Zanaptak.TypedCssClasses

type Icons = CssClasses<"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.1/css/all.min.css", Naming.PascalCase>

let isDevelopment =
    #if DEBUG
    true
    #else
    false
    #endif

module Log =
    /// Logs error to the console during development
    let developmentError (error: exn) =
        if isDevelopment
        then Browser.Dom.console.error(error)

module Cmd =
    /// Converts an asynchronous operation that returns a message into into a command of that message.
    /// Logs unexpected errors to the console while in development mode.
    let fromAsync (operation: Async<'msg>) : Cmd<'msg> =
        let delayedCmd (dispatch: 'msg -> unit) : unit =
            let delayedDispatch = async {
                match! Async.Catch operation with
                | Choice1Of2 msg -> dispatch msg
                | Choice2Of2 error -> Log.developmentError error
            }

            Async.StartImmediate delayedDispatch

        Cmd.ofSub delayedCmd

[<RequireQualifiedAccess>]
module StaticFile =

    open Fable.Core.JsInterop

    /// Function that imports a static file by it's relative path. Ignores the file when compiled for mocha tests.
    let inline import (path: string) : string =
        #if !MOCHA_TESTS
        importDefault<string> path
        #else
        path
        #endif

[<RequireQualifiedAccess>]
module Config =
    open System
    open Fable.Core

    /// Returns the value of a configured variable using its key.
    /// Retursn empty string when the value does not exist
    [<Emit("process.env[$0] ? process.env[$0] : ''")>]
    let variable (key: string) : string = jsNative

    /// Tries to find the value of the configured variable if it is defined or returns a given default value otherwise.
    let variableOrDefault (key: string) (defaultValue: string) =
        let foundValue = variable key
        if String.IsNullOrWhiteSpace foundValue
        then defaultValue
        else foundValue