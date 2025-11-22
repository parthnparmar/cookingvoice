using System.Text.Json;
using System.Net.Http.Headers;

namespace CookingWithVoice.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string WhisperEndpoint = "https://api.openai.com/v1/audio/transcriptions";

        public OpenAIService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key not configured.");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> TranscribeAudioAsync(IFormFile audioFile)
        {
            try
            {
                if (audioFile == null || audioFile.Length == 0)
                    throw new ArgumentException("Audio file is empty or null");

                using var content = new MultipartFormDataContent();
                
                // Add the audio file
                var streamContent = new StreamContent(audioFile.OpenReadStream());
                
                // Set proper content type for audio
                var contentType = audioFile.ContentType;
                if (string.IsNullOrEmpty(contentType) || contentType == "application/octet-stream")
                {
                    contentType = "audio/webm"; // Default for web recordings
                }
                
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                content.Add(streamContent, "file", audioFile.FileName ?? "audio.webm");

                // Add required model parameter
                content.Add(new StringContent("whisper-1"), "model");
                
                // Add language hint for better accuracy
                content.Add(new StringContent("en"), "language");

                var response = await _httpClient.PostAsync(WhisperEndpoint, content);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"OpenAI API error ({response.StatusCode}): {responseContent}");
                }

                // Parse the JSON response
                using var doc = JsonDocument.Parse(responseContent);
                var text = doc.RootElement.GetProperty("text").GetString();
                
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException("No transcription text received from OpenAI");
                }
                
                return text.Trim();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse OpenAI response", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"OpenAI API request failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Transcription failed: {ex.Message}", ex);
            }
        }
    }
}