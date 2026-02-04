# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# First-time setup (copies template and restores dependencies)
./setup.sh    # Linux/Mac
setup.bat     # Windows

# Or manually:
dotnet restore
dotnet build
dotnet run
```

## Setup for New Developers

1. Run `./setup.sh` (Linux/Mac) or `setup.bat` (Windows)
2. Edit `appsettings.json` and add your OpenAI API key
3. Run `dotnet run`

## Project Overview

SogneAgent is a Danish Parish (Sogn) Information Agent built with .NET 8.0 and Microsoft Semantic Kernel. It provides an interactive chatbot that queries the Dataforsyningen API for Danish parish data.

## Architecture

The project uses Semantic Kernel's plugin-based architecture for LLM function calling:

- **Program.cs** - Entry point, initializes Semantic Kernel with OpenAI, registers plugins, runs the chat REPL loop
- **Models/Parish.cs** - Data model mapping JSON from Dataforsyningen API (Danish field names like `navn`, `kode`, `visueltcenter`)
- **Plugins/ParishPlugin.cs** - Semantic Kernel plugin exposing three functions:
  - `search_parishes_by_name()` - Search by name/partial name
  - `get_parish_details()` - Get details by parish code
  - `list_parishes()` - List parishes with optional filtering

API base URL: `https://api.dataforsyningen.dk/sogne`

## Configuration

OpenAI settings are loaded from `appsettings.json` (gitignored) or environment variable `OPENAI_API_KEY`. Use `appsettings.template.json` as reference:

```json
{
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY_HERE",
    "ModelId": "gpt-4o-mini"
  }
}
```

## Key Dependencies

- Microsoft.SemanticKernel (v1.29.0) - AI orchestration with automatic function calling
- Microsoft.Extensions.Configuration - JSON and environment variable configuration

## Architecture Diagrams

### High-Level Architecture

```mermaid
flowchart TB
    subgraph User Interface
        Console[Console REPL]
    end

    subgraph Application
        Program[Program.cs<br/>Entry Point]
        ChatHistory[Chat History]
    end

    subgraph Semantic Kernel
        Kernel[Kernel]
        ChatService[IChatCompletionService]
        FunctionCalling[Auto Function Calling]
    end

    subgraph Plugins
        ParishPlugin[ParishPlugin]
        SearchFn[search_parishes_by_name]
        DetailsFn[get_parish_details]
        ListFn[list_parishes]
    end

    subgraph External
        OpenAI[OpenAI API<br/>gpt-4o-mini]
        Dataforsyningen[Dataforsyningen API<br/>api.dataforsyningen.dk/sogne]
    end

    subgraph Models
        Parish[Parish Model]
    end

    Console --> Program
    Program --> ChatHistory
    Program --> Kernel
    Kernel --> ChatService
    ChatService --> FunctionCalling
    FunctionCalling --> ParishPlugin
    ParishPlugin --> SearchFn
    ParishPlugin --> DetailsFn
    ParishPlugin --> ListFn
    SearchFn & DetailsFn & ListFn --> Dataforsyningen
    Dataforsyningen --> Parish
    ChatService --> OpenAI
```

### Chat Flow Sequence

```mermaid
sequenceDiagram
    participant U as User
    participant P as Program.cs
    participant CH as ChatHistory
    participant SK as Semantic Kernel
    participant OAI as OpenAI API
    participant PP as ParishPlugin
    participant DF as Dataforsyningen API

    U->>P: Enter question
    P->>CH: AddUserMessage()
    P->>SK: GetChatMessageContentAsync()
    SK->>OAI: Send chat + available functions

    alt Function call needed
        OAI-->>SK: Function call request
        SK->>PP: Invoke function (e.g., search_parishes_by_name)
        PP->>DF: HTTP GET /sogne?q=...
        DF-->>PP: JSON response
        PP-->>SK: Formatted result
        SK->>OAI: Send function result
        OAI-->>SK: Final response
    else Direct response
        OAI-->>SK: Response content
    end

    SK-->>P: ChatMessageContent
    P->>CH: AddAssistantMessage()
    P->>U: Display response
```

### Class Structure

```mermaid
classDiagram
    class Program {
        +Main()
        -configuration: IConfiguration
        -kernel: Kernel
        -chatService: IChatCompletionService
        -chatHistory: ChatHistory
    }

    class ParishPlugin {
        -HttpClient _httpClient
        -string BaseUrl
        +SearchByNameAsync(query) string
        +GetParishDetailsAsync(code) string
        +ListParishesAsync(nameFilter, limit) string
    }

    class Parish {
        +string Name
        +string Code
        +double[] VisualCenter
        +double[] BoundingBox
        +DateTime? Changed
        +DateTime? GeoChanged
        +int? GeoVersion
        +ToString() string
    }

    Program --> ParishPlugin : registers
    ParishPlugin --> Parish : deserializes to
```
