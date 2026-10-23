// Clean Architecture WebUI Helper Utilities
window.setupCommandPaletteShortcut = (dotNetRef) => {
    window.addEventListener('keydown', (e) => {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('ToggleFromShortcut');
        }
    });
};

window.applyThemeConfiguration = (config) => {
    if (!config) return;
    const { isDark, primaryRgb, gradient, themeClass } = config;

    window.setThemeMode(isDark);

    // Remove any previous theme-* classes
    const classes = Array.from(document.documentElement.classList);
    classes.forEach(c => {
        if (c.startsWith('theme-')) {
            document.documentElement.classList.remove(c);
        }
    });

    if (themeClass) {
        document.documentElement.classList.add(themeClass);
    }

    if (primaryRgb) {
        document.documentElement.style.setProperty('--mud-palette-primary-rgb', primaryRgb);
    }
    if (gradient) {
        document.documentElement.style.setProperty('--primary-gradient', gradient);
    }
};
