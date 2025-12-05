using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClipSpeak
{
    public class TtsClient
    {
        private readonly HttpClient _httpClient;

        public string ApiUrl { get; set; } = "http://localhost:8880/v1/audio/speech"; // Default Kokoro/OpenAI compatible endpoint
        public string ApiKey { get; set; } = ""; // Optional

        public TtsClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<System.Collections.Generic.List<string>> GetVoicesAsync()
        {
            if (string.IsNullOrWhiteSpace(ApiUrl))
                return new System.Collections.Generic.List<string>();

            // Try to guess the voices endpoint. 
            // If ApiUrl is .../v1/audio/speech, maybe voices is .../v1/audio/voices or .../v1/models
            // For Kokoro-FastAPI, it might be /v1/voices
            
            string baseUrl = ApiUrl;
            if (baseUrl.EndsWith("/speech"))
            {
                baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf("/speech"));
                // now .../v1/audio
            }
            
            // Try to derive the voices endpoint.
            // The OpenAPI spec confirms /v1/audio/voices is the correct endpoint.
            // If the user provided .../v1/audio/speech, we want .../v1/audio/voices
            
            string voicesUrl;
            if (ApiUrl.EndsWith("/speech"))
            {
                voicesUrl = ApiUrl.Substring(0, ApiUrl.LastIndexOf("/speech")) + "/voices";
            }
            else
            {
                // Fallback: try to append /v1/audio/voices to the root authority
                var uri = new Uri(ApiUrl);
                voicesUrl = $"{uri.Scheme}://{uri.Authority}/v1/audio/voices";
            }

            try 
            {
                if (!string.IsNullOrEmpty(ApiKey))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
                }

                var response = await _httpClient.GetAsync(voicesUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    // Expecting { "voices": ["a", "b"] } or [ "a", "b" ] or { "data": [ { "id": "a" } ] }
                    
                    // Expecting { "voices": ["a", "b"] } or just a list ["a", "b"]
                    // The spec says it returns a JSON, usually a list of strings or objects.
                    
                    var voices = new System.Collections.Generic.List<string>();
                    dynamic data = JsonConvert.DeserializeObject(json);
                    
                    if (data is Newtonsoft.Json.Linq.JArray)
                    {
                        foreach (var item in data) voices.Add(item.ToString());
                    }
                    else if (data.voices != null)
                    {
                        foreach (var item in data.voices) voices.Add(item.ToString());
                    }
                    else if (data is Newtonsoft.Json.Linq.JObject)
                    {
                        // Sometimes it might be a dictionary where keys are voice names
                        foreach (var property in ((Newtonsoft.Json.Linq.JObject)data).Properties())
                        {
                            voices.Add(property.Name);
                        }
                    }

                    return voices;
                }
            }
            catch 
            {
                // Ignore errors, return empty
            }

            return new System.Collections.Generic.List<string>();
        }

        public async Task<Stream> GetAudioAsync(string text, string voice, float speed = 1.0f)
        {
            if (string.IsNullOrWhiteSpace(ApiUrl))
                throw new InvalidOperationException("API URL is not configured.");

            var requestData = new
            {
                model = "kokoro", // Often required by OpenAI-compatible APIs
                input = text,
                voice = voice,
                response_format = "mp3", // or wav, depending on server
                speed = speed
            };

            var json = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            if (!string.IsNullOrEmpty(ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
            }

            var response = await _httpClient.PostAsync(ApiUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"TTS API request failed: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsStreamAsync();
        }
    }
}
