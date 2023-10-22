namespace Client
open System.Text.Json
open Bolero
open Bolero.Remoting.Client
open Bolero.Templating.Client
open System
open Microsoft.AspNetCore.Components


type Main = Template<"wwwroot/main.html">

type Page = 
    
    | [<EndPoint "/">] Home
    | [<EndPoint "/session">] Session of string

type Message =
    | Navigate of Page
    | Redirect of Page
    | Post
    | SetInput of string
    | Fetch

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

    let update debounce js message model =
        Console.WriteLine $"{message}"
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
        
        //////////////////////////////////////////
        ///
        /// INITIALIZE DEBOUNCE 
        ///
        /////////////////////////////////////////
        let debounce = Agents.Timers.delay(TIMEOUT)        
        
        override this.Program =
            Program.mkProgram
               ( fun _ -> init, Cmd.none)
               (update debounce this.JSRuntime) view
            |> Program.withRouter router

