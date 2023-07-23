namespace Client
open Bolero
open Bolero.Templating.Client
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open System

type Main = Template<"wwwroot/main.html">

type Page =
    
    | [<EndPoint "/">] Home
    | [<EndPoint "/">] Session of string

type Message =
    | Navigate of Page
    | Post

type Model = Page
    

module Post =
    let post() =
        async {
            return Guid.NewGuid()
        }
        

module Main =
    open Elmish    
    open Bolero.Html
    let router = Router.infer Navigate id

    let update message model =
        match message with
        | Navigate page ->
            page, Cmd.none
        | Post ->
            model, Cmd.OfAsync.perform Post.post () (string >> Session >> Navigate)

    let view model dispatch =
        Main()
            .HeaderContent(              
              Main.StandardNav().Home(router.getRoute Home).Elt()
            )
            .Input("nothing", fun thing -> Console.WriteLine(thing))
            .Post(fun _ -> dispatch Post)
            .Elt()
    
    type MyApp() =
        inherit ProgramComponent<Model, Message>()
        
        [<Inject>]
        member val JSRuntime = Unchecked.defaultof<IJSRuntime> with get, set

        override this.Program =
            Program.mkProgram (fun _ -> Home, Cmd.none) update view
            |> Program.withRouter router

