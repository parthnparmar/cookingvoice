// CookingWithVoice/Services/IOpenAIService.cs
using Microsoft.AspNetCore.Http; // for IFormFile
using System.Text.Json;
using System.Net.Http.Headers;

namespace CookingWithVoice.Services
{
    public interface IOpenAIService
    {
        Task<string> TranscribeAudioAsync(IFormFile audioFile);
    }
}
