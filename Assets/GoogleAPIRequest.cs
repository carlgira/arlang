using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class GoogleAPIResponse
{
    public List<Candidate> candidates { get; set; }
    public UsageMetadata usageMetadata { get; set; }
}

[Serializable]
public class Candidate
{
    public Content content { get; set; }
    public List<SafetyRating> safetyRatings { get; set; }
}

[Serializable]
public class Content
{
    public string role { get; set; }
    public List<Part> parts { get; set; }
}

[Serializable]
public class Part
{
    public string text { get; set; }
}

[Serializable]
public class SafetyRating
{
    public string category { get; set; }
    public string probability { get; set; }
    public double probabilityScore { get; set; }
    public string severity { get; set; }
    public double severityScore { get; set; }
}

[Serializable]
public class UsageMetadata
{
    public int promptTokenCount { get; set; }
    public int candidatesTokenCount { get; set; }
    public int totalTokenCount { get; set; }
}



[Serializable]
public class Phrase
{
    public string original_phrase { get; set; }

    public string translated_phrase { get; set; }

    public List<Word> words { get; set; }
}

[Serializable]
public class Word
{
    public string kanji { get; set; }
    public string hiragana { get; set; }
    public string translation { get; set; }
}


public class GoogleAPIRequest : MonoBehaviour
{

    public async Task<Phrase> SendRequestWithImageData(string imageData)
    {
        string jsonRequest = @"{
        ""contents"": [
            {
                ""role"": ""user"",
                ""parts"": [
                    {
                        ""inlineData"": {
                            ""mimeType"": ""image/jpeg"",
                            ""data"": """ + imageData + @"""
                        }
                    },
                    {
                        ""text"": ""Extract the Japanese phrase from the given image. Translate the entire phrase to English. Additionally, provide each word in the phrase along with its hiragana and English translation. The response should be in JSON format.\n\nExample JSON format:\n{\n   \""original_phrase\"": \""<Japanese Phrase>\"",\n   \""translated_phrase\"": \""<Translated English Phrase>\"",\n   \""words\"": [\n       {\n           \""kanji\"": \""<Kanji Word>\"",\n           \""hiragana\"": \""<Hiragana Word>\"",\n           \""translation\"": \""<English Translation>\""\n       },\n       ...\n   ]\n}\n\nExample JSON response:\n{\n   \""original_phrase\"": \""私は日本語を勉強しています。\"",\n   \""translated_phrase\"": \""I am studying Japanese.\"",\n   \""words\"": [\n       {\n           \""kanji\"": \""私\"",\n           \""hiragana\"": \""わたし\"",\n           \""translation\"": \""I\""\n       },\n       {\n           \""kanji\"": \""は\"",\n           \""hiragana\"": \""は\"",\n           \""translation\"": \""(topic marker)\""\n       },\n       {\n           \""kanji\"": \""日本語\"",\n           \""hiragana\"": \""にほんご\"",\n           \""translation\"": \""Japanese (language)\""\n       },\n       {\n           \""kanji\"": \""を\"",\n           \""hiragana\"": \""を\"",\n           \""translation\"": \""(object marker)\""\n       },\n       {\n           \""kanji\"": \""勉強\"",\n           \""hiragana\"": \""べんきょう\"",\n           \""translation\"": \""study\""\n       },\n       {\n           \""kanji\"": \""して\"",\n           \""hiragana\"": \""して\"",\n           \""translation\"": \""doing\""\n       },\n       {\n           \""kanji\"": \""います\"",\n           \""hiragana\"": \""います\"",\n           \""translation\"": \""am (present continuous)\""\n       }\n   ]\n}""
                    }
                ]
            }
        ],
        ""generationConfig"": {
            ""maxOutputTokens"": 8192,
            ""temperature"": 0,
            ""topP"": 0.95,
            ""response_mime_type"": ""application/json""
        }
    }";

        return await SendRequest(jsonRequest);
    }

    public async Task<string> SendRequestWithText(string text)
    {
        string escapedText = JsonConvert.ToString(text).Trim('"');
        string jsonRequest = $@"{{
            ""contents"": [
                {{
                ""role"": ""user"",
                ""parts"": [
                    {{
                    ""text"": ""Text: {escapedText}\n\nPlease provide a response that includes: \n0. By no means add comments or formating to the response, simply output a single paragrah with no html at all \n1. A short, human-readable explanation of the words and kanjis used in the Japanese text, aimed at students learning Japanese. Write the text to be used for someone listening to an audio.""
                    }}
                ]
                }}
            ],
            ""generationConfig"": {{
                ""maxOutputTokens"": 512,
                ""temperature"": 0,
                ""topP"": 0.95
            }}
            }}";

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Config.GEMINI_API_KEY}");
            StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(Config.GEMINI_API_URL, content);

            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                List<GoogleAPIResponse> responses = JsonConvert.DeserializeObject<List<GoogleAPIResponse>>(responseBody);
                string cc = GetTextResponse(responses);
                return cc;
            }
            else
            {
                string errorMessage = $"Error: {response.StatusCode}\nResponse Body: {responseBody}";
                Debug.LogError(errorMessage);
                return null;
            }
        }
    }

    private async Task<Phrase> SendRequest(string jsonRequest)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Config.GEMINI_API_KEY}");
            StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(Config.GEMINI_API_URL, content);

            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                List<GoogleAPIResponse> responses = JsonConvert.DeserializeObject<List<GoogleAPIResponse>>(responseBody);
                string cc = GetTextResponse(responses);
                Phrase phrase = JsonConvert.DeserializeObject<Phrase>(cc);
                return phrase;
            }
            else
            {
                string errorMessage = $"Error: {response.StatusCode}\nResponse Body: {responseBody}";
                Debug.LogError(errorMessage);
                return null;
            }
        }
    }

    private string GetTextResponse(List<GoogleAPIResponse> responses)
    {
        string result = "";
        foreach (var res in responses)
        {
            foreach (var candidate in res.candidates)
            {
                foreach (var part in candidate.content.parts)
                {
                    result += part.text;
                }
            }
        }
        return result;
    }
}