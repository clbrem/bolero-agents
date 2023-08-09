namespace Agents


module Debugger =
    type Message<'Msg> =
        | Message of 'Msg
        | StepForward
        | StepBack

    type Model<'T> =
        { model: 'T
          advance: 'T list option
          back: 'T list option }

    let model inner =
        { model = inner
          advance = None
          back = None }

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
                
