// --- High-Accuracy (Whisper) Workflow ---
async function sendAudioToWhisper(audioBlob) {
    const formData = new FormData();
    // Assuming audioBlob is a Blob object from MediaRecorder
    formData.append('audioFile', audioBlob, 'command.webm'); 

    try {
        const response = await fetch('/api/voice/transcribe', {
            method: 'POST',
            body: formData
        });

        if (response.ok) {
            const result = await response.json();
            const transcribedText = result.transcription;
            document.getElementById('whisper-output').textContent = 'Heard (Whisper): ' + transcribedText;
            processWhisperCommand(transcribedText);
        } else {
            speak("Sorry, I could not transcribe that command.");
        }
    } catch (error) {
        console.error('Whisper API call failed:', error);
    }
}

// Process the high-accuracy text (more complex NLP/search)
function processWhisperCommand(command) {
    if (command.toLowerCase().includes('show me a recipe for')) {
        const query = command.replace(/show me a recipe for/i, '').trim();
        // Call backend recipe search endpoint
        fetchRecipeAndSpeak(query); 
    } 
    // ... more complex command parsing
}