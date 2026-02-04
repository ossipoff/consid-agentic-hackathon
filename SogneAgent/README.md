# SogneAgent - Danish Parish Information Agent

An interactive chatbot that provides information about Danish parishes (sogne) using Microsoft Semantic Kernel and the Dataforsyningen API.

## What is Semantic Kernel?

[Semantic Kernel](https://github.com/microsoft/semantic-kernel) is Microsoft's open-source SDK for building AI agents and integrating LLMs into applications. Key features used in this project:

- **Plugin Architecture** - Encapsulate functionality as plugins that the LLM can invoke
- **Automatic Function Calling** - The LLM decides when to call your functions based on user intent
- **Chat Completion** - Manages conversation history and context

In this project, the `ParishPlugin` exposes three functions to the LLM. When a user asks "What parishes are in Copenhagen?", Semantic Kernel automatically:
1. Sends the question to OpenAI along with available function definitions
2. OpenAI responds with a function call request (`search_parishes_by_name("København")`)
3. Semantic Kernel executes the function and returns results to OpenAI
4. OpenAI formulates a natural language response

```
User: "Tell me about Trinitatis parish"
  |
Semantic Kernel -> OpenAI: "User wants parish info, here are available functions..."
  |
OpenAI -> Semantic Kernel: "Call get_parish_details(code='7003')"
  |
Semantic Kernel -> ParishPlugin -> Dataforsyningen API
  |
OpenAI: "Trinitatis parish (code 7003) is located in central Copenhagen..."
```

## Features

- Search parishes by name or partial name
- Get detailed parish information (coordinates, boundaries, metadata)
- List and browse parishes
- Natural language interface powered by GPT-4o-mini

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- OpenAI API key

### Setup

**Windows:**
```cmd
setup.bat
```

**Linux/Mac:**
```bash
chmod +x setup.sh
./setup.sh
```

Then edit `appsettings.json` and add your OpenAI API key.

### Run

```bash
dotnet run
```

## Project Structure

```
SogneAgent/
├── Program.cs              # Entry point, chat loop, Semantic Kernel setup
├── Models/
│   └── Parish.cs           # Data model for parish entities
├── Plugins/
│   └── ParishPlugin.cs     # Semantic Kernel plugin with API functions
├── appsettings.json        # Configuration (gitignored)
└── appsettings.template.json
```

## API Reference

This project uses the [Dataforsyningen API](https://api.dataforsyningen.dk/sogne) - Denmark's official geographic data service.

## License

MIT
