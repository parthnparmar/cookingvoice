// Check for Web Speech API and MediaRecorder Support
const SpeechSynthesis = window.speechSynthesis;
if (!navigator.mediaDevices || !window.MediaRecorder) {
    alert("Your browser does not fully support the required APIs (MediaRecorder or Web Speech API). Please use Chrome or Firefox.");
}

// UI Elements
const statusDiv = document.getElementById('status');
const startStopBtn = document.getElementById('startStopBtn');
const btnIcon = document.getElementById('btnIcon');
const btnText = document.getElementById('btnText');
const recipeDisplay = document.getElementById('recipeDisplay');
const searchResults = document.getElementById('searchResults');
const recipeGrid = document.getElementById('recipeGrid');
const recipeTitle = document.getElementById('recipeTitle');
const recipeImage = document.getElementById('recipeImage');
const recipeDescription = document.getElementById('recipeDescription');
const prepTime = document.getElementById('prepTime');
const cookTime = document.getElementById('cookTime');
const servings = document.getElementById('servings');
const ingredientsList = document.getElementById('ingredientsList');
const currentInstruction = document.getElementById('currentInstruction');
const currentStepNumber = document.getElementById('currentStepNumber');
const timerDisplay = document.getElementById('timerDisplay');
const timerValue = document.getElementById('timerValue');
const ingredientsSection = document.getElementById('ingredientsSection');
const instructionsSection = document.getElementById('instructionsSection');
const progressFill = document.getElementById('progressFill');
const savedRecipes = document.getElementById('savedRecipes');

// State Variables
let mediaRecorder;
let audioChunks = [];
let audioStream;
let isRecording = false;
let currentTimer = null;
let currentRecipe = null;
let currentStepIndex = 0;
let searchResultsData = [];

// --- Text-to-Speech (TTS) Function (🔊) ---
function speak(text) {
    if (SpeechSynthesis.speaking) {
        SpeechSynthesis.cancel();
    }
    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = 'en-US'; 
    utterance.rate = 1.0;
    SpeechSynthesis.speak(utterance);
}

// --- Audio Recording Functions (🎤) ---
function blobToFile(theBlob, fileName){
    // Creates a File object from a Blob for FormData submission
    const file = new File([theBlob], fileName, { type: theBlob.type });
    return file;
}

async function startRecording() {
    try {
        if (SpeechSynthesis.speaking) SpeechSynthesis.cancel();
        
        audioStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        mediaRecorder = new MediaRecorder(audioStream, { mimeType: 'audio/webm' }); // Use webm for compatibility
        audioChunks = [];

        mediaRecorder.ondataavailable = event => {
            audioChunks.push(event.data);
        };

        mediaRecorder.onstop = () => {
            const audioBlob = new Blob(audioChunks, { type: 'audio/webm' });
            uploadAudio(audioBlob);
            audioStream.getTracks().forEach(track => track.stop());
            btnIcon.textContent = '🎤';
            btnText.textContent = 'Tap to Speak';
            startStopBtn.classList.remove('recording');
        };

        mediaRecorder.start();
        isRecording = true;
        statusDiv.textContent = "Recording... Say your command now.";
        btnIcon.textContent = '🔴';
        btnText.textContent = 'Recording...';
        startStopBtn.classList.add('recording');

    } catch (err) {
        statusDiv.textContent = 'Error accessing the microphone. Check permissions.';
        console.error('Microphone error:', err);
        speak('I need microphone permission to work.');
    }
}

function stopRecording() {
    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
        mediaRecorder.stop();
        isRecording = false;
        statusDiv.textContent = "Recording stopped. Uploading to Whisper API for high-accuracy transcription...";
    }
}



// --- Response Handler ---
function handleAssistantResponse(data) {
    statusDiv.textContent = data.responseText;
    speak(data.responseText);

    switch (data.action) {
        case "show_search_results":
            displaySearchResults(data.searchResults);
            break;
        case "show_recipe":
            displayRecipe(data.recipeData, data.currentStepIndex);
            break;
        case "update_step":
        case "repeat_step":
            updateStep(data.recipeData, data.currentStepIndex);
            break;
        case "set_timer":
            startKitchenTimer(data.timerDuration);
            break;
        case "show_ingredients":
            if (currentRecipe) {
                showIngredients();
            }
            break;
        case "show_saved_recipes":
            toggleSaved();
            break;
        case "recipe_complete":
            // Show completion celebration
            currentInstruction.innerHTML = "<div class='completion-message'>🎉 Recipe Complete! 🎉<br>Enjoy your delicious meal!</div>";
            break;
        case "info":
        default:
            break;
    }
}

// --- Recipe Display Functions ---
function displayRecipe(recipe, stepIndex) {
    currentRecipe = recipe;
    currentStepIndex = stepIndex;
    
    recipeTitle.textContent = recipe.title;
    recipeDescription.textContent = recipe.description || '';
    recipeImage.src = recipe.imageUrl || 'https://via.placeholder.com/200x150?text=Recipe';
    prepTime.textContent = recipe.prepTime || 15;
    cookTime.textContent = recipe.cookTime || 30;
    servings.textContent = recipe.servings || 4;
    
    ingredientsList.innerHTML = '';
    recipe.ingredients.forEach(ing => {
        const li = document.createElement('li');
        li.textContent = ing;
        ingredientsList.appendChild(li);
    });

    // Show ingredients first, then switch to instructions
    showIngredients();
    setTimeout(() => {
        showInstructions();
        updateStep(recipe, stepIndex);
        updateProgress();
    }, 2000);

    recipeDisplay.style.display = 'block';
    searchResults.style.display = 'none';
}

