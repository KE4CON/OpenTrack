// OpenTrack service worker — makes the web app installable and keeps previously-visited pages
// available offline. Static assets are cached on install; page navigations are network-first with a
// cache fallback, so pages you've opened while online remain viewable offline (e.g. a bug-hunt
// checklist next to you on a tablet). Only GETs are cached — mutations always go to the network (and
// the offline check-off queue in checklist-offline.js handles writes made while offline).
//
// Note: cached pages persist on the device, so this suits a personal/trusted device. On a shared
// device, sign out clears the session but not this cache; bump CACHE below to invalidate if needed.
const CACHE = "ot-cache-v1";
const SHELL = [
    "/",
    "/command-palette.js",
    "/auto-refresh.js",
    "/checklist-offline.js",
    "/manifest.webmanifest",
    "/icon.svg",
];

self.addEventListener("install", (e) => {
    e.waitUntil(caches.open(CACHE).then((c) => c.addAll(SHELL)).then(() => self.skipWaiting()));
});

self.addEventListener("activate", (e) => {
    e.waitUntil(
        caches.keys()
            .then((keys) => Promise.all(keys.filter((k) => k !== CACHE).map((k) => caches.delete(k))))
            .then(() => self.clients.claim())
    );
});

self.addEventListener("fetch", (e) => {
    const req = e.request;
    if (req.method !== "GET") return; // never cache mutations
    const url = new URL(req.url);
    if (url.origin !== location.origin) return; // don't touch cross-origin
    if (url.pathname.startsWith("/_framework") || url.pathname.startsWith("/_blazor")) return; // let Blazor manage its own

    e.respondWith((async () => {
        try {
            const res = await fetch(req);
            // Cache successful navigations and known static assets for offline use.
            if (res && res.ok && (req.mode === "navigate" || SHELL.includes(url.pathname))) {
                const c = await caches.open(CACHE);
                c.put(req, res.clone());
            }
            return res;
        } catch (err) {
            const cached = await caches.match(req);
            if (cached) return cached;
            if (req.mode === "navigate") {
                const home = await caches.match("/");
                if (home) return home;
            }
            throw err;
        }
    })());
});
