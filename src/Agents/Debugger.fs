namespace Agents
open Elmish

module Debugger =
    type Message<'Msg, 'Model> =
        | Message of 'Msg 
        | StepForward
        | StepBack
        | Load
        | Populate of 'Msg list

    type Model<'T> =
        { model: 'T
          advance: 'T list option
          back: 'T list  }    

    let model inner =
        { model = inner
          advance = None
          back = [] }            

    // View Transformer
    let view (innerView: 'T -> ('M -> unit) -> 'N) =
        fun model dispatch ->
            innerView
                model.model
                ( Message >> dispatch )
    
    let update (init: 'T) readAsync (innerUpdate : 'M -> 'T -> 'T*Cmd<'M>) (message: Message<'M, 'T>) (model: Model<'T>) =
        System.Console.WriteLine(message)
        let fold (messages: 'M list) : 'T list=
            let justUpdateModel msg = innerUpdate msg >> fst
            let folder =
                function
                | a :: rest ->
                    fun m -> justUpdateModel m a :: a :: rest
                | _ ->
                    fun m -> justUpdateModel m init :: init :: []
            List.fold folder [] messages
            
            
        match message, model  with
            |Populate messages, _ ->
                {
                 model with
                   advance = Some []
                   back = fold messages 
                 }, Cmd.ofMsg StepBack
            | StepForward, {advance = Some (a :: rest)}->
                { model with
                    advance = Some rest
                    back = model.model :: model.back
                    model = a                 
                 }, Cmd.none
            // Done with updates
            | StepForward, { advance = Some []} ->
                {
                    model with
                        advance = None                        
                }, Cmd.none
            | StepBack, { advance = Some items; back = a :: rest } ->
                {
                    model with
                      advance = Some (model.model :: items)
                      model = a
                      back = rest
                }, Cmd.none
            | StepBack, { advance = Some _ ; back = [] } ->
               model, Cmd.none
            | StepBack, {advance = None} ->
                model, readAsync model.model Populate
            | Message msg, {advance = None} ->
                let updated, cmd = innerUpdate msg model.model
                { model with
                    model = updated
                }, Cmd.map Message cmd            
            | _ -> model, Cmd.none
    
    
    
 

 
