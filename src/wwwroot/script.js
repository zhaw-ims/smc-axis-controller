window.registerPressEvents = (element, dotnetHelper) => {
    if (!element) return; // Prevent null reference issues

    element.addEventListener("mousedown", () => {
        dotnetHelper.invokeMethodAsync("OnJogPlusPressStart");
    });

    element.addEventListener("mouseup", () => {
        dotnetHelper.invokeMethodAsync("OnJogPlusPressEnd");
    });

    // Support for mobile touch events
    element.addEventListener("touchstart", () => {
        dotnetHelper.invokeMethodAsync("OnJogPlusPressStart");
    });

    element.addEventListener("touchend", () => {
        dotnetHelper.invokeMethodAsync("OnJogPlusPressEnd");
    });
};