function updateStep(recipe, stepIndex) {
    currentRecipe = recipe;
    currentStepIndex = stepIndex;
    currentInstruction.textContent = recipe.instructions[stepIndex];
    currentStepNumber.textContent = stepIndex + 1;
    updateProgress();
}


// --- Timer Function (⏲️) ---
function startKitchenTimer(durationString) {
    if (currentTimer) clearInterval(currentTimer);

    // Simple parsing for "X minutes"
    const match = durationString.match(/(\d+)\s+minutes?/i);
    if (!match) return;

    let minutes = parseInt(match[1]);
    let totalSeconds = minutes * 60;
    
    timerDisplay.style.display = 'block';
    
    function updateTimer() {
        let min = Math.floor(totalSeconds / 60);
        let sec = totalSeconds % 60;
        timerValue.textContent = `${min.toString().padStart(2, '0')}:${sec.toString().padStart(2, '0')}`;

        if (totalSeconds <= 0) {
            clearInterval(currentTimer);
            timerDisplay.style.display = 'none';
            speak("Your timer for " + durationString + " is finished!");
            statusDiv.textContent = "TIMER FINISHED!";
        }
        totalSeconds--;
    }

    updateTimer();
    currentTimer = setInterval(updateTimer, 1000);
}


// --- Navigation Functions ---
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

function updateProgress() {
    if (currentRecipe) {
        const progress = ((currentStepIndex + 1) / currentRecipe.instructions.length) * 100;
        progressFill.style.width = progress + '%';
    }
}

// --- Step Control Functions ---
function previousStep() {
    if (currentRecipe && currentStepIndex > 0) {
        currentStepIndex--;
        updateStep(currentRecipe, currentStepIndex);
        speak(`Going back to step ${currentStepIndex + 1}: ${currentRecipe.instructions[currentStepIndex]}`);
    }
}

function nextStep() {
    if (currentRecipe && currentStepIndex < currentRecipe.instructions.length - 1) {
        currentStepIndex++;
        updateStep(currentRecipe, currentStepIndex);
        speak(`Step ${currentStepIndex + 1}: ${currentRecipe.instructions[currentStepIndex]}`);
    } else if (currentRecipe && currentStepIndex === currentRecipe.instructions.length - 1) {
        speak("Congratulations! You've completed the recipe. Your dish is ready to enjoy!");
        currentInstruction.innerHTML = "<div class='completion-message'>🎉 Recipe Complete! 🎉<br>Enjoy your delicious meal!</div>";
    }
}

function playStep() {
    if (currentRecipe) {
        const instruction = currentRecipe.instructions[currentStepIndex];
        speak(`Step ${currentStepIndex + 1}: ${instruction}`);
    }
}

// --- Save Functions ---
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

// --- Saved Recipes Functions ---
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

// --- Search Results Functions ---
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
    currentRecipe = convertSearchResultToRecipe(searchResult);
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

// --- Text Input Functionality ---
const textCommand = document.getElementById('textCommand');
const textSubmit = document.getElementById('textSubmit');

if (textSubmit) {
    textSubmit.addEventListener('click', () => {
        const command = textCommand.value.trim();
        if (command) {
            processTextCommand(command);
            textCommand.value = '';
        }
    });
}

if (textCommand) {
    textCommand.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            const command = textCommand.value.trim();
            if (command) {
                processTextCommand(command);
                textCommand.value = '';
            }
        }
    });
}

async function processTextCommand(command) {
    statusDiv.textContent = `Processing: "${command}"`;
    
    try {
        const response = await fetch('/api/voice/process-text', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ command: command })
        });
        
        if (response.ok) {
            const data = await response.json();
            handleAssistantResponse(data);
        } else {
            const errorData = await response.json();
            statusDiv.textContent = `Error: ${errorData.error || 'Unknown error'}`;
            speak('Sorry, there was an error processing your command.');
        }
    } catch (error) {
        statusDiv.textContent = `Error: ${error.message}`;
        speak('Sorry, there was an error processing your command.');
    }
}

// --- Enhanced Error Handling for Voice ---
async function uploadAudio(audioBlob) {
    const audioFile = blobToFile(audioBlob, `audio-${Date.now()}.webm`);
    const formData = new FormData();
    formData.append("audioFile", audioFile);

    try {
        statusDiv.textContent = "Processing your voice command...";
        
        const response = await fetch('/api/voice/transcribe', {
            method: 'POST',
            body: formData,
        });

        if (response.ok) {
            const data = await response.json();
            if (data.transcription) {
                statusDiv.textContent = `Heard: "${data.transcription}"`;
            }
            handleAssistantResponse(data);
        } else {
            const errorData = await response.json();
            statusDiv.textContent = `Voice Error: ${errorData.message || errorData.error}`;
            speak('Sorry, I had trouble understanding you. Try typing your command instead.');
            
            // Show text input as fallback
            if (textCommand) {
                textCommand.focus();
                textCommand.placeholder = "Voice failed - type your command here...";
            }
        }
    } catch (error) {
        statusDiv.textContent = `Connection Error: ${error.message}`;
        speak('Sorry, there was a connection error. Try typing your command.');
        
        if (textCommand) {
            textCommand.focus();
            textCommand.placeholder = "Connection failed - type your command here...";
        }
    }
}

// --- Main Event Listener ---
startStopBtn.addEventListener('click', () => {
    if (isRecording) {
        stopRecording();
    } else {
        startRecording();
    }
});