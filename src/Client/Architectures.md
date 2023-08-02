# M-V-U Architecture

## M-V-U 
```mermaid
flowchart TD
    View(View) -->|Dispatch| Update
    Model(Model) -->|View Engine| View 
    Update(Update) -->|Update Function| Model    
```
* All updates to model are dispatched through a single message queue
* Model is represented by immutable data
* View updated when model changes

## M-V-VM (Not Unidirectional!)
```mermaid
flowchart TD
    View(View) <-->|Data Binding| ViewModel
    ViewModel(ViewModel) --> |Dispatch| Model
    Model(Model) --> |Update| ViewModel
```
## M-V-C (Not Immutable!)
```mermaid
flowchart TD
    View(View) -->|User Interaction| Controller(Controller)
    Controller -->|Manipulates| Model(Model)
    Model --> |Update|View
```

