window.speakText = (text, lang) => {
    if (!('speechSynthesis' in window)) {
        alert("Trình duyệt của bạn không hỗ trợ Text to Speech");
        return;
    }
    window.speechSynthesis.cancel(); // Stop currently playing audio
    if (!text) return;

    var msg = new SpeechSynthesisUtterance();
    msg.text = text;
    // Try to find correct voice based on lang
    // Vi: 'vi-VN', En: 'en-US' or 'en-GB'
    var langCode = lang === 'vi' ? 'vi-VN' : 'en-US';
    msg.lang = langCode;

    window.speechSynthesis.speak(msg);
};

window.stopSpeaking = () => {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel();
    }
};
