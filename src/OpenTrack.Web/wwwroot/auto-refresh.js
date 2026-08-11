// OpenTrack smart-poll auto-refresh. A page opts in by rendering
//   <input type="checkbox" class="ot-autorefresh" data-key="dashboard">
// When ticked, every ~30s it asks the server "has anything I can see changed?" (GET /activity/token)
// and reloads ONLY when the answer differs from what was loaded — so the page stays current without
// pointless reloads. It never reloads while the tab is hidden or while you're typing. The choice is
// remembered per page. Pure vanilla JS; on hosts without the endpoint (e.g. the desktop app) the fetch
// simply fails and nothing happens.
(function () {
    "use strict";
    var INTERVAL_MS = 30000;
    var timer = null;
    var lastToken = null;

    function storageKey(box) { return "ot-live-" + (box.getAttribute("data-key") || "page"); }

    function busy() {
        if (document.hidden) return true;
        var a = document.activeElement;
        return !!(a && (a.tagName === "INPUT" || a.tagName === "TEXTAREA" || a.tagName === "SELECT"));
    }

    async function poll() {
        if (busy()) return;
        try {
            var r = await fetch("/activity/token", { headers: { "Accept": "text/plain" }, credentials: "same-origin" });
            if (!r.ok) return;
            var t = await r.text();
            if (lastToken === null) { lastToken = t; return; } // establish a baseline first
            if (t !== lastToken) { lastToken = t; window.location.reload(); }
        } catch (e) { /* endpoint unavailable (e.g. desktop app) — do nothing */ }
    }

    function apply(box) {
        try { localStorage.setItem(storageKey(box), box.checked ? "1" : "0"); } catch (e) { }
        if (timer) { clearInterval(timer); timer = null; }
        lastToken = null;
        if (box.checked) { poll(); timer = setInterval(poll, INTERVAL_MS); }
    }

    function init() {
        if (timer) { clearInterval(timer); timer = null; }
        document.querySelectorAll("input.ot-autorefresh").forEach(function (box) {
            var saved = null;
            try { saved = localStorage.getItem(storageKey(box)); } catch (e) { }
            if (saved === "1") box.checked = true;
            box.addEventListener("change", function () { apply(box); });
            if (box.checked) apply(box);
        });
    }

    document.addEventListener("DOMContentLoaded", init);
    // Blazor enhanced navigation swaps the DOM without a full reload — re-scan after each navigation.
    document.addEventListener("enhancedload", init);
})();
