using CookingWithVoice.Models;
using CookingWithVoice.Data;
using Microsoft.EntityFrameworkCore;

namespace CookingWithVoice.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly CookingDbContext _context;

        public RecipeService(CookingDbContext context)
        {
            _context = context;
        }

        public async Task<Recipe?> FindRecipeByTitleAsync(string title)
        {
            return await _context.Recipes
                .FirstOrDefaultAsync(r => r.Title.ToLower().Contains(title.ToLower()));
        }

        public async Task<List<Recipe>> GetAllRecipesAsync()
        {
            return await _context.Recipes.ToListAsync();
        }

        public async Task<Recipe> AddRecipeAsync(Recipe recipe)
        {
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return recipe;
        }

        public async Task<Recipe?> UpdateRecipeAsync(Recipe recipe)
        {
            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();
            return recipe;
        }

        public async Task<bool> DeleteRecipeAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return false;
            
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}