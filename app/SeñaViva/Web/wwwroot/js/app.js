// Loading feedback for a statically rendered app: a pixel progress bar while navigating or submitting,
// a busy state on the submitted button (which also blocks double submits) and closing the mobile menu.
(() => {
    const root = document.documentElement;
    let pending;

    const startLoading = () => {
        clearTimeout(pending);
        pending = setTimeout(() => root.classList.add("is-loading"), 120);
    };

    const stopLoading = () => {
        clearTimeout(pending);
        root.classList.remove("is-loading");
        document.querySelectorAll(".btn.is-busy").forEach((button) => {
            button.classList.remove("is-busy");
            button.removeAttribute("aria-busy");
        });

        const menu = document.getElementById("menu-toggle");
        if (menu) {
            menu.checked = false;
        }
    };

    document.addEventListener("click", (event) => {
        const link = event.target.closest("a[href]");
        if (!link || event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || link.target === "_blank") {
            return;
        }

        const url = new URL(link.href, location.href);
        if (url.origin === location.origin && !(url.hash && url.pathname === location.pathname)) {
            startLoading();
        }
    });

    document.addEventListener("submit", (event) => {
        const button = event.submitter ?? event.target.querySelector("[type=submit]");
        if (button?.classList.contains("is-busy")) {
            event.preventDefault();
            return;
        }

        if (button?.classList.contains("btn")) {
            button.classList.add("is-busy");
            button.setAttribute("aria-busy", "true");
        }

        startLoading();
    });

    window.addEventListener("pageshow", stopLoading);
    window.Blazor?.addEventListener?.("enhancedload", stopLoading);
})();
