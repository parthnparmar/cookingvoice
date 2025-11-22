namespace CookingWithVoice.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Cuisine { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public int PrepTime { get; set; } // in minutes
        public int CookTime { get; set; } // in minutes
        public int Servings { get; set; }
        public string Difficulty { get; set; } = "Medium";
        public List<string> Ingredients { get; set; } = new List<string>();
        public List<string> Instructions { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSaved { get; set; } = false;
        public string Source { get; set; } = "Manual"; // Manual, Google, YouTube
    }

    public class RecipeSearchResult
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // Google, YouTube
        public string SourceUrl { get; set; } = string.Empty;
        public List<string> Ingredients { get; set; } = new List<string>();
        public List<string> Instructions { get; set; } = new List<string>();
    }

    // Helper model to capture command intent from transcription
    public class CommandIntent
    {
        public string Intent { get; set; } = "unknown"; // e.g., "find_recipe", "next_step", "timer"
        public string Target { get; set; } = string.Empty; // e.g., "chocolate cake", "10 minutes"
    }

    // Model for the response sent back to the frontend
    public class AssistantResponse
    {
        public string ResponseText { get; set; } = string.Empty;
        public string Action { get; set; } = "info"; // Used by JS to trigger UI/State changes
        public Recipe? RecipeData { get; set; }
        public List<RecipeSearchResult>? SearchResults { get; set; }
        public int CurrentStepIndex { get; set; }
        public string TimerDuration { get; set; } = string.Empty;
        public string ConversionResult { get; set; } = string.Empty;
    }
}