using ADVCalculatorServer.Tools;
using Azure.AI.OpenAI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI.Chat;
using System.ClientModel;
using System.Net;
using System.Text.Json;
using ADVCalculatorServer.services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<Adv_Calculator>();

//builder.Services.AddScoped<IAuthService, AuthService>();

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

Console.WriteLine("Please Select Server Mode\nSERVER MODE: 0\nCLIENT MODE: 1");
var mode = Console.ReadLine();

if (mode == "1")
{
    Uri Endpoint = new Uri(configuration["CURRENT_ENDPOINT"]);
    string serverName = "Adv Calculator";

    var tools = new List<string> { "NthRoot", "LogNum", "PowNum" };
    var argumentsData = new Dictionary<string, Dictionary<string, string>>
    {
        ["NthRoot"] = new Dictionary<string, string>
        {
            ["num"] = "double",
            ["n"] = "double"
        },
        ["LogNum"] = new Dictionary<string, string>
        {
            ["num"] = "double",
            ["logBase"] = "double"
        },
        ["PowNum"] = new Dictionary<string, string>
        {
            ["num"] = "double",
            ["p"] = "double"
        }
    };

    string serverDesc = "This an Advance calculator that can be used to carry out maths until seconary grade";

    var clientTransport = new SseClientTransport(new SseClientTransportOptions
    {
        Endpoint = new Uri(configuration["REGISTRY_SERVER_ENDPOINT"]),
    });

    var regClient = await McpClientFactory.CreateAsync(clientTransport);

    Console.WriteLine("Enter the value based on mode of operation");
    Console.WriteLine("Login: 0");
    Console.WriteLine("Register: 1");
    mode = Console.ReadLine();

    Console.WriteLine("UserName: ");
    string username = Console.ReadLine();
    Console.WriteLine("Password: ");
    string pwd = Console.ReadLine();

    string role = string.Empty;
    if (mode == "1")
    {
        Console.WriteLine("Role(admin, math, geo): ");
        role = Console.ReadLine();

        var regArguments = new Dictionary<string, object?>
        {
            { "username", username },
            { "password", pwd },
            { "role", role },
        };

        var regResponse = await regClient.CallToolAsync("Register", regArguments);
        Console.WriteLine(((TextContentBlock)regResponse.Content[0]).Text);
    }

    var authArguments = new Dictionary<string, object?>
    {
        { "username", username },
        { "password", pwd }
    };

    var authResponse = await regClient.CallToolAsync("Login", authArguments);
    string token = ((TextContentBlock)authResponse.Content[0]).Text;

    //------------------------------------------------------------------------------------------------------------------
    var available_tools = await regClient.ListToolsAsync();

    var toolInfoForPrompt = new System.Text.StringBuilder();
    toolInfoForPrompt.AppendLine("--- AVAILABLE TOOLS AND THEIR SCHEMAS ---");
    foreach (var tool in available_tools)
    {
        string rawSchemaJson = tool.JsonSchema.GetRawText();
        using var jsonDoc = JsonDocument.Parse(rawSchemaJson);
        string formattedSchema = JsonSerializer.Serialize(jsonDoc.RootElement, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        toolInfoForPrompt.AppendLine($"Tool Name: {tool.Name}");
        toolInfoForPrompt.AppendLine($"Description: {tool.Description}");
        toolInfoForPrompt.AppendLine("Schema:");
        toolInfoForPrompt.AppendLine(formattedSchema);
        toolInfoForPrompt.AppendLine("-----------------------------------------");
    }

    //--------------------------------------------------------------------------------------------------------------------

    Console.WriteLine("Client Started!!!");
    Console.WriteLine("type \"quit\" to exit\n");

    var deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT"];
    var endpoint = configuration["AZURE_OPENAI_ENDPOINT"];
    var key = configuration["AZURE_OPENAI_KEY"];

    var client = new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(key)
            );

    var chatClient = client.GetChatClient(deploymentName);

    var serverDetailsForPrompt = new Dictionary<string, object?>
    {
        ["Endpoint"] = Endpoint.ToString(),
        ["serverName"] = serverName,
        ["tools"] = tools,
        ["argumentsJson"] = JsonSerializer.Serialize(argumentsData),
        ["serverDesc"] = serverDesc,
        ["token"] = token,
    };
    string serverDetailsJson = JsonSerializer.Serialize(serverDetailsForPrompt, new JsonSerializerOptions { WriteIndented = true });

    string SYS_PROMPT = $"""
    You are an assistant that performs database operations. You MUST follow all instructions and schemas precisely.

    {toolInfoForPrompt}

    You must use the below server details data for the tool call's arguments without omitting or changing any fields, especially the 'arguments' field.

    ---BEGIN SERVER DETAILS FOR TOOL CALL---
    {serverDetailsJson}
    ---END SERVER DETAILS FOR TOOL CALL---
    """;

    var messages = new List<ChatMessage>();
    messages.Add(new SystemChatMessage(SYS_PROMPT));
    while (true)
    {
        Console.WriteLine("Query: ");
        var query = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(query) || query.ToLower() == "quit")
        {
            break;
        }

        messages.Add(new UserChatMessage(query));

        try
        {
            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = 0.1f,
            };

            foreach (var tool in available_tools)
            {
                string rawSchemaJson = tool.JsonSchema.GetRawText();
                var cleanedSchema = PrepareFunctionSchema(rawSchemaJson);
                ChatTool t = ChatTool.CreateFunctionTool(
                    functionName: tool.Name,
                    functionDescription: tool.Description,
                    functionParameters: cleanedSchema
                );
                chatCompletionOptions.Tools.Add(t);
            }

            var completion = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);

            while (completion.Value.ToolCalls.Count > 0)
            {
                messages.Add(new AssistantChatMessage(completion.Value));
                for (int i = 0; i < completion.Value.ToolCalls.Count; i++)
                {
                    string tool_name = completion.Value.ToolCalls[i].FunctionName;

                    var tool_args = completion.Value.ToolCalls[i].FunctionArguments;
                    string argsJson = tool_args.ToString();
                    var argsDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(argsJson);

                    Console.WriteLine($"Tool: {tool_name}\nArgs: {argsJson}");

                    var result = await regClient.CallToolAsync(
                        tool_name,
                        argsDict
                    );

                    messages.Add(new ToolChatMessage(
                                    toolCallId: completion.Value.ToolCalls[i].Id,
                                    content: ((TextContentBlock)result.Content[0]).Text
                                ));
                }

                completion = await chatClient.CompleteChatAsync(messages, chatCompletionOptions);
            }

            var response = completion.Value.Content[0].Text;
            messages.Add(new AssistantChatMessage(response));

            Console.WriteLine($"\nResponse: \n{response}\n");
            Console.WriteLine(new string('-', 50));

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

