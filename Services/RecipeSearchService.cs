using CookingWithVoice.Models;
using System.Text.Json;

namespace CookingWithVoice.Services
{
    public class RecipeSearchService : IRecipeSearchService
    {
        private readonly HttpClient _httpClient;

        public RecipeSearchService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<List<RecipeSearchResult>> SearchRecipesAsync(string query)
        {
            // Enhanced mock implementation with realistic recipes
            var lowerQuery = query.ToLower();
            var mockResults = new List<RecipeSearchResult>();

            if (lowerQuery.Contains("pasta") || lowerQuery.Contains("spaghetti"))
            {
                mockResults.Add(new RecipeSearchResult
                {
                    Title = "Classic Spaghetti Carbonara",
                    Description = "Creamy Italian pasta with eggs, cheese, and pancetta.",
                    ImageUrl = "https://via.placeholder.com/300x200?text=Carbonara",
                    Source = "Italian Recipes",
                    Ingredients = new List<string>
                    {
                        "400g spaghetti", "200g pancetta or bacon", "4 large eggs", "100g Parmesan cheese", "2 cloves garlic", "Black pepper", "Salt"
                    },
                    Instructions = new List<string>
                    {
                        "Boil salted water and cook spaghetti until al dente",
                        "Fry pancetta in a large pan until crispy",
                        "Beat eggs with grated Parmesan and black pepper",
                        "Drain pasta, reserving some pasta water",
                        "Add hot pasta to pancetta pan",
                        "Remove from heat and quickly mix in egg mixture",
                        "Add pasta water if needed for creaminess",
                        "Serve immediately with extra Parmesan"
                    }
                });
            }
            else if (lowerQuery.Contains("chicken") || lowerQuery.Contains("curry"))
            {
                mockResults.Add(new RecipeSearchResult
                {
                    Title = "Chicken Curry",
                    Description = "Aromatic and flavorful chicken curry with spices.",
                    ImageUrl = "https://via.placeholder.com/300x200?text=Chicken+Curry",
                    Source = "Indian Cuisine",
                    Ingredients = new List<string>
                    {
                        "1 kg chicken pieces", "2 onions chopped", "4 tomatoes chopped", "1 tbsp ginger-garlic paste", "2 tsp curry powder", "1 tsp turmeric", "1 can coconut milk", "Salt to taste", "2 tbsp oil"
                    },
                    Instructions = new List<string>
                    {
                        "Heat oil in a large pot over medium heat",
                        "Add onions and cook until golden brown",
                        "Add ginger-garlic paste and cook for 1 minute",
                        "Add tomatoes and cook until soft",
                        "Add curry powder and turmeric, cook for 30 seconds",
                        "Add chicken pieces and brown on all sides",
                        "Pour in coconut milk and bring to a simmer",
                        "Cover and cook for 25 minutes until chicken is tender",
                        "Season with salt and serve with rice"
                    }
                });
            }
            else if (lowerQuery.Contains("chocolate") || lowerQuery.Contains("cake"))
            {
                mockResults.Add(new RecipeSearchResult
                {
                    Title = "Rich Chocolate Cake",
                    Description = "Moist and decadent chocolate cake perfect for celebrations.",
                    ImageUrl = "https://via.placeholder.com/300x200?text=Chocolate+Cake",
                    Source = "Baking Masters",
                    Ingredients = new List<string>
                    {
                        "2 cups all-purpose flour", "2 cups sugar", "3/4 cup cocoa powder", "2 tsp baking soda", "1 tsp baking powder", "1 tsp salt", "2 eggs", "1 cup buttermilk", "1 cup hot coffee", "1/2 cup vegetable oil"
                    },
                    Instructions = new List<string>
                    {
                        "Preheat oven to 350°F and grease two 9-inch pans",
                        "Mix all dry ingredients in a large bowl",
                        "In another bowl, whisk eggs, buttermilk, and oil",
                        "Combine wet and dry ingredients",
                        "Slowly add hot coffee and mix until smooth",
                        "Divide batter between prepared pans",
                        "Bake for 30-35 minutes until toothpick comes out clean",
                        "Cool completely before frosting"
                    }
                });
            }
            else
            {
                // Default generic recipes
                mockResults.AddRange(new List<RecipeSearchResult>
                {
                    new RecipeSearchResult
                    {
                        Title = $"Classic {query}",
                        Description = $"A delicious and easy {query} recipe perfect for any occasion.",
                        ImageUrl = "https://via.placeholder.com/300x200?text=Recipe+Image",
                        Source = "Recipe Collection",
                        Ingredients = new List<string>
                        {
                            "Main ingredient", "Seasonings", "Cooking oil", "Fresh herbs", "Salt and pepper"
                        },
                        Instructions = new List<string>
                        {
                            "Prepare all ingredients",
                            "Heat cooking oil in a pan",
                            "Add main ingredient and seasonings",
                            "Cook according to recipe requirements",
                            "Garnish with fresh herbs and serve"
                        }
                    }
                });
            }

            return Task.FromResult(mockResults);
        }

        public Task<Recipe> ConvertToRecipeAsync(RecipeSearchResult searchResult)
        {
            return Task.FromResult(new Recipe
            {
                Title = searchResult.Title,
                Description = searchResult.Description,
                ImageUrl = searchResult.ImageUrl,
                VideoUrl = searchResult.VideoUrl,
                Source = searchResult.Source,
                Ingredients = searchResult.Ingredients,
                Instructions = searchResult.Instructions,
                PrepTime = 15,
                CookTime = 30,
                Servings = 4,
                Difficulty = "Medium",
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}