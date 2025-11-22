namespace CookingWithVoice.Models
{
    /// <summary>
    /// Represents the parsed intent and target extracted from the user's voice command.
    /// This model helps the backend decide what action to take.
    /// </summary>
    public class Commandintent
    {
        /// <summary>
        /// The recognized action the user wants to perform (e.g., "find_recipe", "next_step", "set_timer", "unit_convert").
        /// </summary>
        public string Intent { get; set; } = "unknown"; 

        /// <summary>
        /// The specific subject or parameter of the action (e.g., "chocolate cake", "10 minutes", "cups to grams").
        /// </summary>
        public string Target { get; set; } = string.Empty; 
    }
}