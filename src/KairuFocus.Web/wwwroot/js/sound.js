window.kairufocusSound = {
    play: function (soundFile, volume) {
        if (!soundFile) return;
        var audio = new Audio(soundFile);
        audio.volume = Math.min(1, Math.max(0, (volume ?? 100) / 100));
        audio.play().catch(function () { /* autoplay may be blocked */ });
    }
};
