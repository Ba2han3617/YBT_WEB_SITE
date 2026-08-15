// ── Navbar scroll effect ────────────────────────────────────
window.addEventListener('scroll', function() {
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        if (window.scrollY > 50) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }
    }
});

// ── Scroll Reveal ────────────────────────────────────────────
function reveal() {
    var reveals = document.querySelectorAll('.reveal');
    for (var i = 0; i < reveals.length; i++) {
        var windowHeight = window.innerHeight;
        var elementTop = reveals[i].getBoundingClientRect().top;
        if (elementTop < windowHeight - 120) {
            reveals[i].classList.add('active');
        }
    }
}
window.addEventListener('scroll', reveal);
reveal(); // İlk yükleme

// ── Counter Animation (suffix desteği ile) ───────────────────
function formatNumber(value) {
    return value >= 1000 ? (value / 1000).toFixed(1).replace('.0', '') + 'k' : value;
}

function animateCounter(counter) {
    const target   = parseInt(counter.getAttribute('data-target') || '0', 10);
    const suffix   = counter.getAttribute('data-suffix') || '';
    const duration = 1800; // ms
    const start    = performance.now();

    function update(now) {
        const elapsed  = now - start;
        const progress = Math.min(elapsed / duration, 1);
        // Ease-out cubic
        const eased = 1 - Math.pow(1 - progress, 3);
        const current = Math.round(eased * target);
        counter.textContent = current + suffix;
        if (progress < 1) {
            requestAnimationFrame(update);
        } else {
            counter.textContent = target + suffix;
        }
    }

    requestAnimationFrame(update);
}

// IntersectionObserver ile görünce başlat
const counterObserver = new IntersectionObserver((entries, obs) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            const counters = entry.target.querySelectorAll('.counter-value');
            counters.forEach(c => animateCounter(c));
            obs.unobserve(entry.target);
        }
    });
}, { threshold: 0.3 });

const metricsSection = document.querySelector('.metrics-section');
if (metricsSection) {
    counterObserver.observe(metricsSection);
}

// Eski counter-section desteği (geriye dönük uyumluluk)
const oldCounterSection = document.querySelector('.counter-section');
if (oldCounterSection) {
    counterObserver.observe(oldCounterSection);
}

// ── Hero code rain (hafif dekoratif) ────────────────────────
(function buildCodeRain() {
    const container = document.getElementById('codeRain');
    if (!container) return;

    const chars = '01アイウエオカキクケコABCDEF{};()=>function const let var<>/#@!$%';
    const cols  = Math.floor(container.offsetWidth / 14) || 60;

    let html = '';
    for (let i = 0; i < cols * 28; i++) {
        html += chars[Math.floor(Math.random() * chars.length)];
        if ((i + 1) % cols === 0) html += '\n';
    }
    container.textContent = html;
})();
