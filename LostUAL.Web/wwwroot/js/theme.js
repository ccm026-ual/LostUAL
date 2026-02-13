window.theme = {
    get: () => localStorage.getItem("theme") || "light",
    set: (t) => {
        document.documentElement.setAttribute("data-bs-theme", t);
        localStorage.setItem("theme", t);
    }
};
