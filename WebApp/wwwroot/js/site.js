document.addEventListener("click", async function (e) {
    const link = e.target.closest("[data-ajax-link]");
    if (!link) return;

    e.preventDefault();

    const url = link.getAttribute("href");

    const response = await fetch(url, {
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        }
    });

    if (!response.ok) return;

    const html = await response.text();

    document.getElementById("page-content").innerHTML = html;
    window.history.pushState({}, "", url);
});

window.addEventListener("popstate", async () => {
    const response = await fetch(location.href, {
        headers: { "X-Requested-With": "XMLHttpRequest" }
    });

    if (!response.ok) return;

    document.getElementById("page-content").innerHTML =
        await response.text();
});
