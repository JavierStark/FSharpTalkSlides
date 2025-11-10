# Slide 1: Hello F#

Welcome to the presentation!

```fsharp
printfn "Hello world from F#"
```

---

# Slide 2: Lists and Pipelines

You can even show simple data transformations:

```fsharp
[1..5]
|> List.map (fun x -> x * x)
|> List.sum
```
