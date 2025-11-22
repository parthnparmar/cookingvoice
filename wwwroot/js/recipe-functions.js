// Recipe Search and Display Functions

function displaySearchResults(results) {
    searchResultsData = results;
    recipeGrid.innerHTML = '';
    
    results.forEach((recipe, index) => {
        const card = document.createElement('div');
        card.className = 'recipe-card';
        card.onclick = () => selectRecipe(index);
        
        card.innerHTML = `
            <img src="${recipe.imageUrl || 'https://via.placeholder.com/300x200?text=Recipe'}" alt="${recipe.title}">
            <div class="recipe-card-content">
                <h4>${recipe.title}</h4>
                <p>${recipe.description}</p>
                <div class="recipe-actions">
                    <span class="recipe-source">${recipe.source}</span>
                    <button class="save-btn" onclick="event.stopPropagation(); saveRecipeFromSearch(${index})">💾 Save</button>
                </div>
            </div>
        `;
        
        recipeGrid.appendChild(card);
    });
    
    searchResults.style.display = 'block';
    recipeDisplay.style.display = 'none';
}

async function selectRecipe(index) {
    const searchResult = searchResultsData[index];
    currentRecipe = await convertSearchResultToRecipe(searchResult);
    currentStepIndex = 0;
    displayRecipe(currentRecipe, 0);
    searchResults.style.display = 'none';
}

function convertSearchResultToRecipe(searchResult) {
    return {
        id: 0,
        title: searchResult.title,
        description: searchResult.description,
        imageUrl: searchResult.imageUrl,
        videoUrl: searchResult.videoUrl,
        prepTime: 15,
        cookTime: 30,
        servings: 4,
        difficulty: 'Medium',
        ingredients: searchResult.ingredients,
        instructions: searchResult.instructions,
        source: searchResult.source
    };
}

function displayRecipe(recipe, stepIndex) {
    currentRecipe = recipe;
    currentStepIndex = stepIndex;
    
    recipeTitle.textContent = recipe.title;
    recipeDescription.textContent = recipe.description;
    recipeImage.src = recipe.imageUrl || 'https://via.placeholder.com/200x150?text=Recipe';
    prepTime.textContent = recipe.prepTime || 15;
    cookTime.textContent = recipe.cookTime || 30;
    servings.textContent = recipe.servings || 4;
    
    // Display ingredients
    ingredientsList.innerHTML = '';
    recipe.ingredients.forEach(ingredient => {
        const li = document.createElement('li');
        li.textContent = ingredient;
        ingredientsList.appendChild(li);
    });
    
    // Show ingredients first, then switch to instructions after a delay
    showIngredients();
    setTimeout(() => {
        showInstructions();
        updateStep(recipe, stepIndex);
        updateProgress();
        // Announce the first step
        speak(`Let's start cooking! Step 1: ${recipe.instructions[0]}`);
    }, 3000);
    
    recipeDisplay.style.display = 'block';
    searchResults.style.display = 'none';
}

function updateStep(recipe, stepIndex) {
    currentStepIndex = stepIndex;
    currentStepNumber.textContent = stepIndex + 1;
    currentInstruction.textContent = recipe.instructions[stepIndex];
    updateProgress();
}

function updateProgress() {
    if (currentRecipe) {
        const progress = ((currentStepIndex + 1) / currentRecipe.instructions.length) * 100;
        progressFill.style.width = progress + '%';
    }
}

// Navigation Functions
function showIngredients() {
    ingredientsSection.style.display = 'block';
    instructionsSection.style.display = 'none';
    updateNavButtons('ingredients');
}

function showInstructions() {
    ingredientsSection.style.display = 'none';
    instructionsSection.style.display = 'block';
    updateNavButtons('instructions');
}

function updateNavButtons(active) {
    document.querySelectorAll('.nav-btn').forEach(btn => btn.classList.remove('active'));
    if (active === 'ingredients') {
        document.querySelector('.nav-btn[onclick="showIngredients()"]').classList.add('active');
    } else {
        document.querySelector('.nav-btn[onclick="showInstructions()"]').classList.add('active');
    }
}

