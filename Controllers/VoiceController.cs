using Microsoft.AspNetCore.Mvc;
using CookingWithVoice.Services;
using CookingWithVoice.Models;

namespace CookingWithVoice.Controllers
{
    [Route("api/voice")]
    [ApiController]
    public class VoiceController : ControllerBase
    {
        private readonly IOpenAIService _openAIService;
        private readonly IRecipeService _recipeService;
        private readonly IRecipeSearchService _recipeSearchService;
        private static Recipe? _currentRecipe;
        private static int _currentStepIndex = 0;

        public VoiceController(IOpenAIService openAIService, IRecipeService recipeService, IRecipeSearchService recipeSearchService)
        {
            _openAIService = openAIService;
            _recipeService = recipeService;
            _recipeSearchService = recipeSearchService;
        }

        [HttpPost("transcribe")]
        public async Task<IActionResult> TranscribeAudio([FromForm] IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest(new { error = "No audio file provided." });
            }

            try
            {
                // Validate audio file
                if (audioFile.Length > 25 * 1024 * 1024) // 25MB limit
                {
                    return BadRequest(new { error = "Audio file too large. Maximum size is 25MB." });
                }

                string transcription;
                try
                {
                    transcription = await _openAIService.TranscribeAudioAsync(audioFile);
                }
                catch (Exception apiEx)
                {
                    // Log the actual error for debugging
                    Console.WriteLine($"OpenAI API Error: {apiEx.Message}");
                    
                    // Return a helpful error message
                    return StatusCode(500, new { 
                        error = "Voice transcription failed", 
                        message = "Please check your OpenAI API key and try again. You can also type your command instead.",
                        details = apiEx.Message
                    });
                }

                if (string.IsNullOrWhiteSpace(transcription))
                {
                    return BadRequest(new { error = "No speech detected. Please try speaking more clearly." });
                }

                var response = await ProcessVoiceCommand(transcription);
                return Ok(new { 
                    responseText = response.ResponseText,
                    action = response.Action,
                    recipeData = response.RecipeData,
                    searchResults = response.SearchResults,
                    currentStepIndex = response.CurrentStepIndex,
                    timerDuration = response.TimerDuration,
                    transcription = transcription // Include what was heard for debugging
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Voice Controller Error: {ex.Message}");
                return StatusCode(500, new { error = "Processing failed.", message = ex.Message });
            }
        }

        private async Task<AssistantResponse> ProcessVoiceCommand(string command)
        {
            var lowerCommand = command.ToLower();
            
            // Recipe search commands
            if (lowerCommand.Contains("find") || lowerCommand.Contains("search") || lowerCommand.Contains("recipe for"))
            {
                var query = ExtractRecipeQuery(lowerCommand);
                var searchResults = await _recipeSearchService.SearchRecipesAsync(query);
                
                return new AssistantResponse
                {
                    ResponseText = $"I found {searchResults.Count} recipes for {query}. Which one would you like to cook?",
                    Action = "show_search_results",
                    SearchResults = searchResults
                };
            }
            
            // Recipe selection commands
            if (lowerCommand.Contains("cook") || lowerCommand.Contains("make") || lowerCommand.Contains("start"))
            {
                var query = ExtractRecipeQuery(lowerCommand);
                var searchResults = await _recipeSearchService.SearchRecipesAsync(query);
                
                if (searchResults.Any())
                {
                    var selectedResult = searchResults.First();
                    _currentRecipe = await _recipeSearchService.ConvertToRecipeAsync(selectedResult);
                    _currentStepIndex = 0;
                    
                    return new AssistantResponse
                    {
                        ResponseText = $"Great! Let's cook {_currentRecipe.Title}. Here are the ingredients you'll need, then I'll guide you step by step.",
                        Action = "show_recipe",
                        RecipeData = _currentRecipe,
                        CurrentStepIndex = _currentStepIndex
                    };
                }
            }
            
            // Step navigation commands
            if (_currentRecipe != null)
            {
                if (lowerCommand.Contains("next") || lowerCommand.Contains("continue"))
                {
                    if (_currentStepIndex < _currentRecipe.Instructions.Count - 1)
                    {
                        _currentStepIndex++;
                        return new AssistantResponse
                        {
                            ResponseText = $"Step {_currentStepIndex + 1}: {_currentRecipe.Instructions[_currentStepIndex]}",
                            Action = "update_step",
                            RecipeData = _currentRecipe,
                            CurrentStepIndex = _currentStepIndex
                        };
                    }
                    else
                    {
                        return new AssistantResponse
                        {
                            ResponseText = "Congratulations! You've completed the recipe. Your dish is ready to enjoy!",
                            Action = "recipe_complete"
                        };
                    }
                }
                
                if (lowerCommand.Contains("previous") || lowerCommand.Contains("back"))
                {
                    if (_currentStepIndex > 0)
                    {
                        _currentStepIndex--;
                        return new AssistantResponse
                        {
                            ResponseText = $"Going back to step {_currentStepIndex + 1}: {_currentRecipe.Instructions[_currentStepIndex]}",
                            Action = "update_step",
                            RecipeData = _currentRecipe,
                            CurrentStepIndex = _currentStepIndex
                        };
                    }
                }
                
                if (lowerCommand.Contains("repeat") || lowerCommand.Contains("again"))
                {
                    return new AssistantResponse
                    {
                        ResponseText = $"Step {_currentStepIndex + 1}: {_currentRecipe.Instructions[_currentStepIndex]}",
                        Action = "repeat_step",
                        RecipeData = _currentRecipe,
                        CurrentStepIndex = _currentStepIndex
                    };
                }
                
                if (lowerCommand.Contains("ingredients"))
                {
                    var ingredientsList = string.Join(", ", _currentRecipe.Ingredients);
                    return new AssistantResponse
                    {
                        ResponseText = $"Here are the ingredients for {_currentRecipe.Title}: {ingredientsList}",
                        Action = "show_ingredients",
                        RecipeData = _currentRecipe
                    };
                }
            }
            
            // Timer commands
            if (lowerCommand.Contains("timer") && lowerCommand.Contains("minute"))
            {
                var timerMatch = System.Text.RegularExpressions.Regex.Match(lowerCommand, @"(\d+)\s*minute");
                if (timerMatch.Success)
                {
                    var minutes = timerMatch.Groups[1].Value;
                    return new AssistantResponse
                    {
                        ResponseText = $"Setting a timer for {minutes} minutes.",
                        Action = "set_timer",
                        TimerDuration = $"{minutes} minutes"
                    };
                }
            }
            
            // List saved recipes
            if (lowerCommand.Contains("my recipes") || lowerCommand.Contains("saved recipes"))
            {
                var savedRecipes = await _recipeService.GetAllRecipesAsync();
                var savedList = savedRecipes.Where(r => r.IsSaved).ToList();
                
                if (savedList.Any())
                {
                    var recipeNames = string.Join(", ", savedList.Select(r => r.Title));
                    return new AssistantResponse
                    {
                        ResponseText = $"You have {savedList.Count} saved recipes: {recipeNames}. Which one would you like to cook?",
                        Action = "show_saved_recipes"
                    };
                }
                else
                {
                    return new AssistantResponse
                    {
                        ResponseText = "You don't have any saved recipes yet. Try searching for a recipe first!",
                        Action = "info"
                    };
                }
            }
            
            // Default response
            return new AssistantResponse
            {
                ResponseText = "I heard: " + command + ". Try saying 'find chocolate cake recipe' to search for recipes, or 'next step' if you're cooking.",
                Action = "info"
            };
        }
        
        private string ExtractRecipeQuery(string command)
        {
            // Remove common command words to extract the recipe name
            var query = command
                .Replace("find", "")
                .Replace("search", "")
                .Replace("recipe for", "")
                .Replace("cook", "")
                .Replace("make", "")
                .Replace("start", "")
                .Trim();
            
            return string.IsNullOrEmpty(query) ? "recipe" : query;
        }
        
        [HttpGet("test-api")]
        public IActionResult TestApiKey()
        {
            try
            {
                var apiKey = _openAIService.GetType().GetField("_apiKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_openAIService) as string;
                var hasKey = !string.IsNullOrEmpty(apiKey);
                var keyPreview = hasKey ? $"{apiKey.Substring(0, 7)}...{apiKey.Substring(apiKey.Length - 4)}" : "No key";
                
                return Ok(new { 
                    hasApiKey = hasKey,
                    keyPreview = keyPreview,
                    message = hasKey ? "API key is configured" : "API key is missing"
                });
            }
            catch (Exception ex)
            {
                return Ok(new { hasApiKey = false, message = ex.Message });
            }
        }
        
        [HttpPost("process-text")]
        public async Task<IActionResult> ProcessTextCommand([FromBody] TextCommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Command))
            {
                return BadRequest(new { error = "Command is required." });
            }

            try
            {
                var response = await ProcessVoiceCommand(request.Command);
                return Ok(new { 
                    responseText = response.ResponseText,
                    action = response.Action,
                    recipeData = response.RecipeData,
                    searchResults = response.SearchResults,
                    currentStepIndex = response.CurrentStepIndex,
                    timerDuration = response.TimerDuration
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Processing failed.", message = ex.Message });
            }
        }
    }
    
    public class TextCommandRequest
    {
        public string Command { get; set; } = string.Empty;
    }
}