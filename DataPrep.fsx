// Install packages
#r "nuget: ParquetSharp.DataFrame, 0.1.0"
#r "nuget: Microsoft.Data.Analysis, 0.21.0-preview.23266.6"

// Add open statements
open ParquetSharp
open Microsoft.Data.Analysis
open System.Text.Json
open System.IO

// Create Parquet file reader
let reader = new ParquetFileReader("./data/wikipedia-embeddings.parquet")

// Load data into DataFrame
let df = reader.ToDataFrame(columns=[|"title";"text";"wiki_id";"paragraph_id"|])

// Format data into anonymous object and serialize to JSON
let data = 
    df.Rows
    |> Seq.map(fun x -> 
    {|
        Title=x[0]
        Text=x[1]
        WikiId=x[2]
        ParagraphId=x[3]
    |})
    |> Array.ofSeq
    |> JsonSerializer.Serialize

// Save to file
File.WriteAllText("data.json",data)