// Step Control Functions
function previousStep() {
    if (currentRecipe && currentStepIndex > 0) {
        currentStepIndex--;
        updateStep(currentRecipe, currentStepIndex);
        speak(`Step ${currentStepIndex + 1}: ${currentRecipe.instructions[currentStepIndex]}`);
    }
}

function nextStep() {
    if (currentRecipe && currentStepIndex < currentRecipe.instructions.length - 1) {
        currentStepIndex++;
        updateStep(currentRecipe, currentStepIndex);
        speak(`Step ${currentStepIndex + 1}: ${currentRecipe.instructions[currentStepIndex]}`);
    } else if (currentRecipe && currentStepIndex === currentRecipe.instructions.length - 1) {
        speak("That was the final step! Your dish is complete. Enjoy your meal!");
    }
}

function playStep() {
    if (currentRecipe) {
        const instruction = currentRecipe.instructions[currentStepIndex];
        speak(`Step ${currentStepIndex + 1}: ${instruction}`);
    }
}

// Save Functions
async function saveRecipe() {
    if (!currentRecipe) return;
    
    try {
        const response = await fetch('/api/recipe/save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(currentRecipe)
        });
        
        if (response.ok) {
            speak("Recipe saved successfully!");
            statusDiv.textContent = "Recipe saved to your collection";
        }
    } catch (error) {
        speak("Sorry, I couldn't save the recipe right now.");
    }
}

async function saveRecipeFromSearch(index) {
    const searchResult = searchResultsData[index];
    
    try {
        const response = await fetch('/api/recipe/save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(searchResult)
        });
        
        if (response.ok) {
            speak("Recipe saved successfully!");
            statusDiv.textContent = `${searchResult.title} saved to your collection`;
        }
    } catch (error) {
        speak("Sorry, I couldn't save the recipe right now.");
    }
}

// Saved Recipes Functions
async function toggleSaved() {
    if (savedRecipes.style.display === 'none' || !savedRecipes.style.display) {
        await loadSavedRecipes();
        savedRecipes.style.display = 'block';
    } else {
        savedRecipes.style.display = 'none';
    }
}

async function loadSavedRecipes() {
    try {
        const response = await fetch('/api/recipe/saved');
        const recipes = await response.json();
        
        savedRecipes.innerHTML = '';
        
        if (recipes.length === 0) {
            savedRecipes.innerHTML = '<p>No saved recipes yet. Save some recipes to see them here!</p>';
            return;
        }
        
        const grid = document.createElement('div');
        grid.className = 'recipe-grid';
        
        recipes.forEach(recipe => {
            const card = document.createElement('div');
            card.className = 'recipe-card';
            card.onclick = () => {
                displayRecipe(recipe, 0);
                savedRecipes.style.display = 'none';
            };
            
            card.innerHTML = `
                <img src="${recipe.imageUrl || 'https://via.placeholder.com/300x200?text=Recipe'}" alt="${recipe.title}">
                <div class="recipe-card-content">
                    <h4>${recipe.title}</h4>
                    <p>${recipe.description}</p>
                    <div class="recipe-actions">
                        <span class="recipe-source">${recipe.source}</span>
                        <button class="delete-btn" onclick="event.stopPropagation(); deleteRecipe(${recipe.id})">🗑️ Delete</button>
                    </div>
                </div>
            `;
            
            grid.appendChild(card);
        });
        
        savedRecipes.appendChild(grid);
    } catch (error) {
        savedRecipes.innerHTML = '<p>Error loading saved recipes.</p>';
    }
}

async function deleteRecipe(id) {
    try {
        const response = await fetch(`/api/recipe/${id}`, { method: 'DELETE' });
        if (response.ok) {
            await loadSavedRecipes();
            speak("Recipe deleted successfully!");
        }
    } catch (error) {
        speak("Sorry, I couldn't delete the recipe right now.");
    }
}