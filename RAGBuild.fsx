#load "./Modules.fsx"

open System.IO
open System.Text.Json
open Modules.Utils
open Modules.Builders

(*
    RAG Builder

    1. Scenario (RAG)
    2. Environment
        a. Choose your model (s)
            i. Embeddings
            ii. Completions
    3. Data
        a. Load data
    4. Generate Embeddings
        b. Generate embeddings
        c. Save embeddings to a file
    5. Try it out
        a. Enter a query
        b. Show most relevant documents
        c. Generate an answer
*)

// 1. Scenario (RAG)

// 2. Environment - Choose your models

// Completions client & options
let c = createClient "AOAI-ENDPOINT" "AOAI-KEY"

let cc = 
    gptoptions {
        use_client c
        use_model "davinci"
    }

// Embeddings client & options
let e = createClient "AOAI-ENDPOINT" "AOAI-KEY"

let ec = 
    embeddingconfig {
        use_client e
        use_model "embeddings-ada-002"
    }

// 3. Data - load your data
// Filtering only first 10 articles to return quickly
type OriginalData = 
    {
        Title:string
        Text:string
        WikiId:int
        ParagraphId:int
    }

let data = 
    File.ReadAllText("./data/data.json")
    |> JsonSerializer.Deserialize<OriginalData array>
    |> Array.groupBy(fun x -> x.WikiId)
    |> Array.take 10
    |> Array.collect snd

// 4. Generate Embeddings

// Completions Client

// Generate embeddings
let embeddingsData = 
    data
    |> Array.map(fun x -> 
    {|
        Embedding = embedding { with_config ec; text x.Text }
        Metadata = x
    |})

// Save data

File.WriteAllText("./data/embeddings.json",JsonSerializer.Serialize(embeddingsData))

// 5. Try it out
let userQuery = 
    embedding {
        with_config ec
        text "What is the name of Cristiano Ronaldo's Son?"
    }

let top3 = 
    embeddingsData
    |> Array.sortByDescending(fun x -> cosineSimilarity userQuery x.Embedding)
    |> Array.take 3

top3
|> Array.iter(fun x -> printfn $"Text: {x.Metadata.Text}\nSimilarity: {cosineSimilarity userQuery x.Embedding}")