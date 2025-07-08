using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
// import namespaces
using Azure;
using Azure.AI.Translation.Text;
using Microsoft.Extensions.Configuration;

namespace translate_text
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Set console encoding to UTF-8
                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;

                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                                                          .AddUserSecrets<Program>();
                IConfigurationRoot configuration = builder.Build();
                string translatorRegion = configuration["TranslatorRegion"];
                string translatorKey = configuration["TranslatorKey"];

                // Create client using endpoint and key
                AzureKeyCredential credential = new(translatorKey);
                TextTranslationClient client = new(credential, translatorRegion);

                // Choose target language
                Response<GetSupportedLanguagesResult> languagesResponse = await client.GetSupportedLanguagesAsync(scope: "translation").ConfigureAwait(false);
                GetSupportedLanguagesResult languages = languagesResponse.Value;
                Console.WriteLine($"{languages.Translation.Count} languages available.\n(See https://learn.microsoft.com/azure/ai-services/translator/language-support#translation)");
                Console.WriteLine("Enter a target language code for translation (for example, 'en'):");
                string targetLanguage = "xx";
                bool languageSupported = false;
                while (!languageSupported)
                {
                    targetLanguage = Console.ReadLine();
                    if (languages.Translation.ContainsKey(targetLanguage))
                    {
                        languageSupported = true;
                    }
                    else
                    {
                        Console.WriteLine($"{targetLanguage} is not a supported language.");
                    }

                }

                // Translate text
                string inputText = "";
                while (inputText.ToLower() != "quit")
                {
                    Console.WriteLine("Enter text to translate ('quit' to exit)");
                    inputText = Console.ReadLine();
                    if (inputText.ToLower() != "quit")
                    {
                        Response<IReadOnlyList<TranslatedTextItem>> translationResponse = await client.TranslateAsync(targetLanguage, inputText).ConfigureAwait(false);
                        IReadOnlyList<TranslatedTextItem> translations = translationResponse.Value;
                        TranslatedTextItem translation = translations[0];
                        string sourceLanguage = translation?.DetectedLanguage?.Language;
                        Console.WriteLine($"'{inputText}' translated from {sourceLanguage} to {translation?.Translations[0].TargetLanguage} as '{translation?.Translations?[0]?.Text}'.");
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
