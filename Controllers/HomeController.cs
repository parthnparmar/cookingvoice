using CookingWithVoice.Models;
using CookingWithVoice.Services;
using Microsoft.AspNetCore.Mvc;

namespace CookingWithVoice.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOpenAIService _openAIService;
        private readonly IRecipeService _recipeService;
        private readonly IRecipeSearchService _recipeSearchService;

        // --- Simulated Application State (Must be replaced with proper session/database state management in production) ---
        private static Recipe? CurrentRecipe { get; set; }
        private static int CurrentStepIndex { get; set; } = 0;
        // ------------------------------------------------------------------------------------------------------------------

        public HomeController(IOpenAIService openAIService, IRecipeService recipeService, IRecipeSearchService recipeSearchService)
        {
            _openAIService = openAIService;
            _recipeService = recipeService;
            _recipeSearchService = recipeSearchService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TranscribeAudio([FromForm] IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest(new { error = "No audio file provided." });
            }

            try
            {
                string transcription = await _openAIService.TranscribeAudioAsync(audioFile);
                var response = await ProcessVoiceCommand(transcription);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Transcription failed.", message = ex.Message });
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
                    CurrentRecipe = await _recipeSearchService.ConvertToRecipeAsync(selectedResult);
                    CurrentStepIndex = 0;
                    
                    return new AssistantResponse
                    {
                        ResponseText = $"Great! Let's cook {CurrentRecipe.Title}. Here are the ingredients you'll need, then I'll guide you step by step.",
                        Action = "show_recipe",
                        RecipeData = CurrentRecipe,
                        CurrentStepIndex = CurrentStepIndex
                    };
                }
            }
            
            // Step navigation commands
            if (CurrentRecipe != null)
            {
                if (lowerCommand.Contains("next") || lowerCommand.Contains("continue"))
                {
                    if (CurrentStepIndex < CurrentRecipe.Instructions.Count - 1)
                    {
                        CurrentStepIndex++;
                        return new AssistantResponse
                        {
                            ResponseText = $"Step {CurrentStepIndex + 1}: {CurrentRecipe.Instructions[CurrentStepIndex]}",
                            Action = "update_step",
                            RecipeData = CurrentRecipe,
                            CurrentStepIndex = CurrentStepIndex
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
                    if (CurrentStepIndex > 0)
                    {
                        CurrentStepIndex--;
                        return new AssistantResponse
                        {
                            ResponseText = $"Going back to step {CurrentStepIndex + 1}: {CurrentRecipe.Instructions[CurrentStepIndex]}",
                            Action = "update_step",
                            RecipeData = CurrentRecipe,
                            CurrentStepIndex = CurrentStepIndex
                        };
                    }
                }
                
                if (lowerCommand.Contains("repeat") || lowerCommand.Contains("again"))
                {
                    return new AssistantResponse
                    {
                        ResponseText = $"Step {CurrentStepIndex + 1}: {CurrentRecipe.Instructions[CurrentStepIndex]}",
                        Action = "repeat_step",
                        RecipeData = CurrentRecipe,
                        CurrentStepIndex = CurrentStepIndex
                    };
                }
                
                if (lowerCommand.Contains("ingredients"))
                {
                    var ingredientsList = string.Join(", ", CurrentRecipe.Ingredients);
                    return new AssistantResponse
                    {
                        ResponseText = $"Here are the ingredients for {CurrentRecipe.Title}: {ingredientsList}",
                        Action = "show_ingredients",
                        RecipeData = CurrentRecipe
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
    }
}