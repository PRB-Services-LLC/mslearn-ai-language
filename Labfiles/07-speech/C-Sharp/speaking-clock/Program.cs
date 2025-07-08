using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

// Import namespaces
using Azure.Identity;
using Azure.AI.Projects;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;


namespace speaking_clock
{
    class Program
    {
        private static SpeechConfig speechConfig;

        static async Task Main(string[] args)
        {

            // Clear the console
            Console.Clear();

            try
            {
                // Get config settings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddUserSecrets<Program>();
                IConfigurationRoot configuration = builder.Build();
                string projectKey = configuration["PROJECT_KEY"];
                string location = configuration["LOCATION"];

                // Configure speech service
                speechConfig = SpeechConfig.FromSubscription(projectKey, location);
                Console.WriteLine("Ready to use speech service in " + speechConfig.Region);


                // Get spoken input
                string command = "";
                command = await TranscribeCommand();
                if (command.ToLower()=="what time is it?")
                {
                    await TellTime();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task<string> TranscribeCommand()
        {
            string command = "";

            // Configure speech recognition
            string audioFile = "time.wav";
            using AudioConfig audioConfig = AudioConfig.FromWavFileInput(audioFile);
            using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            // Process speech input
            Console.WriteLine("Listening...");
            SpeechRecognitionResult speech = await speechRecognizer.RecognizeOnceAsync();
            if (speech.Reason == ResultReason.RecognizedSpeech)
            {
                command = speech.Text;
                Console.WriteLine(command);
            }
            else
            {
                Console.WriteLine(speech.Reason);
                if (speech.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(speech);
                    Console.WriteLine(cancellation.Reason);
                    Console.WriteLine(cancellation.ErrorDetails);
                }
            }

            // Return the command
            return command;
        }

        static async Task TellTime()
        {
            var now = DateTime.Now;
            string responseText = "The time is " + now.Hour.ToString() + ":" + now.Minute.ToString("D2");

            // Configure speech synthesis
            var outputFile = "output.wav";
            speechConfig.SpeechSynthesisVoiceName = "en-GB-RyanNeural";
            using var audioConfig = AudioConfig.FromWavFileOutput(outputFile);
            using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

            // Synthesize spoken output
            // SpeechSynthesisResult speak = await speechSynthesizer.SpeakTextAsync(responseText);
            // if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
            // {
            //     Console.WriteLine(speak.Reason);
            // }
            // else
            // {
            //     Console.WriteLine("Spoken output saved in " + outputFile);
            // }
            // Synthesize spoken output
            string responseSsml = $@"
   <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
       <voice name='en-GB-LibbyNeural'>
           {responseText}
           <break strength='weak'/>
           Time to end this lab!
       </voice>
   </speak>";
            SpeechSynthesisResult speak = await speechSynthesizer.SpeakSsmlAsync(responseSsml);
            if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                Console.WriteLine(speak.Reason);
            }
            else
            {
                Console.WriteLine("Spoken output saved in " + outputFile);
            }


            // Print the response
            Console.WriteLine(responseText);
        }

    }
}
