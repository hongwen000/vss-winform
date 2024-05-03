using Newtonsoft.Json;

public static class ScoringLogic
{
    public static int GetCharacterMatchScore(string search, string title)
    {
        var searchCounts = search.GroupBy(c => c).ToDictionary(gr => gr.Key, gr => gr.Count());
        var titleCounts = title.GroupBy(c => c).ToDictionary(gr => gr.Key, gr => gr.Count());

        int score = 0;
        foreach (var kvp in searchCounts)
        {
            if (titleCounts.TryGetValue(kvp.Key, out int titleCount))
            {
                score += Math.Min(kvp.Value, titleCount);
            }
        }
        return score;
    }

    public static string RemoveVSCodePostfix(string title)
    {
        string[] parts = title.Split(new[] { " - " }, StringSplitOptions.None);
        if (parts.Length > 3)
        {
            parts = parts.Take(parts.Length - 2).ToArray();
        }
        else
        {
            parts = parts.Take(parts.Length - 1).ToArray();
        }
        return string.Join(" - ", parts);
    }
    public static Dictionary<string, string> LoadMagicSearches()
    {
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        if (File.Exists(configPath))
        {
            string json = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        return new Dictionary<string, string>();
    }
}