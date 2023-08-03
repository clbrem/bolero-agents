namespace Client
open System.Text.Json
open System.Text.Json.Serialization
open Bolero
open Bolero.Remoting.Client
open Bolero.Templating.Client
open System
open Microsoft.AspNetCore.Components

type Main = Template<"wwwroot/main.html">

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
type Model =
    { page: Page
      input: string
    }
    

module Main =
    open Elmish
    
    let router = Router.infer Navigate (fun model -> model.page)
        
    let init = {
        page = Home
        input = ""
    }

    let update  debounce js message model =
        Console.WriteLine $"{JsonSerializer.Serialize(message)}"
        let log (newModel, cmd) =
            match model.page with
            | Session session ->                
                newModel, Cmd.batch [cmd; Cmd.OfJS.attempt js "Database.log" [|{|session = session; message = message|}|] (fun _ -> Redirect Home)]
            | _ ->
                newModel, cmd
            
    
        match message with
        | Navigate page ->
            {model with page = page}, Cmd.ofMsg Fetch        
        | SetInput input ->
            /////////////////////////////
            // 
            // Added Debounce
            //
            ///////////////////////////
            {model with input = input}, Agents.Cmd.OfAgent.perform debounce Post 
        | Redirect page ->
            { model with page = page }, Cmd.none
        | Post ->
            model, 
            match model.page with
            | Session session -> 
              Cmd.OfJS.attempt js "Database.write" [|{|session = session; input = model.input |}|] (fun _ -> Redirect Home)
            | Home ->                
                let guid =  Guid.NewGuid() |> string 
                Cmd.OfJS.perform js "Database.write" [|{|session = guid; input = model.input|}|] (fun _ -> Session guid |> Redirect)
        | Fetch ->
            model, 
            match model.page with
            | Home -> Cmd.none
            | Session tok -> Cmd.OfJS.perform js "Database.read" [| Guid(tok) |] SetInput
        |> log 

    let view (model: Model) dispatch =
        Main()
            .HeaderContent(              
              Main.StandardNav().Home(router.getRoute Home).Elt()
            )
            .Input(model.input, SetInput >> dispatch)            
            .Elt()            
    
    type MyApp() =
        inherit ProgramComponent<Model, Message>()
        let TIMEOUT = TimeSpan.FromMilliseconds(500)
        let debounce = MailboxProcessor.Start(
            fun inbox ->
                let rec loop timer =
                    async {
                        let! dispatch, msg = inbox.Receive()
                        Agents.Timers.tryDispose timer
                        use timer = Agents.Timers.wait TIMEOUT dispatch msg
                        return! loop (Some timer)
                    }
                loop None
            )

        

        override this.Program =
            Program.mkProgram
               ( fun _ -> init, Cmd.ofMsg Fetch)
               (update debounce this.JSRuntime) view
            |> Program.withRouter router

