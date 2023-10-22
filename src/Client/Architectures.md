# M-V-U Architecture

## M-V-U 
```mermaid
flowchart LR
    Model(Model) -->|View Engine| View
    View(View) -->|Dispatch| Update
    Update(Update) -->|Update Function| Model    
```
* All updates to model are dispatched through a single message queue
* Model is represented by immutable data
* View updated when model changes

## M-V-VM (Not Unidirectional!)
```mermaid
flowchart LR
    Model(Model) --> |Update| ViewModel
    View(View) <-->|Data Binding| ViewModel    
    ViewModel(ViewModel) --> |Dispatch| Model
    
    
```
## M-V-C (Multiple channels!)
```mermaid
flowchart LR
    Model(Model) -->|Update| View
    View(View) -->|User Interaction| ControllerB(Controller)
    View -->|User Interaction| ControllerA(Controller)
    ControllerA -->|Manipulates| Model(Model)
    ControllerB -->|Manipulates| Model(Model)    
     
```

