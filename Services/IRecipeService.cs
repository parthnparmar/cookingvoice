using CookingWithVoice.Models;

namespace CookingWithVoice.Services
{
    public interface IRecipeService
    {
        Task<Recipe?> FindRecipeByTitleAsync(string title);
        Task<List<Recipe>> GetAllRecipesAsync();
        Task<Recipe> AddRecipeAsync(Recipe recipe);
        Task<Recipe?> UpdateRecipeAsync(Recipe recipe);
        Task<bool> DeleteRecipeAsync(int id);
    }
}