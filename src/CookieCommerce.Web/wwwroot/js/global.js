// wwwroot/js/global.js
window.applyTheme = function () {
    console.log("Applying theme logic...");
    const theme = localStorage.getItem('theme');
    const systemDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

    if (theme === 'dark' || (!theme && systemDark)) {
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