globalThis.copyToClipboard = (text) => navigator.clipboard.writeText(text);
globalThis.scrollToTop = () => window.scrollTo(0, 0);

// Trigger a file input click (for drag-and-drop or custom click)
globalThis.triggerInputFile = (inputId) => {
    const el = document.getElementById(inputId);
    if (el) el.click();
};

// Read file from an input[type=file] element as base64 string
globalThis.readFileAsBase64 = (inputId) => {
    return new Promise((resolve) => {
        const input = document.getElementById(inputId);
        if (!input || !input.files || !input.files[0]) { resolve(''); return; }
        const reader = new FileReader();
        reader.onload = () => {
            const idx = reader.result?.indexOf(',') ?? -1;
            resolve(idx >= 0 ? reader.result.substring(idx + 1) : '');
        };
        reader.onerror = () => resolve('');
        reader.readAsDataURL(input.files[0]);
    });
};

// Read the first dropped file directly from a DragEvent's dataTransfer.
// Returns JSON: { name, contentType, size, data } or null on failure.
globalThis.readDropFile = (dataTransfer) => {
    return new Promise((resolve) => {
        if (!dataTransfer?.files?.length) { resolve(null); return; }
        const file = dataTransfer.files[0];
        const reader = new FileReader();
        reader.onload = () => {
            const idx = reader.result?.indexOf(',') ?? -1;
            const base64 = idx >= 0 ? reader.result.substring(idx + 1) : '';
            resolve(JSON.stringify({
                name: file.name,
                contentType: file.type,
                size: file.size,
                data: base64
            }));
        };
        reader.onerror = () => resolve(null);
        reader.readAsDataURL(file);
    });
};

// ── Toast — invoked from Blazor via IJSRuntime ──────────────────────────────────
globalThis.showToast = (type, message) => {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const el = document.createElement('div');
    el.className = `toast toast-${type}`;
    el.innerHTML = `<span>${message}</span>`;
    container.appendChild(el);

    setTimeout(() => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(8px)';
        el.style.transition = 'opacity 0.2s, transform 0.2s';
        setTimeout(() => el.remove(), 250);
    }, 3500);
};
