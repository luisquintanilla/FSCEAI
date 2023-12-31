# F# CE Fun

Sample applications of [F# Computation Expressions (CEs)](https://learn.microsoft.com/dotnet/fsharp/language-reference/computation-expressions)

## Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Azure OpenAI Service](https://aka.ms/oai/access?azure-portal=true)
    - [Deployments](https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource?pivots=cli)
        - [Completions Model](https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource?pivots=cli)
        - [Embeddings Model](https://learn.microsoft.com/azure/cognitive-services/openai/concepts/models#embeddings-models)

## Data

The data used in this sample is a subset from the following [dataset](https://huggingface.co/datasets/Cohere/wikipedia-22-12-simple-embeddings/viewer/Cohere--wikipedia-22-12-simple-embeddings/train).

- *data.json* contains the raw data and contains only the `title`,`text`,`wiki_id` and `paragraph_id` columns.
- *embeddings.json* the original data as well as embeddings generated by OpenAI embeddings model. This file gets generated by *RAGBuild.fsx*.

## Run

1. Get your Azure OpenAI Service endpoint, model, and key values and replace them in the `RAGBuild.fsx` and `RAGConsume.fsx` scripts.

    - **AOAI-ENDPOINT** - Replace with your Azure OpenAI endpoint value
    - **AOAI-KEY** - Replace with your Azure OpenAI key credential value
    - **GPT-MODEL** - Replace with the name (deployment ID) of your GPT model
    - **EMBEDDING-MODEL** - Replace with the name (deployment ID) of your embedding model

1. In the terminal, use the .NET CLI to run the following command.

    - Run the embedding generation process

        ```dotnetcli
        dotnet fsi RAGBuild.fsx        
        ``` 

    - Run script that implements Retrieval Augmented Generation (RAG) pattern that uses the data, embeddings, and GPT to answer a question.

        ```dotnetcli
        dotnet fsi RAGConsume.fsx
        ```

## Miscellanous

The *Modules.fsx* file contains F# Computation Expressions (CE) which help scaffold Embedding and GPT components as well as run jobs to generate embeddings and text completions. These show up in the code as `embeddingconfig`, `embedding`, `gptoptions` and `gptcompletion`.