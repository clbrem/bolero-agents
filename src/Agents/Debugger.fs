namespace Agents
open Elmish

module Debugger =
    type Message<'Msg> =
        | Message of 'Msg 
        | StepForward
        | StepBack
        | Load

    type Model<'T> =
        { model: 'T
          advance: 'T list option
          back: 'T list  }    

    let model inner =
        { model = inner
          advance = None
          back = [] }        

    // tests if isDebugging
    let isDebugging =
        function
        | { advance = Some _ } -> true
        | _ -> false

    // View Transformer
    let view (innerView: 'T -> ('M -> unit) -> 'N) =
        fun model dispatch ->
            innerView
                model.model
                (
                // ignore if is debugging
                if isDebugging model then
                    fun _ -> ()
                else
                    Message >> dispatch
                )
    
    let update readAsync (innerUpdate : 'M -> 'T -> 'T*Cmd<'M>) (message: Message<'M>) (model: Model<'T>) =
        
        match message, model  with
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
                        model = a
                }, Cmd.none
            | StepBack, { advance = Some items; back = a :: rest } ->
                {
                    model with
                      advance = Some (model.model :: items)
                      model = a
                      back = rest
                }, Cmd.none
            | StepBack, { advance = Some items; back = [] }
               {
                   
               }
            | _ -> model, Cmd.none

            match message with
            | Message msg ->
                let updated, cmd = innerUpdate msg model.model
                { model with
                    model = updated
                }, Cmd.map Message cmd
            | _ -> model, Cmd.none 
            
 

 
