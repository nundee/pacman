export function PlaySound() {
    document.getElementById('game-sound').play();
}

export function StopSound() {
    var audio = document.getElementById('game-sound');
    audio.pause();
    audio.currentTime = 0; // Reset the audio to the beginning
}