Let's try Bolero

# Introduction
Behind this talk is the story of a small team (4 people) of engineers & data scientists who desperately 
needed to update an app written long ago (8-10 years) by a long forgotten people (consultants, now defunct)
in an ancient and arcane script (AngularJS). Only the manager (me) has experience in UI/typescript/etc. 
Others: MATLAB (mechanical engineer), R/Python (data scientist), F#/C# (software engineer). 

Problem: Front End development is **hard**.
* Need to understand design, psychology, asynchronous programming
* It has to be disposable! Keep it simple
* Some parts are easy: onboarding (but this makes it harder!)
  * run `create-awesomesaucely-app`, download half of NPM (`node-package-manager`), you're up and running!
  * 6 months later--every dependency is broken
* `node` ecosystem ever-evolving. (I have package manager `fnm` for my package manager `npm`)

Requirements: 
* .NET ecosystem (Blazor)
* Easy to maintain/reason about (for F# devs at least...)
* Minimal effort
* Performance? SEO? Ecommerce plugin? who cares. 

# Bolero
Bolero is an F# web framework that builds on Blazor (.NET webassembly) & Elmish (F# MVU). 
Maintained by [Intellifactory](https://intellifactory.com).
It is a joy to program in. 

I want to talk about a few things:
* What is the MVU architecture? Why F#?
* How can we *zhoosh* up MVU with plugins?
* Why MVU makes event sourcing trivial, and why event sourcing is cool.

Oh, and by the way--this is a coding talk! we're going to work through my repo [Bolero Agents](https://github.com/clbrem/bolero-agents).