using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

// Add references
using Azure.Identity;
using Azure.AI.Projects;
using OpenAI.Chat;

// Clear the console
Console.Clear();

try
{
    // Get configuration settings
    var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                            .AddUserSecrets<Program>();

    IConfigurationRoot configuration = builder.Build();
    string project_connection = configuration["PROJECT_ENDPOINT"];
    string model_deployment = configuration["MODEL_DEPLOYMENT"];

    // Initialize the project client
    DefaultAzureCredentialOptions options = new()
    {
        ExcludeEnvironmentCredential = true,
        ExcludeManagedIdentityCredential = true
    };
    var projectClient = new AIProjectClient(new Uri(project_connection), new DefaultAzureCredential(options));


    // Get a chat client
    ChatClient openaiClient = projectClient.GetAzureOpenAIChatClient(deploymentName: model_deployment, connectionName: null, apiVersion: "2024-10-21");

    // Initialize prompts
    string system_message = "You are an AI assistant for a produce supplier company.";
    string prompt = "";

    // Loop until the user types 'quit'
    while (prompt.ToLower() != "quit")
    {
        // Get user input
        Console.WriteLine("\nAsk a question about the audio\n(or type 'quit' to exit)\n");
        prompt = Console.ReadLine().ToLower();
        if (prompt == "quit")
        {
            break;
        }
        else if (prompt.Length < 1)
        {
            Console.WriteLine("Please enter a question.\n");
            continue;
        }
        else
        {
            Console.WriteLine("Getting a response ...\n");

            // Encode the audio file
            // string filePath = "https://github.com/MicrosoftLearning/mslearn-ai-language/raw/refs/heads/main/Labfiles/09-audio-chat/data/avocados.mp3";
            // Encode teh audio file
            string filePath = "https://github.com/MicrosoftLearning/mslearn-ai-language/raw/refs/heads/main/Labfiles/09-audio-chat/data/fresas.mp3";
            using HttpClient client = new();
            byte[] audioFileRawBytes = await client.GetByteArrayAsync(filePath);
            BinaryData audioData = BinaryData.FromBytes(audioFileRawBytes);

            // Get a response to audio input
            List<ChatMessage> messages =
            [
               new SystemChatMessage(system_message),
               new UserChatMessage(ChatMessageContentPart.CreateTextPart(prompt)
                                   , ChatMessageContentPart.CreateInputAudioPart(audioData, ChatInputAudioFormat.Mp3)),
            ];
            ChatCompletion completion = openaiClient.CompleteChat(messages);
            Console.WriteLine(completion.Content[0].Text);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
