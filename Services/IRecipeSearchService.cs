using CookingWithVoice.Models;

namespace CookingWithVoice.Services
{
    public interface IRecipeSearchService
    {
        Task<List<RecipeSearchResult>> SearchRecipesAsync(string query);
        Task<Recipe> ConvertToRecipeAsync(RecipeSearchResult searchResult);
    }
}