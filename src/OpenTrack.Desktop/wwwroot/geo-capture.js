// geo-capture.js — fills an issue form's hidden Latitude/Longitude inputs from the browser's location.
// Vanilla island (no framework): a button [data-geo-capture] inside a <form> that contains
// input[name="model.Latitude"] and input[name="model.Longitude"]. Purely opt-in — nothing happens until
// the user clicks, and location is only read with the browser's own permission prompt.
(function () {
    "use strict";

    function wire(btn) {
        if (btn._geoWired) return;
        btn._geoWired = true;
        btn.addEventListener("click", function (e) {
            e.preventDefault();
            var form = btn.closest("form");
            if (!form) return;
            if (!navigator.geolocation) { status(btn, "This browser can't provide a location."); return; }

            status(btn, "Getting your location…");
            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    var lat = pos.coords.latitude, lng = pos.coords.longitude;
                    setValue(form, 'input[name="model.Latitude"]', lat.toFixed(6));
                    setValue(form, 'input[name="model.Longitude"]', lng.toFixed(6));
                    status(btn, "Location attached: " + lat.toFixed(5) + ", " + lng.toFixed(5));
                },
                function (err) { status(btn, "Couldn't get location: " + (err && err.message ? err.message : "denied")); },
                { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
            );
        });
    }

    function setValue(form, selector, value) {
        var el = form.querySelector(selector);
        if (el) { el.value = value; el.dispatchEvent(new Event("input", { bubbles: true })); }
    }

    function status(btn, text) {
        var id = btn.getAttribute("data-geo-status");
        var el = id ? document.getElementById(id) : null;
        if (el) el.textContent = text;
    }

    function init() { document.querySelectorAll("[data-geo-capture]").forEach(wire); }

    document.addEventListener("DOMContentLoaded", init);
    document.addEventListener("enhancedload", init); // re-run after Blazor enhanced navigation
})();
