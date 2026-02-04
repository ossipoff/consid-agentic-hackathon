# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
dotnet restore
dotnet build
dotnet run
```

## Architecture

Semantic Kernel plugin-based architecture with automatic LLM function calling:

- **Program.cs** - Entry point, initializes Kernel with OpenAI, registers plugins, runs chat REPL
- **Models/Parish.cs** - Data model mapping Danish JSON fields (`navn`, `kode`, `visueltcenter`)
- **Plugins/ParishPlugin.cs** - Three kernel functions:
  - `search_parishes_by_name(query)` - Search by name/partial name
  - `get_parish_details(code)` - Get details by parish code
  - `list_parishes(nameFilter?, limit?)` - List with optional filtering

External API: `https://api.dataforsyningen.dk/sogne`

## Key Dependencies

- Microsoft.SemanticKernel (v1.29.0) - AI orchestration with auto function calling
- Microsoft.Extensions.Configuration - Config from appsettings.json and environment variables

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
