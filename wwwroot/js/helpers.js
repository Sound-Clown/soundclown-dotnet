// @ wwwroot/js/helpers.js
window.copyToClipboard = (text) => navigator.clipboard.writeText(text);
window.scrollToTop = () => window.scrollTo(0, 0);

// ── Toast — invoked from Blazor via IJSRuntime ──────────────────────────────────
window.showToast = (type, message) => {
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
