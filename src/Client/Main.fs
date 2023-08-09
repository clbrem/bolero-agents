namespace Client
open Bolero
open Bolero.Remoting.Client
open Bolero.Templating.Client
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open System

type Main = Template<"wwwroot/main.html">


type Page =    
    | [<EndPoint "/">] Home
    | [<EndPoint "/session">] Session of string


////////////////////////
///
///  MESSAGE
///
//////////////////////

type Message =
    | Navigate of Page      // Navigate to a Page
    | Redirect of Page      // Same as Navigate     
    | Post                  // POST form
    | SetInput of string    // Set form input
    | Fetch                 // FETCH input history 

////////////////////////////////////
///
///    MODEL
///
/////////////////////////////////// 

type Model =
    { page: Page
      input: string
     }
    

module Main =
    open Elmish
    
    ///////////////////////////////////
    ///
    ///   ROUTER
    ///
    ///////////////////////////////////

    let router = Router.infer Navigate (fun model -> model.page)
    
    ///////////////////////////////////
    ///
    ///   INIT
    ///
    //////////////////////////////////
    let init = {
        page = Home
        input = ""
    }
    
    //////////////////////////////////
    ///    
    ///   UPDATE
    ///
    ///////////////////////////////////

    let update js message model =
        Console.WriteLine ($"{message}")
        match message with
        | Navigate page ->
            {model with page = page}, Cmd.ofMsg Fetch        
        | SetInput input ->
            {model with input = input}, Cmd.none
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
        
    ///////////////////////////////////////////
    ///
    ///  VIEW    
    ///
    /// //////////////////////////////////////

    let view (model: Model) dispatch =
        Main()
            .HeaderContent(              
              Main.StandardNav().Home(router.getRoute Home).Elt()
            )
            .Input(model.input, SetInput >> dispatch)
            .Post(fun _ -> dispatch Post)
            .Elt()            
    
    type MyApp() =
        inherit ProgramComponent<Model, Message>()

        override this.Program =
            Program.mkProgram ( fun _ -> init, Cmd.none) (update this.JSRuntime) view
            |> Program.withRouter router

