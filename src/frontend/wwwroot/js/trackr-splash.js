(function () {
    var root = document.documentElement;
    var splash = document.getElementById('trackr-splash');
    var app = document.getElementById('app');
    var display = 0;
    var lastBlazor = -1;
    var stallFrames = 0;
    var finishing = false;
    var rafId = 0;

    function readBlazor() {
        var raw = getComputedStyle(root).getPropertyValue('--blazor-load-percentage').trim();
        var value = parseFloat(raw);
        return Number.isFinite(value) ? Math.max(0, Math.min(100, value)) : 0;
    }

    function setDisplay(value) {
        display = Math.max(0, Math.min(100, value));
        var rounded = Math.round(display);
        root.style.setProperty('--trackr-load-display', rounded + '%');
        var label = document.getElementById('trackr-load-label');
        if (label) {
            label.textContent = rounded + '%';
        }
    }

    function hideSplash() {
        if (!splash) {
            return;
        }

        splash.classList.add('trackr-splash--hide');
        window.setTimeout(function () {
            splash.remove();
        }, 450);
    }

    function complete() {
        if (finishing) {
            return;
        }

        finishing = true;
    }

    function tick() {
        if (finishing) {
            setDisplay(Math.min(100, display + 2));
            if (display >= 99.5) {
                hideSplash();
                return;
            }

            rafId = requestAnimationFrame(tick);
            return;
        }

        var blazor = readBlazor();
        if (blazor === lastBlazor) {
            stallFrames++;
        } else {
            stallFrames = 0;
            lastBlazor = blazor;
        }

        var target;
        if (stallFrames > 15 || blazor >= 100) {
            // Downloads done or stalled — simulate runtime / startup progress
            target = Math.min(94, display + 0.45);
        } else {
            // Map asset download (0–100) into the first ~55% of the splash
            target = blazor * 0.55;
        }

        if (display < target) {
            var step = Math.max(0.35, (target - display) * 0.14);
            setDisplay(Math.min(target, display + step));
        }

        rafId = requestAnimationFrame(tick);
    }

    if (app) {
        var observer = new MutationObserver(function () {
            if (app.childElementCount > 0) {
                complete();
            }
        });
        observer.observe(app, { childList: true });
    }

    rafId = requestAnimationFrame(tick);
    window.trackrSplash = { complete: complete };
})();
