(function () {
    const CONTAINER_ID = 'toast-container';
    const MAX_STACK = 3;
    const AUTO_DISMISS_MS = 5000;

    function ensureContainer() {
        let c = document.getElementById(CONTAINER_ID);
        if (!c) {
            c = document.createElement('div');
            c.id = CONTAINER_ID;
            c.setAttribute('aria-live', 'polite');
            c.setAttribute('aria-atomic', 'true');
            document.body.appendChild(c);
        }
        return c;
    }

    function iconFor(kind) {
        switch (kind) {
            case 'success': return 'bi-check-circle-fill';
            case 'error':   return 'bi-exclamation-octagon-fill';
            case 'warning': return 'bi-exclamation-triangle-fill';
            case 'info':
            default:        return 'bi-info-circle-fill';
        }
    }

    function show(kind, message) {
        if (!message) return;
        const container = ensureContainer();

        // Enforce stack limit
        while (container.children.length >= MAX_STACK) {
            container.removeChild(container.firstChild);
        }

        const el = document.createElement('div');
        el.className = 'app-toast app-toast-' + kind;
        el.setAttribute('role', 'status');
        el.innerHTML =
            '<i class="bi ' + iconFor(kind) + ' app-toast-icon"></i>' +
            '<div class="app-toast-body"></div>' +
            '<button type="button" class="app-toast-close" aria-label="Close">&times;</button>';

        el.querySelector('.app-toast-body').textContent = message;

        const dismiss = () => {
            el.classList.add('app-toast-out');
            setTimeout(() => el.remove(), 250);
        };
        el.querySelector('.app-toast-close').addEventListener('click', dismiss);
        container.appendChild(el);

        // Trigger enter animation on next frame
        requestAnimationFrame(() => el.classList.add('app-toast-in'));

        setTimeout(dismiss, AUTO_DISMISS_MS);
    }

    window.Toast = {
        success: (m) => show('success', m),
        error:   (m) => show('error', m),
        warning: (m) => show('warning', m),
        info:    (m) => show('info', m)
    };
})();
