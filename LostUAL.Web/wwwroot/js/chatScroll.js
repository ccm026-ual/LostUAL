window.lostualScrollToBottom = (elem) => {
    if (!elem) return;
    if (typeof elem.scrollTop === "undefined") return;
    elem.scrollTo({ top: elem.scrollHeight, behavior: "smooth" });
};
