using Newtonsoft.Json;
using System.Reflection;
using System.Diagnostics;

namespace vss
{
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

        public static Dictionary<string, MagicSearch> LoadMagicSearches()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

            if (!File.Exists(configPath))
            {
                ExtractDefaultConfig(configPath);
            }

            string json = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<Dictionary<string, MagicSearch>>(json);
        }
        public static void ListEmbeddedResources()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Debug.WriteLine("Listing Embedded Resources:");
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                Debug.WriteLine(resourceName);
            }
        }

        private static void ExtractDefaultConfig(string outputPath)
        {
            // Access the embedded resource
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "vss.Resources.config.json";  // Adjust the namespace and resource name accordingly
            // ListEmbeddedResources();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                File.WriteAllText(outputPath, result);
            }
        }
    }

    public class MagicSearch
    {
        [JsonProperty("expanded_name")]
        public string ExpandedName { get; set; }
        [JsonProperty("command")]
        public string Command { get; set; }
    }
}
