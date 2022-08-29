class LanguageBatch
{
    public Dictionary<string, Language> Translation { get; set; }
}

class Language
{
    public string Name { get; set; }
    public string NativeName { get; set; }
    public string DIR { get; set; }
}