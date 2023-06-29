#load "./Modules.fsx"

open System
open System.IO
open System.Text.Json
open Modules.Builders
open Modules.Utils

// Define data types
type OriginalData = 
    {
        Title:string
        Text:string
        WikiId:int
        ParagraphId:int
    }

type EmbeddingData = 
    {
        Embedding: float32 array
        Metadata: OriginalData
    }

// Configure data source
let dataSource = 
    File.ReadAllText("./data/embeddings.json")
    |> JsonSerializer.Deserialize<EmbeddingData array>

// Configure models
let embeddingOptions = 
    let embeddingClient = createClient "AOAI-ENDPOINT" "AOAI-KEY"
    embeddingconfig {
        use_client embeddingClient
        use_model "EMBEDDING-MODEL"
    }

let completionOptions = 
    let gptClient = createClient "AOAI-ENDPOINT" "AOAI-KEY" 
    gptoptions {
        use_client gptClient
        use_model "GPT-MODEL"
    }

// Define function to run RAG pattern
let RAG (q:string) (dataSource:EmbeddingData array) = 

    // 1. Convert user query to embedding
    let qEmbedding = 
        embedding { 
            with_config embeddingOptions
            text q
        }

    // 2. Get top 3 sources by comparing cosine similarity
    // between user query vector and document vectors
    let sources = 
        dataSource
        |> Array.sortByDescending(fun doc -> cosineSimilarity qEmbedding doc.Embedding)
        |> Array.take 3

    // 3. (Optional) Format sources
    let formattedSources = 
        sources
        |> Array.map(fun doc -> $"{doc.Metadata.WikiId}: {doc.Metadata.Text}")
        |> fun src -> String.Join('\n', src)

    // Define prompt template
    let promptTemplate (sources:string) (input:string)= 
        $"
        You are an agent. You have access to articles to help you answer questions. Use only the contents from these documents to answer questions.

        {sources}

        {input}
        "

    printfn $"{(promptTemplate formattedSources q)}"

    // 4. Generate completion by injecting 
    // original user query and sources into prompt template 
    gptcompletion { 
        with_config completionOptions
        prompt (promptTemplate formattedSources q) }

// Define user query
let question = "What is the name of Cristiano Ronaldo's son?"

// RUN IT!
let answer = RAG question dataSource

printfn $"Q: {question}\nA: {answer.Trim()}"
