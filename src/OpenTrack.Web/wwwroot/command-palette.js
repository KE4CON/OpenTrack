// OpenTrack command palette — Ctrl/Cmd+K. Pure vanilla JS so it works the same on the static-SSR web
// host and the desktop WebView. It only ever navigates (changes the URL); no server interactivity.
(function () {
    "use strict";
    var overlay, input, list, items = [], sel = 0;

    var commands = [
        { label: "Dashboard", href: "dashboard", keys: ["dashboard", "home", "overview"] },
        { label: "All issues", href: "issues", keys: ["issues", "list", "bugs"] },
        { label: "Quick add a problem", href: "issues/quick", keys: ["new", "add", "quick", "report", "create"] },
        { label: "Projects", href: "projects", keys: ["projects"] },
        { label: "Notifications", href: "notifications", keys: ["notifications", "alerts"] },
        { label: "Backup & export", href: "backup", keys: ["backup", "export", "import", "csv", "json", "mantis"] }
    ];

    function ensureStyle() {
        if (document.getElementById("otcmdk-style")) return;
        var s = document.createElement("style");
        s.id = "otcmdk-style";
        s.textContent =
            ".otcmdk-overlay{position:fixed;inset:0;background:rgba(0,0,0,.4);display:none;align-items:flex-start;justify-content:center;z-index:2000;}" +
            ".otcmdk-box{background:var(--bs-body-bg,#fff);color:var(--bs-body-color,#212529);margin-top:12vh;width:min(90vw,36rem);border-radius:.5rem;box-shadow:0 1rem 3rem rgba(0,0,0,.4);overflow:hidden;}" +
            ".otcmdk-box input{width:100%;border:0;border-bottom:1px solid rgba(0,0,0,.1);padding:.9rem 1rem;font-size:1.1rem;outline:none;background:transparent;color:inherit;}" +
            ".otcmdk-list{max-height:20rem;overflow-y:auto;}" +
            ".otcmdk-item{padding:.6rem 1rem;cursor:pointer;}" +
            ".otcmdk-item.sel,.otcmdk-item:hover{background:rgba(13,110,253,.12);}" +
            ".otcmdk-empty{padding:.6rem 1rem;color:#888;}";
        document.head.appendChild(s);
    }

    function build() {
        ensureStyle();
        overlay = document.createElement("div");
        overlay.className = "otcmdk-overlay";
        var box = document.createElement("div");
        box.className = "otcmdk-box";
        input = document.createElement("input");
        input.type = "text";
        input.placeholder = "Type to search, #123 to jump to an issue, or a command…";
        input.setAttribute("aria-label", "Command palette");
        list = document.createElement("div");
        list.className = "otcmdk-list";
        box.appendChild(input);
        box.appendChild(list);
        overlay.appendChild(box);
        overlay.addEventListener("mousedown", function (e) { if (e.target === overlay) close(); });
        input.addEventListener("input", function () { render(input.value); });
        document.body.appendChild(overlay);
    }

    function isOpen() { return overlay && overlay.style.display === "flex"; }

    function open() {
        if (!overlay || !document.body.contains(overlay)) build();
        overlay.style.display = "flex";
        input.value = "";
        render("");
        setTimeout(function () { input.focus(); }, 0);
    }

    function close() { if (overlay) overlay.style.display = "none"; }

    function go(href) { close(); window.location.href = href; }

    function results(q) {
        q = (q || "").trim();
        var out = [];
        var m = q.match(/^#?(\d+)$/);
        if (m) out.push({ label: "Go to issue #" + m[1], href: "issues/" + m[1] });
        var lq = q.toLowerCase();
        commands.forEach(function (c) {
            if (!q || c.label.toLowerCase().indexOf(lq) >= 0 || c.keys.some(function (k) { return k.indexOf(lq) >= 0; }))
                out.push(c);
        });
        if (q && !m) out.push({ label: 'Search issues for "' + q + '"', href: "issues?Text=" + encodeURIComponent(q) });
        return out;
    }

    function render(q) {
        items = results(q);
        sel = 0;
        list.innerHTML = "";
        if (!items.length) {
            var e = document.createElement("div");
            e.className = "otcmdk-empty";
            e.textContent = "No matches.";
            list.appendChild(e);
            return;
        }
        items.forEach(function (it, i) {
            var el = document.createElement("div");
            el.className = "otcmdk-item" + (i === 0 ? " sel" : "");
            el.textContent = it.label;
            el.addEventListener("mousedown", function (ev) { ev.preventDefault(); go(it.href); });
            list.appendChild(el);
        });
    }

    function move(d) {
        if (!items.length) return;
        sel = (sel + d + items.length) % items.length;
        var kids = list.children;
        for (var i = 0; i < kids.length; i++) kids[i].classList.toggle("sel", i === sel);
        kids[sel].scrollIntoView({ block: "nearest" });
    }

    document.addEventListener("keydown", function (e) {
        if ((e.ctrlKey || e.metaKey) && (e.key === "k" || e.key === "K")) {
            e.preventDefault();
            isOpen() ? close() : open();
        } else if (isOpen()) {
            if (e.key === "Escape") { e.preventDefault(); close(); }
            else if (e.key === "ArrowDown") { e.preventDefault(); move(1); }
            else if (e.key === "ArrowUp") { e.preventDefault(); move(-1); }
            else if (e.key === "Enter") { e.preventDefault(); if (items[sel]) go(items[sel].href); }
        }
    });
})();
