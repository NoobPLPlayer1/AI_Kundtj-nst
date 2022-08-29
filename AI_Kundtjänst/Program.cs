using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Azure;
using Azure.AI.TextAnalytics;
using System.Net.Http.Headers;
using System.Xml.Linq;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Models;
using Microsoft.Bot.Builder;
using Azure.AI.Language.QuestionAnswering;

IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile(Path.GetFullPath("appsettings.json"));
IConfigurationRoot configuration = builder.Build();
string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
string cogSvcKey = configuration["CognitiveServiceKey"];

Console.InputEncoding = Console.OutputEncoding = Encoding.Unicode;

AzureKeyCredential cogCredentials = new AzureKeyCredential(cogSvcKey);
Uri cogEndpoint = new Uri(cogSvcEndpoint);
TextAnalyticsClient cogClient = new TextAnalyticsClient(cogEndpoint, cogCredentials);

using Translator translator = new(configuration["TranslateServiceKey"]);
await translator.Initilize();

Uri languageEndpoint = new Uri(configuration["LanguageServicesEndpoint"]);
AzureKeyCredential credential = new AzureKeyCredential(configuration["LanguageServiceKey"]);
string projectName = "FAQLabb";
string deploymentName = "test";

QuestionAnsweringClient qnaClient = new QuestionAnsweringClient(languageEndpoint, credential);
QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

HashSet<string> spokenLanguages = new();

string? input;
while ((input = PrefixReadline("You")) != "exit")
{
    if(input != "")
    {
        var inputResult = await translator.Translate("en", input);

        string inputLanguage = inputResult[0].DetectedLanguage.Language;
        spokenLanguages.Add(inputLanguage);
        Response<AnswersResult> response = qnaClient.GetAnswers(inputResult[0].Translations[0].Text, project);

        foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
        {
            answer.Metadata.TryGetValue("special_output", out var specialAnswerType);
            switch (specialAnswerType)
            {
                case "lang_count":
                    var langResult = await translator.Translate(inputLanguage, $"We have spoken {spokenLanguages.Count} languages!");
                    Console.WriteLine(langResult[0].Translations[0].Text);

                    break;
                default:
                    var outputResult = await translator.Translate(inputLanguage, answer.Answer);
                    Console.Write($"Bot: ");
                    Console.WriteLine(outputResult[0].Translations[0].Text);
                    break;
            }
        }
    }
}

string PrefixReadline(string prefix)
{
    Console.Write($"{prefix}: ");
    return Console.ReadLine()!;
}