// wwwroot/js/global.js
window.applyTheme = function (defaultDark) {
    if (typeof defaultDark === 'boolean') window.commerceDefaultDark = defaultDark;
    console.log("Applying theme logic...");
    const theme = localStorage.getItem('theme');
    const systemDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

    if (theme === 'dark' || (!theme && (window.commerceDefaultDark || systemDark))) {
        document.documentElement.classList.add('dark');
        console.log("Tiger says: Dark mode ON");
    } else {
        document.documentElement.classList.remove('dark');
        console.log("Tiger says: Dark mode OFF");
    }
};

// Auto-run on load
applyTheme();

// Listen for Blazor's special navigation
document.addEventListener('enhancedload', applyTheme);

window.requestCommerceNotifications = async function () {
    if ('Notification' in window && Notification.permission === 'default') await Notification.requestPermission();
};
window.notifyCommerceOrder = function (showNotification, sound) {
    if (showNotification && 'Notification' in window && Notification.permission === 'granted')
        new Notification('New paid cookie order', { body: 'Open the orders page to prepare the order.', tag: 'cookie-order' });
    if (sound) {
        try {
            const audio = new (window.AudioContext || window.webkitAudioContext)();
            const oscillator = audio.createOscillator();
            const gain = audio.createGain();
            gain.gain.value = 0.08;
            oscillator.frequency.value = 660;
            oscillator.connect(gain); gain.connect(audio.destination);
            oscillator.start(); oscillator.stop(audio.currentTime + 0.2);
            oscillator.onended = () => audio.close();
        } catch (_) { }
    }
};
