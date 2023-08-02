namespace Agents

module Cmd =
    open Elmish
    module OfAgent =
        let perform<'S,'T> (agent: MailboxProcessor<Dispatch<'S> * 'T>) (message:'T) = 
            Cmd.ofEffect (fun dispatch -> agent.Post(dispatch, message))
        
        let attempt<'S,'T> (agent: MailboxProcessor<'S>) (handler: exn -> 'T) =
            Cmd.ofEffect (fun dispatch -> agent.Error.Add(handler >> dispatch))
        
        let either<'S,'T> (agent: MailboxProcessor<Dispatch<'S> * 'T>) (message: 'T) (handler : exn -> 'S) =
            Cmd.ofEffect (
                fun dispatch ->
                    do agent.Error.Add(handler >> dispatch)
                    do agent.Post(dispatch, message)
                    )
        