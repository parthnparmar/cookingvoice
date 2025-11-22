// --- Web Speech API: Synthesis (TTS) ---
const synth = window.speechSynthesis;

function speak(text) {
    if (synth.speaking) {
        console.error('speechSynthesis.speaking');
        return;
    }
    const utterThis = new SpeechSynthesisUtterance(text);
    utterThis.onend = function (event) {
        console.log('Speech finished');
    }
    utterThis.onerror = function (event) {
        console.error('Speech error: ' + event.error);
    }
    // Set voice properties (optional)
    // utterThis.voice = synth.getVoices().find(v => v.lang === 'en-US');
    utterThis.rate = 1.0;
    synth.speak(utterThis);
}

// --- Web Speech API: Recognition (Simple Commands) ---
let recognition;
let isListening = false;
const statusElement = document.getElementById('voice-status');
const outputElement = document.getElementById('voice-output');

if ('webkitSpeechRecognition' in window) {
    recognition = new webkitSpeechRecognition();
    recognition.continuous = false; // Only listen for a single phrase
    recognition.interimResults = false;
    recognition.lang = 'en-US';

    recognition.onresult = function(event) {
        const transcript = event.results[0][0].transcript.toLowerCase();
        outputElement.textContent = 'Heard (Local): ' + transcript;
        processLocalCommand(transcript);
        stopListening(); // Stop after successful recognition
    }

    recognition.onerror = function(event) {
        console.error('Recognition error: ' + event.error);
        statusElement.textContent = 'Error: ' + event.error;
        stopListening();
    }

    recognition.onend = function() {
        if (isListening) {
             // Restart listening if continuous mode is desired
             // startListening();
        }
        statusElement.textContent = 'Ready for Command';
        isListening = false;
    }
} else {
    // Fallback or use Whisper for all transcription
    statusElement.textContent = 'Web Speech Recognition not supported. Use mic button for high-accuracy mode.';
}

function startListening() {
    if (recognition && !isListening) {
        isListening = true;
        recognition.start();
        statusElement.textContent = 'Listening...';
    } else {
        // Fallback or High-Accuracy mode start (see next section)
        startHighAccuracyRecording(); 
    }
}

function stopListening() {
    if (recognition && isListening) {
        recognition.stop();
        isListening = false;
    }
}

// --- Command Processing (Simple Logic) ---
function processLocalCommand(command) {
    if (command.includes('hello') || command.includes('hi')) {
        speak("Hello! How can I assist you in the kitchen?");
    } else if (command.includes('next step') || command.includes('go on')) {
        // Implement logic to advance recipe step
        speak("Moving to the next instruction.");
    } else if (command.includes('timer') && command.includes('minute')) {
        // Simple timer logic
        speak("Setting a timer for ten minutes.");
        // Implement timer start
    } else if (command.includes('show me a recipe for')) {
        // For search, typically use High-Accuracy/Whisper mode
        speak("Finding that recipe now. Please wait.");
        // Trigger Whisper/AJAX workflow
    } else {
        speak("I heard: " + command + ". Could you please repeat?");
    }
}

// Attach to UI element (e.g., a mic button)
document.getElementById('mic-button').addEventListener('click', () => {
    startListening();
});