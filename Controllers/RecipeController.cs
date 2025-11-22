using Microsoft.AspNetCore.Mvc;
using CookingWithVoice.Services;
using CookingWithVoice.Models;

namespace CookingWithVoice.Controllers
{
    [Route("api/recipe")]
    [ApiController]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly IRecipeSearchService _recipeSearchService;

        public RecipeController(IRecipeService recipeService, IRecipeSearchService recipeSearchService)
        {
            _recipeService = recipeService;
            _recipeSearchService = recipeSearchService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchRecipes([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Query parameter is required" });
            }

            try
            {
                var results = await _recipeSearchService.SearchRecipesAsync(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Search failed", message = ex.Message });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveRecipe([FromBody] RecipeSearchResult searchResult)
        {
            try
            {
                var recipe = await _recipeSearchService.ConvertToRecipeAsync(searchResult);
                recipe.IsSaved = true;
                var savedRecipe = await _recipeService.AddRecipeAsync(recipe);
                return Ok(savedRecipe);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Save failed", message = ex.Message });
            }
        }

        [HttpGet("saved")]
        public async Task<IActionResult> GetSavedRecipes()
        {
            try
            {
                var recipes = await _recipeService.GetAllRecipesAsync();
                return Ok(recipes.Where(r => r.IsSaved));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get saved recipes", message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            try
            {
                var success = await _recipeService.DeleteRecipeAsync(id);
                if (success)
                    return Ok(new { message = "Recipe deleted successfully" });
                else
                    return NotFound(new { error = "Recipe not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Delete failed", message = ex.Message });
            }
        }
    }
}