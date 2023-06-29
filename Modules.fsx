#r "nuget: Azure.AI.OpenAI, 1.0.0-beta.5"

open System
open System.Linq
open Azure.AI.OpenAI

[<AutoOpen>]
module Domain = 

    type EmbeddingConfig = 
        {
            Client: OpenAIClient
            ModelName: string
        }
        with static member Empty = 
            {
                Client = OpenAIClient("")
                ModelName = ""
            }

    type Embedding = 
        {
            Config: EmbeddingConfig
            Text: string
        }
        with static member Empty = 
            {
                Config = EmbeddingConfig.Empty
                Text = ""
            }

    type GPTCompletionOptions = 
        {
            Client: OpenAIClient
            Temperature: float32
            MaxTokens: int
            ModelName: string
        }
        with static member Empty = 
            {
                Client = new OpenAIClient("")
                Temperature = 0.0f
                MaxTokens = 60
                ModelName = ""
            }

    type GPTCompletion =
        {
            GPTOptions: GPTCompletionOptions
            Prompt: string
        }
        with static member Empty = 
            {
                GPTOptions = GPTCompletionOptions.Empty
                Prompt = ""
            }

[<AutoOpen>]
module Builders = 

    open Domain

    type EmbeddingConfigBuilder () = 

        member _.Yield _ = EmbeddingConfig.Empty

        [<CustomOperation "use_client">]
        member _.UseClient (state:EmbeddingConfig, client:OpenAIClient) = 
            {state with Client = client}

        [<CustomOperation "use_model">]
        member _.UseModel (state:EmbeddingConfig, deploymentId:string) = 
            { state with ModelName = deploymentId }

    type EmbeddingBuilder () = 

        member _.Yield _ = Embedding.Empty

        member _.Run (state:Embedding) = 
            let eo = new EmbeddingsOptions(state.Text)

            let embedding = 
                async {
                    let! e = 
                        state.Config.Client.GetEmbeddingsAsync(state.Config.ModelName, eo)
                        |> Async.AwaitTask

                    let res = 
                        match e.HasValue with
                        | true -> e.Value.Data[0].Embedding.ToArray()
                        | false -> [||]

                    return res
                } |> Async.RunSynchronously

            embedding

        [<CustomOperation "text">]
        member _.Text (state: Embedding, text:string) =
            { state with Text = text}

        [<CustomOperation "with_config">]
        member _.WithConfig (state:Embedding, config:EmbeddingConfig) = 
            { state with Config = config }

    type GPTCompletionOptionsBuilder () = 
        
        member _.Yield _ = 
            GPTCompletionOptions.Empty

        [<CustomOperation "use_client">]
        member _.UseClient (state:GPTCompletionOptions, client:OpenAIClient) = 
            {state with Client = client}

        [<CustomOperation "use_model">]
        member _.UseModel (state:GPTCompletionOptions, deploymentId:string) = 
            { state with ModelName = deploymentId } 

        [<CustomOperation "set_temperature">]
        member _.SetTemperature (state:GPTCompletionOptions, temp:float32) = 
            { state with Temperature = temp }

        [<CustomOperation "set_tokens">]
        member _.SetTokens (state:GPTCompletionOptions, maxTokens:int) = 
            { state with MaxTokens = maxTokens }   

    type GPTCompletionBuilder () = 

        member _.Yield _ = GPTCompletion.Empty

        member _.Run (state: GPTCompletion) = 
            let co = new CompletionsOptions()
            co.MaxTokens <- state.GPTOptions.MaxTokens
            co.Temperature <- state.GPTOptions.Temperature
            co.Prompts.Add(state.Prompt)

            let completion = 
                async {
                    let! req = 
                        state.GPTOptions.Client.GetCompletionsAsync(state.GPTOptions.ModelName, co)
                        |> Async.AwaitTask

                    let c = 
                        match req.HasValue with
                        | true -> req.Value.Choices[0].Text
                        | false -> ""

                    return c
                } |> Async.RunSynchronously

            completion

        [<CustomOperation "prompt">]
        member _.Prompt (state: GPTCompletion, text:string) =
            { state with Prompt = text}

        [<CustomOperation "with_config">]
        member _.WithConfig (state:GPTCompletion, config:GPTCompletionOptions) = 
            { state with GPTOptions = config }

    let embeddingconfig = new EmbeddingConfigBuilder()
    let embedding = new EmbeddingBuilder()

    let gptoptions = new GPTCompletionOptionsBuilder()

    let gptcompletion = GPTCompletionBuilder()

[<AutoOpen>]
module Utils = 

    let createClient (endpoint:string) (key:string) = 
        let uri = new Uri(endpoint)
        let keyCredentials = Azure.AzureKeyCredential(key)
        new OpenAIClient(uri,keyCredentials)

    let cosineSimilarity (vector1: float32 array) (vector2: float32 array) =
        if Array.length vector1 <> Array.length vector2 then
            failwith "Vector dimensions must match."
        else
            let dotProduct = Array.map2 (*) vector1 vector2 |> Array.sum
            let magnitude1 = sqrt (Array.sumBy (fun x -> x * x) vector1)
            let magnitude2 = sqrt (Array.sumBy (fun x -> x * x) vector2)
            
            dotProduct / (magnitude1 * magnitude2)