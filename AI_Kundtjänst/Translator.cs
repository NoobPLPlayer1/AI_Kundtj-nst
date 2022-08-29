using System.Text;
using Newtonsoft.Json;

class Translator : IDisposable
{
    HttpClient client = new();
    Dictionary<string, string> LanguageCode = new();

    public string SubscriptionKey { get; }
    public string Url { get; init; } = "https://api.cognitive.microsofttranslator.com";
    public string LanguageUrl { get; init; } = "https://api.cognitive.microsofttranslator.com/languages";

    public Translator(string key)
    {
        SubscriptionKey = key;

        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
    }

    public async Task Initilize()
    {
        var response = await client.GetAsync(LanguageUrl + $"?api-version=3.0");

        if (!response.IsSuccessStatusCode)
            Console.WriteLine("Error: Could not get language availabel languages!");
        else
        {
            var result = await response.Content.ReadAsStringAsync();
            Dictionary<string, Language> translation = JsonConvert.DeserializeObject<LanguageBatch>(result).Translation;
            foreach (var kvp in translation)
            {
                LanguageCode[kvp.Key] = kvp.Key; // Ifall en språk kod skulle komma in
                LanguageCode[kvp.Value.Name] = kvp.Key;
                LanguageCode[kvp.Value.NativeName] = kvp.Key;
            }
        }
    }

    public async Task<TranslationInfo[]> Translate(string language, params string[] texts) => JsonConvert.DeserializeObject<TranslationInfo[]>(await Process("translate", $"&to={LanguageCode[language]}", texts));
    public async Task<DetectedLanguage[]> DetectLanguage(params string[] texts) => JsonConvert.DeserializeObject<DetectedLanguage[]>(await Process("detect", "", texts));

    async Task<string> Process(string type, string args, params string[] texts)
    {
        TextBlock[] items = new TextBlock[texts.Length];
        for (int k = 0; k < texts.Length; k++)
            items[k].Text = texts[k];
        using HttpRequestMessage request = new();
        request.Method = HttpMethod.Post;
        request.RequestUri = new(Url + $"/{type}?api-version=3.0{args}");
        request.Content = new StringContent(JsonConvert.SerializeObject(items), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error: {result}");
            return result;
        }

        return result;
    }

    public void Dispose()
    {
        client.Dispose();
    }

    public struct TextBlock
    {
        public string Text { get; set; }
    }

    public struct DetectedLanguage
    {
        public string Language { get; set; }
        public double Score { get; set; }
        //public bool IsTranslationSupported { get; set; }
        //public bool IsTransliterationSupported { get; set; }
    }

    public struct TranslationInfo
    {
        public DetectedLanguage DetectedLanguage { get; set; }
        public TextBlock[] Translations { get; set; }
    }
}