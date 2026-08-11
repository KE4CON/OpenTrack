// OpenTrack offline check-off for the bug-hunt checklist. Runs only on a /projects/{id}/checklist page.
//
// When you tap Pass/Fail/N-A while ONLINE, nothing changes — the normal static-SSR form posts as usual.
// When you're OFFLINE, the tap is intercepted: the change is queued in localStorage and the card is
// updated optimistically (with a "pending sync" marker). When the device comes back online — on the
// next load of the page, or when the browser fires the "online" event — the queue is replayed to the
// same-origin endpoint and the page refreshes to show the true, synced state.
//
// This deliberately avoids the Background Sync API (which iOS Safari lacks), so it works on an iPad.
(function () {
    "use strict";
    var QKEY = "ot-checklist-queue";
    var STATUS = { pass: "Pass", fail: "Fail", na: "NotApplicable", reset: "Pending" };
    var LABEL = { Pass: "Pass", Fail: "Fail", NotApplicable: "N/A", Pending: "To do" };
    var lastSubmitter = null;

    function readQueue() { try { return JSON.parse(localStorage.getItem(QKEY) || "[]"); } catch (e) { return []; } }
    function writeQueue(a) { try { localStorage.setItem(QKEY, JSON.stringify(a)); } catch (e) { } }

    function projectId() {
        var m = location.pathname.match(/\/projects\/(\d+)\/checklist/);
        return m ? parseInt(m[1], 10) : null;
    }

    function token() {
        var el = document.querySelector('form input[name="__RequestVerificationToken"]');
        return el ? el.value : null;
    }

    async function postStatus(pid, item, status, tok) {
        var body = new URLSearchParams();
        body.set("status", status);
        if (tok) body.set("__RequestVerificationToken", tok);
        return fetch("/checklist/" + pid + "/" + item + "/status", {
            method: "POST",
            body: body.toString(),
            credentials: "same-origin",
            headers: { "Content-Type": "application/x-www-form-urlencoded" }
        });
    }

    function enqueue(pid, item, status) {
        var items = readQueue().filter(function (x) { return !(x.p === pid && x.i === item); }); // last write wins per item
        items.push({ p: pid, i: item, s: status });
        writeQueue(items);
    }

    function optimistic(item, status) {
        var card = document.querySelector('[data-checklist-item="' + item + '"]');
        if (!card) return;
        var badge = card.querySelector(".js-status-badge");
        if (badge) badge.textContent = LABEL[status] || status;
        if (!card.querySelector(".js-pending")) {
            var note = document.createElement("span");
            note.className = "js-pending small text-warning ms-2";
            note.textContent = "• pending sync";
            (badge && badge.parentNode ? badge.parentNode : card).appendChild(note);
        }
    }

    async function flush() {
        var items = readQueue();
        if (!items.length || !navigator.onLine) return;
        var tok = token();
        var remaining = [];
        for (var k = 0; k < items.length; k++) {
            try {
                var r = await postStatus(items[k].p, items[k].i, items[k].s, tok);
                if (!r.ok) remaining.push(items[k]);
            } catch (e) { remaining.push(items[k]); }
        }
        writeQueue(remaining);
        if (remaining.length < items.length) window.location.reload(); // show the synced state
    }

    function onSubmit(e) {
        var pid = projectId();
        if (pid === null) return;
        var btn = (e.submitter) || lastSubmitter;
        if (!btn || btn.name !== "Action") return;
        var parts = String(btn.value || "").split(":");
        var verb = parts[0], item = parseInt(parts[1], 10);
        if (verb === "convert") {
            if (!navigator.onLine) { e.preventDefault(); alert("Creating an issue from a failure needs a connection. It will be here when you're back online."); }
            return;
        }
        if (!STATUS[verb] || isNaN(item)) return;
        if (navigator.onLine) return; // online → let the normal form submit proceed
        e.preventDefault();
        enqueue(pid, item, STATUS[verb]);
        optimistic(item, STATUS[verb]);
    }

    function init() {
        if (projectId() === null) return;
        var form = document.querySelector("form[data-checklist-form]");
        if (form) {
            // Track the clicked submit button (fallback for browsers without event.submitter).
            form.querySelectorAll('button[name="Action"]').forEach(function (b) {
                b.addEventListener("click", function () { lastSubmitter = b; });
            });
            form.addEventListener("submit", onSubmit);
        }
        flush();
    }

    document.addEventListener("DOMContentLoaded", init);
    document.addEventListener("enhancedload", init);
    window.addEventListener("online", function () { if (readQueue().length) window.location.reload(); });
})();
