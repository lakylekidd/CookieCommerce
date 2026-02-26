window.initPhoneInput = (element, dotNetHelper) => {
    const iti = window.intlTelInput(element, {
        initialCountry: "nl",
        utilsScript: "https://cdn.jsdelivr.net/npm/intl-tel-input@24.5.0/build/js/utils.js",
        strictMode: true
    });

    // When the user types, send the full international number back to Blazor
    element.addEventListener('blur', () => {
        const fullNumber = iti.getNumber();
        dotNetHelper.invokeMethodAsync('UpdatePhoneNumber', fullNumber);
    });
};