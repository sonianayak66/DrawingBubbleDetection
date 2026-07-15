using System;
using System.Collections.Generic;
using Syn.WordNet;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using NuGet.Packaging;

namespace MPCRS.Utilities
{
    public class SynonymFinder
    {
        public class StopwordsContainer
        {
            public List<string> Stopwords { get; set; }
        }

        public static string[] Find(string userq)
        {
            try
            {
                string[] syns = new string[1];
                var directory = @"C:\phi3\dict"; // Set this to the path of your WordNet database
                var wordNet = new WordNetEngine();
                wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.noun")), PartOfSpeech.Noun);
                wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.verb")), PartOfSpeech.Verb);
                wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.adj")), PartOfSpeech.Adjective);
                wordNet.AddDataSource(new StreamReader(Path.Combine(directory, "data.adv")), PartOfSpeech.Adverb);
                wordNet.Load();
                string userQuery = userq;
                var keywords = ExtractKeywords(userQuery);
                var stringBuilder = new StringBuilder();
                foreach (var keyword in keywords)
                {
                    var synonyms = GetSynonyms(wordNet, keyword);
                    stringBuilder.AppendLine(string.Join(", ", synonyms));
                }
                string resultString = stringBuilder.ToString();
                string[] resultArray = resultString.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                return resultArray;
            }
            catch (Exception ex)
            {
                throw;
            }   
        }

        static List<string> ExtractKeywords(string query)
        {
            var stopwords = LoadStopwordsFromJson(@"wwwroot/OpenAI/stopwords.json");
            var words = query.ToLower()
                             .Split(new char[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                             .Where(word => !stopwords.Contains(word))
                             .ToList();
            return words;
        }

        static HashSet<string> LoadStopwordsFromJson(string filePath)
        {
            string jsonText = File.ReadAllText(filePath);
            StopwordsContainer stopwordsContainer = JsonConvert.DeserializeObject<StopwordsContainer>(jsonText);
            List<string> stopwords = stopwordsContainer.Stopwords;
            return new HashSet<string>(stopwords, StringComparer.OrdinalIgnoreCase);
        }

        static List<string> GetSynonyms(WordNetEngine wordNet, string word)
        {
            var synonyms = new List<string>();
            try
            {
                var synSets = wordNet.GetSynSets(word);
                if (synSets != null && synSets.Any())
                {
                    foreach (var synSet in synSets)
                    {
                        synonyms.AddRange(synSet.Words);
                    }
                }
            }
            catch (KeyNotFoundException ex)
            {
                synonyms.Add(word);
            }
            return synonyms.Distinct().ToList(); // Remove duplicate synonyms
        }
    }
}
