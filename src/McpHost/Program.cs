using Azure;
using Azure.AI.OpenAI;
using McpHost.prompts;
using McpHost.Utilities;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

//Initializing llm Client --------------------------------------------------------------------------------------------------------------------------
var prompts = new SystemPrompts();

var azureClient = new AzureOpenAIClient(
                        new Uri(configuration["AZURE_OPENAI_ENDPOINT"]),
                        new ApiKeyCredential(configuration["AZURE_OPENAI_KEY"])
                    );
var llmClient = azureClient.GetChatClient(configuration["AZURE_OPENAI_DEPLOYMENT"]);

//Initializing registry Client --------------------------------------------------------------------------------------------------------------------------

var clientTransport = new SseClientTransport(new SseClientTransportOptions
{
    Endpoint = new Uri(configuration["REGISTRY_SERVER_ENDPOINT"]),
});
var regClient = await McpClientFactory.CreateAsync(clientTransport);

Console.WriteLine("Enter the value based on mode of operation");
Console.WriteLine("Login: 0");
Console.WriteLine("Register: 1");
string mode = Console.ReadLine();


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
    Console.WriteLine(regResponse.Content[0].Text);
}

var authArguments = new Dictionary<string, object?>
{
    { "username", username },
    { "password", pwd }
};

var authResponse = await regClient.CallToolAsync("Login", authArguments);
string token = authResponse.Content[0].Text;

if (!string.IsNullOrEmpty(token) && !token.Contains("error"))
{
    Console.WriteLine("\nLogin Successful!");
}
else
{
    Console.WriteLine($"\nLogin Failed: {token}");
}

//Retrieving Available Server Details --------------------------------------------------------------------------------------------------------------------------

var detailsArguments = new Dictionary<string, object?>
{
    { "token", token }
};

var detailsResponse = await regClient.CallToolAsync("GetAllRunningServers", detailsArguments);
string serverDetails = detailsResponse.Content[0].Text;
prompts.addServerDetails(serverDetails, token);

//Chat Client with User -----------------------------------------------------------------------------------------------------
while(true)
{
    Console.WriteLine("Query: ");
    var query = Console.ReadLine();

    if (query == "quit")
    {
        break;
    }

    var messages = new List<ChatMessage>();
    messages.Add(new SystemChatMessage(prompts.serverPrompt));
    messages.Add(new UserChatMessage(query));

    //getting required server endpoint
    string serverEndpoint = string.Empty;
    try
    {
        var chatCompletionOptions = new ChatCompletionOptions
        {
            Temperature = 0.1f,
        };

        var completion = await llmClient.CompleteChatAsync(messages, chatCompletionOptions);
        messages.Add(new AssistantChatMessage(completion.Value));

        serverEndpoint = completion.Value.Content[0].Text;

        if (string.IsNullOrEmpty(serverEndpoint) || !serverEndpoint.StartsWith("http://"))
        {
            throw new ArgumentException("The provided URL is invalid or not active", nameof(serverEndpoint));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    //Initializing req server Client --------------------------------------------------------------------------------------------------------------------------

    var serverTransport = new SseClientTransport(new SseClientTransportOptions
    {
        Endpoint = new Uri(serverEndpoint),
    });
    
    var serverClient = await McpClientFactory.CreateAsync(serverTransport);
    var available_tools = await serverClient.ListToolsAsync();

    Console.WriteLine($"{serverClient.ServerInfo.Name} serverClient Started!!!");

    //Getting the exact tool to call -------------------------------------------------------------------------------------------------------------------------
    messages = new List<ChatMessage>();
    messages.Add(new SystemChatMessage(prompts.toolPrompt));
    messages.Add(new UserChatMessage(query));

    prompts.addTokenDetails(token);
    string finalResponse = string.Empty;
    try
    {
        var chatCompletionOptions = new ChatCompletionOptions
        {
            Temperature = 0.1f,
        };

        foreach (var tool in available_tools)
        {
            string rawSchemaJson = tool.JsonSchema.GetRawText();
            var cleanedSchema = PrepareFunction.PrepareFunctionSchema(rawSchemaJson);
            ChatTool t = ChatTool.CreateFunctionTool(
                functionName: tool.Name,
                functionDescription: tool.Description,
                functionParameters: cleanedSchema
            );
            chatCompletionOptions.Tools.Add(t);
        }

        var completion = await llmClient.CompleteChatAsync(messages, chatCompletionOptions);
        messages.Add(new AssistantChatMessage(completion.Value));

        finalResponse = await ToolCall(completion, messages, serverClient);

        Console.WriteLine($"\nResponse: \n{finalResponse}\n");
        Console.WriteLine(new string('-', 50));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}



async Task<string> ToolCall(ClientResult<ChatCompletion> completion, List<ChatMessage> messages, IMcpClient serverClient)
{
    string response = string.Empty;
    try
    {
        while (completion.Value.ToolCalls.Count > 0)
        {
            for (int i = 0; i < completion.Value.ToolCalls.Count; i++)
            {
                string tool_name = completion.Value.ToolCalls[i].FunctionName;

                var tool_args = completion.Value.ToolCalls[i].FunctionArguments;
                string argsJson = tool_args.ToString();
                var argsDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(argsJson);

                Console.WriteLine($"\ntool: {tool_name}\n args: {argsJson}");

                var result = await serverClient.CallToolAsync(
                    tool_name,
                    argsDict
                );

                messages.Add(new ToolChatMessage(
                                toolCallId: completion.Value.ToolCalls[i].Id,
                                content: result.Content[0].Text
                            ));
            }

            completion = await llmClient.CompleteChatAsync(messages);
            messages.Add(new AssistantChatMessage(completion.Value));
        }

        response = completion.Value.Content[0].Text;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    return response;
}
