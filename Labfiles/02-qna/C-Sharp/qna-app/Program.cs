using System;
using System.IO;
using System.Text;
// import namespaces
using Azure;
using Azure.AI.Language.QuestionAnswering;
using Microsoft.Extensions.Configuration;

namespace qna_app
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddUserSecrets<Program>();
                IConfigurationRoot configuration = builder.Build();
                string aiSvcEndpoint = configuration["AIServicesEndpoint"];
                string aiSvcKey = configuration["AIServicesKey"];
                string projectName = configuration["QAProjectName"];
                string deploymentName = configuration["QADeploymentName"];

                // Create client using endpoint and key
                AzureKeyCredential credentials = new AzureKeyCredential(aiSvcKey);
                Uri endpoint = new Uri(aiSvcEndpoint);
                QuestionAnsweringClient aiClient = new QuestionAnsweringClient(endpoint, credentials);

                // Submit a question and display the answer
                string user_question = "";
                while (true)
                {
                    Console.Write("Question: ");
                    user_question = Console.ReadLine();
                    if (user_question.ToLower() == "quit")
                        break;
                    QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);
                    Response<AnswersResult> response = aiClient.GetAnswers(user_question, project);
                    foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                    {
                        Console.WriteLine(answer.Answer);
                        Console.WriteLine($"Confidence: {answer.Confidence:P2}");
                        Console.WriteLine($"Source: {answer.Source}");
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



    }
}
