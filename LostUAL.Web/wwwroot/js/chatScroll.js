window.lostualScrollToBottom = (elem) => {
    if (!elem) return;
    elem.scrollTo({ top: elem.scrollHeight, behavior: "smooth" });
};
