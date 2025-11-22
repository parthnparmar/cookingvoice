# 🍳 Voice Cooking Assistant

A smart cooking assistant that responds to voice commands and guides you through recipes step-by-step.

## Voice Commands

### Recipe Search
- **"Find pasta recipe"** - Search for pasta recipes
- **"Find chicken curry recipe"** - Search for chicken curry recipes  
- **"Find chocolate cake recipe"** - Search for chocolate cake recipes
- **"Search for [recipe name]"** - Search for any recipe

### Cooking Instructions
- **"Cook pasta"** or **"Make chicken curry"** - Start cooking a specific recipe
- **"Next step"** or **"Continue"** - Move to the next cooking step
- **"Previous step"** or **"Go back"** - Go back to the previous step
- **"Repeat"** or **"Say that again"** - Repeat the current step
- **"Show ingredients"** - List all ingredients for the current recipe

### Timer & Utilities
- **"Set timer for 5 minutes"** - Set a cooking timer
- **"My recipes"** or **"Saved recipes"** - Show your saved recipes

## How to Use

1. **Start the Application**
   - Run the project in Visual Studio or use `dotnet run`
   - Open your browser to the application URL

2. **Voice Interaction**
   - Click the microphone button 🎤
   - Speak your command clearly
   - Wait for the assistant to respond

3. **Recipe Workflow**
   - Say "Find pasta recipe" to search for recipes
   - Click on a recipe card to select it, or say "Cook pasta" to start directly
   - The assistant will show ingredients first, then guide you through each step
   - Use "Next step" to progress through the cooking instructions
   - The assistant will speak each instruction aloud

## Features

- **Voice Recognition**: Uses OpenAI Whisper for accurate speech-to-text
- **Text-to-Speech**: Speaks instructions and responses aloud
- **Step-by-Step Guidance**: Guides you through each cooking step
- **Recipe Search**: Find recipes by voice command
- **Timer Integration**: Set cooking timers with voice commands
- **Recipe Management**: Save and manage your favorite recipes
- **Mobile Friendly**: Works on mobile devices with microphone access

## Technical Requirements

- **Browser**: Chrome, Firefox, or Safari with microphone permissions
- **Internet Connection**: Required for OpenAI Whisper API
- **Microphone**: For voice input
- **Speakers/Headphones**: For voice output

## Usage Tips

- **Speak Clearly**: Ensure clear pronunciation for better recognition
- **Wait for Response**: Let the assistant finish speaking before giving the next command
- **Use Natural Language**: The assistant understands conversational commands
- **Hands-Free Cooking**: Perfect for when your hands are busy cooking!

## Development

This is an ASP.NET Core application with:
- **Backend**: C# controllers handling voice processing
- **Frontend**: JavaScript for voice interaction and UI
- **Database**: SQLite for recipe storage
- **APIs**: OpenAI Whisper for speech recognition

## Example Conversation

**You**: "Find pasta recipe"
**Assistant**: "I found recipes for pasta. Which one would you like to cook?"
*[Shows recipe options]*

**You**: "Cook spaghetti carbonara"
**Assistant**: "Great! Let's cook Classic Spaghetti Carbonara. Here are the ingredients you'll need..."
*[Shows ingredients, then switches to instructions]*

**You**: "Next step"
**Assistant**: "Step 2: Fry pancetta in a large pan until crispy"

**You**: "Set timer for 10 minutes"
**Assistant**: "Setting a timer for 10 minutes."

Enjoy cooking with your voice assistant! 👨‍🍳🎤