//--------------------------------------------------------------------------------------------------------------------

static BinaryData PrepareFunctionSchema(string rawSchemaJson)
{
    // Parse the original schema
    using JsonDocument doc = JsonDocument.Parse(rawSchemaJson);
    var root = doc.RootElement;

    // Create a new in-memory JSON object
    using var output = new MemoryStream();
    using var writer = new Utf8JsonWriter(output, new JsonWriterOptions { Indented = true });

    writer.WriteStartObject();

    // Iterate over the top-level elements of the schema (like "type", "properties", "required")
    foreach (var property in root.EnumerateObject())
    {
        // Find the "properties" section to modify it
        if (property.Name == "properties" && property.Value.ValueKind == JsonValueKind.Object)
        {
            writer.WritePropertyName("properties");
            writer.WriteStartObject();

            // Iterate over each parameter's definition (like "Endpoint", "serverName", "arguments")
            foreach (var parameter in property.Value.EnumerateObject())
            {
                writer.WritePropertyName(parameter.Name);

                // *** THIS IS THE FIX ***
                // If we find the problematic "arguments" or "newArguments" parameter,
                // we replace its complex schema with a very simple one.
                if (parameter.Name == "arguments" || parameter.Name == "newArguments")
                {
                    writer.WriteStartObject();
                    writer.WriteString("type", "object");
                    writer.WriteString("description", "A dictionary of the tool's functions and their arguments.");
                    writer.WriteEndObject();
                }
                else
                {
                    // For all other parameters, write them as they are.
                    parameter.Value.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
        }
        else
        {
            // For all other top-level elements (like "type", "required"), write them as they are.
            property.WriteTo(writer);
        }
    }

    writer.WriteEndObject();
    writer.Flush();
    return BinaryData.FromBytes(output.ToArray());
}

//-----------------------------------------------------------------------------------------------------------------

var app = builder.Build();
app.MapMcp();
app.Run();
