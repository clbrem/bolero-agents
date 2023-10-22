namespace Client


open System.Text.Json.Serialization
open Bolero
open Bolero.Remoting.Client
open Bolero.Templating.Client
open System
open Agents

type Main = Template<"wwwroot/main.html">
type Wrapper = Template<"wwwroot/wrapper.html">

[<JsonFSharpConverter>]
type Page =

    | [<EndPoint "/">] Home
    | [<EndPoint "/session">] Session of string

[<JsonFSharpConverter>]
type Message =
    | Navigate of Page
    | Redirect of Page
    | Post
    | SetInput of string
    | Fetch

[<CLIMutable>]
type Model = { page: Page; input: string }

module Main =
    open Elmish
    let router = Router.infer Navigate (fun model -> model.page)
        
    let debugRouter = Router.infer (Navigate >> Debugger.Message) (fun (model: Debugger.Model<Model>) -> model.model.page)
    let init = { page = Home; input = "" }    

    let update debounce js message model =
        
        ////////////////////////////////////////////
        ///
        /// ATTACH A "LOGGER"  
        ///
        ///////////////////////////////////////////

        let log (newModel, cmd) =
            match model.page with
            // Only log if in an active session
            | Session session ->
                newModel,
                Cmd.batch
                    [ cmd
                      Cmd.OfJS.attempt
                          js
                          "Database.log"
                          [| {| session = session
                                message = message |} |]
                          (fun _ -> Redirect Home) ]
            | _ -> newModel, cmd

        match message with
        | Navigate page -> { model with page = page }, Cmd.ofMsg Fetch
        | Redirect page -> { model with page = page }, Cmd.none
        | SetInput input -> { model with input = input }, Agents.Cmd.OfAgent.perform debounce Post
        | Post ->
            model,
            match model.page with
            | Session session ->
                Cmd.OfJS.attempt
                    js
                    "Database.write"
                    [| {| session = session
                          input = model.input |} |]
                    (fun _ -> Redirect Home)
            | Home ->
                let guid = Guid.NewGuid() |> string
                Cmd.OfJS.perform
                    js
                    "Database.write"
                    [| {| session = guid
                          input = model.input |} |]
                    (fun _ -> Session guid |> Redirect)
        | Fetch ->
            model,
            match model.page with
            | Home -> Cmd.none
            | Session tok -> Cmd.OfJS.perform js "Database.read" [| Guid(tok) |] SetInput
        |> log

    let view (model: Model) dispatch =
        Main()
            .HeaderContent(Main.StandardNav().Home(router.getRoute Home).Elt())
            .Input(model.input, SetInput >> dispatch)
            .Elt()

    type MyApp() =
        inherit ProgramComponent<Debugger.Model<Model>, Debugger.Message<Message, Model>>()
        let TIMEOUT = TimeSpan.FromMilliseconds(500)

        let debounce =
            MailboxProcessor.Start(fun inbox ->
                let rec loop timer =
                    async {
                        let! dispatch, msg = inbox.Receive()
                        Agents.Timers.tryDispose timer
                        use timer = Agents.Timers.wait TIMEOUT dispatch msg
                        return! loop (Some timer)
                    }
                loop None)
        //////////////////////////////
        ///
        /// RETRIEVE LOG
        ///
        /////////////////////////////
        let readMessages js (model: Model) msg =
            match model.page with
            | Session session-> 
              Cmd.OfJS.perform js "Database.readLog" [|session|] msg
            | _ -> Cmd.none
        
        let wrappedView (model: Debugger.Model<Model>) dispatch =
            Wrapper()
                .Content( view model.model (Debugger.dispatch model dispatch ))
                .Back(fun _ -> dispatch Debugger.StepBack)
                .Forward(fun _ -> dispatch Debugger.StepForward)
                .ForwardButton(match model.advance with | None -> "opacity-50 cursor-not-allowed" |_-> "")
                .BackButton(match model.advance, model.back with |Some _, [] -> "opacity-50 cursor-not-allowed" |_-> "")
                .Debugging(match model.advance with | None -> "" | _ -> "debugging")
                .Elt()
            

        override this.Program =
            
            let innerUpdate = update debounce this.JSRuntime
            let update = Debugger.update init (readMessages this.JSRuntime) innerUpdate   
            Program.mkProgram
              (fun _ -> Debugger.model init, Cmd.none)
              update 
              wrappedView
            |> Program.withRouter debugRouter
