using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SogneAgent.Plugins;

// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var apiKey = configuration.GetSection("OpenAI")["ApiKey"]
    ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException(
        "OpenAI API key not found. Copy appsettings.template.json to appsettings.json and add your key, or set OPENAI_API_KEY environment variable.");

var modelId = configuration.GetSection("OpenAI")["ModelId"] ?? "gpt-4o-mini";

// Create the kernel with OpenAI chat completion
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(modelId, apiKey);
var kernel = builder.Build();

// Register the Parish plugin
kernel.Plugins.AddFromObject(new ParishPlugin(), "Parish");

// Get chat completion service
var chatService = kernel.GetRequiredService<IChatCompletionService>();

// Configure automatic function calling
var executionSettings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

// Initialize chat history with system prompt
var chatHistory = new ChatHistory();
chatHistory.AddSystemMessage("""
    You are a helpful assistant that specializes in Danish parishes (sogne).
    You have access to the official Danish parish registry through the Dataforsyningen API.

    You can help users:
    - Search for parishes by name
    - Get detailed information about specific parishes (coordinates, boundaries)
    - List and discover parishes in Denmark

    When users ask about parishes, use your available functions to look up accurate,
    current information. Always be helpful and provide context about the parishes when relevant.

    Danish parishes (sogne) are ecclesiastical divisions used in Denmark, originally
    for church administration but now also used for various administrative purposes.
    """);

// Welcome message
Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║           Danish Parish (Sogn) Information Agent             ║");
Console.WriteLine("║                  Powered by Semantic Kernel                  ║");
Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
Console.WriteLine("║  Ask me anything about Danish parishes!                      ║");
Console.WriteLine("║  Examples:                                                   ║");
Console.WriteLine("║    - 'What parishes are in Copenhagen?'                      ║");
Console.WriteLine("║    - 'Tell me about Trinitatis parish'                       ║");
Console.WriteLine("║    - 'What is the parish code for Roskilde Domsogn?'         ║");
Console.WriteLine("║    - 'List some parishes'                                    ║");
Console.WriteLine("║                                                              ║");
Console.WriteLine("║  Type 'exit' or 'quit' to end the conversation.              ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Chat loop
while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("You: ");
    Console.ResetColor();

    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye! Farvel!");
        break;
    }

    // Add user message to history
    chatHistory.AddUserMessage(userInput);

    try
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Assistant: ");
        Console.ResetColor();

        // Get response with automatic function calling
        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            kernel);

        Console.WriteLine(response.Content);
        Console.WriteLine();

        // Add assistant response to history
        chatHistory.AddAssistantMessage(response.Content ?? string.Empty);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
        Console.WriteLine();
    }
}
