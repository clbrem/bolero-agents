﻿namespace Agents

module Timers =
    open System.Timers
    let tryDispose (maybeTimer: Timer option) =
        match maybeTimer with
        | Some t -> t.Dispose()
        | None -> ()
    let wait (timeout: System.TimeSpan) (action: 'T -> unit) (msg: 'T)=
        let timer = new Timer(timeout)
        timer.AutoReset <- false
        timer.Elapsed.AddHandler (fun _ _ -> action msg )        
        timer.Start()
        timer
    
    let repeat (timeout: System.TimeSpan) (action: 'T -> unit) (msg: 'T) =
        let timer = new Timer(timeout)
        timer.AutoReset <- true
        timer.Elapsed.AddHandler (fun _ _ -> action msg )
        timer.Start()
        timer
    
    /////////////////////////////////////
    ///
    /// DEBOUNCE AGENT
    ///
    /////////////////////////////////////
    
    let delay(timeout: System.TimeSpan) =
        MailboxProcessor.Start(
            fun inbox ->
                let rec loop timer =
                    async {
                       // Block the thread until a new message comes in
                       let! dispatch, msg  = inbox.Receive()
                       // If timer hasn't fired, dispose it!
                       tryDispose timer
                       // Create a new timer that fires with msg after a while.
                       use timer = wait timeout dispatch msg 
                       return! Some timer |> loop                        
                    }
                loop None                
            )
    let poll (interval: System.TimeSpan) =
        MailboxProcessor.Start(
            fun inbox ->
                let rec loop timer =
                    async {
                       match! inbox.Receive() with
                       | dispatch, Some msg ->
                           tryDispose timer                       
                           use timer = repeat interval dispatch msg 
                           return! Some timer |> loop 
                       | _, None ->
                           tryDispose timer
                           return! loop None
                    }
                loop None                
            )
    
        


