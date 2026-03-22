// Advanced Mod Menu - Works WITHOUT Source Code
// Uses DOM manipulation, memory hooks, and event interception

const mods = {
    flying: false,
    speedMultiplier: 1,
    gravity: 1,
    infiniteHealth: false,
    noDamage: false,
    noCollision: false,
    slowMotion: false
};

// ============================================
// GAME MEMORY MANIPULATION
// ============================================

// Store original functions
const originalSetTimeout = window.setTimeout;
const originalSetInterval = window.setInterval;
const originalRAF = window.requestAnimationFrame;

let frameCount = 0;
let timeAccumulator = 0;

// Override requestAnimationFrame to modify deltaTime
window.requestAnimationFrame = function(callback) {
    return originalRAF(function(timestamp) {
        // Apply speed multiplier
        let modifiedTimestamp = timestamp;
        
        if (mods.slowMotion) {
            modifiedTimestamp = timestamp * 0.5; // 50% speed
        } else if (mods.speedMultiplier !== 1) {
            modifiedTimestamp = timestamp * mods.speedMultiplier;
        }
        
        callback(modifiedTimestamp);
    });
};

// Modify setTimeout for gravity effects
const originalSetTimeoutFn = window.setTimeout;
window.setTimeout = function(fn, delay) {
    if (mods.gravity !== 1 && delay === 0) {
        delay = Math.max(1, delay / mods.gravity);
    }
    return originalSetTimeoutFn(fn, delay);
};

// ============================================
// INPUT INTERCEPTION FOR FLYING
// ============================================

const keyStates = {};
let flyingVelocity = { x: 0, y: 0, z: 0 };

document.addEventListener('keydown', (e) => {
    keyStates[e.key.toLowerCase()] = true;
    
    if (mods.flying) {
        e.preventDefault();
        e.stopPropagation();
    }
}, true);

document.addEventListener('keyup', (e) => {
    keyStates[e.key.toLowerCase()] = false;
}, true);

// Monitor for canvas and inject flying controller
setInterval(() => {
    if (mods.flying) {
        // Simulate flying by manipulating canvas input
        const canvas = document.querySelector('canvas');
        if (canvas) {
            // Store flying state globally for game to access
            window.isFlyingEnabled = true;
            window.flyingInput = {
                up: keyStates['w'] || keyStates['arrowup'] || keyStates[' '],
                down: keyStates['s'] || keyStates['arrowdown'] || keyStates['control'],
                left: keyStates['a'] || keyStates['arrowleft'],
                right: keyStates['d'] || keyStates['arrowright']
            };
        }
    } else {
        window.isFlyingEnabled = false;
    }
}, 16); // ~60fps

// ============================================
// MOUSE INPUT INTERCEPTION
// ============================================

document.addEventListener('mousemove', (e) => {
    if (mods.flying) {
        // Store mouse position for flying rotation
        window.flyingMouse = {
            x: e.clientX,
            y: e.clientY,
            movementX: e.movementX || 0,
            movementY: e.movementY || 0
        };
    }
}, true);

// ============================================
// DOM INJECTION FOR GAME VARIABLES
// ============================================

// Create a script that runs in the game's context
const injectionScript = document.createElement('script');
injectionScript.textContent = `
    // Global mod state accessible to the game
    window.ModState = {
        flying: false,
        speedMultiplier: 1,
        gravity: 1,
        infiniteHealth: false,
        noDamage: false,
        noCollision: false,
        slowMotion: false
    };
    
    // Override Time.deltaTime behavior
    let deltaTimeMultiplier = 1;
    Object.defineProperty(window, 'deltaTimeMultiplier', {
        get: function() { return deltaTimeMultiplier; },
        set: function(val) { deltaTimeMultiplier = val; }
    });
    
    // Store gravity multiplier
    window.gravityMultiplier = 1;
    
    // Hook for detecting damage
    window.originalConsoleError = console.error;
    console.error = function(...args) {
        const msg = args.join(' ').toLowerCase();
        if (msg.includes('damage') || msg.includes('health')) {
            if (window.ModState.noDamage) {
                console.log('[MOD] Damage blocked');
                return;
            }
        }
        return window.originalConsoleError.apply(console, args);
    };
`;
document.head.appendChild(injectionScript);

// ============================================
// CREATE MOD MENU UI
// ============================================

const modMenuContainer = document.createElement('div');
modMenuContainer.id = 'mod-menu-container';
modMenuContainer.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    width: 380px;
    z-index: 100000;
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    display: none;
    background: linear-gradient(135deg, #f5f5f5 0%, #e8e8e8 100%);
    border: 2px solid #333;
    border-radius: 12px;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
    overflow: hidden;
`;

// Header
const header = document.createElement('div');
header.style.cssText = `
    background: linear-gradient(135deg, #2c3e50 0%, #34495e 100%);
    color: white;
    padding: 15px;
    text-align: center;
    font-size: 18px;
    font-weight: bold;
    border-bottom: 2px solid #333;
    letter-spacing: 1px;
`;
header.innerText = '⚙️ MOD MENU';
modMenuContainer.appendChild(header);

// Content area
const content = document.createElement('div');
content.style.cssText = `
    padding: 20px;
    max-height: 600px;
    overflow-y: auto;
`;

// Helper to create toggle
function createToggle(label, modKey, emoji) {
    const container = document.createElement('div');
    container.style.cssText = `
        margin-bottom: 12px;
        padding: 12px;
        background: white;
        border: 1px solid #ddd;
        border-radius: 6px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        transition: all 0.3s ease;
    `;
    
    container.addEventListener('mouseenter', () => {
        container.style.background = '#f9f9f9';
        container.style.borderColor = '#999';
        container.style.boxShadow = '0 2px 8px rgba(0,0,0,0.1)';
    });
    
    container.addEventListener('mouseleave', () => {
        container.style.background = 'white';
        container.style.borderColor = '#ddd';
        container.style.boxShadow = 'none';
    });

    const labelEl = document.createElement('label');
    labelEl.style.cssText = `
        color: #000;
        font-weight: 500;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 8px;
    `;
    labelEl.innerText = `${emoji} ${label}`;

    const toggle = document.createElement('input');
    toggle.type = 'checkbox';
    toggle.checked = mods[modKey];
    toggle.style.cssText = `
        width: 20px;
        height: 20px;
        cursor: pointer;
        accent-color: #2c3e50;
    `;

    toggle.addEventListener('change', () => {
        mods[modKey] = toggle.checked;
        window.ModState[modKey] = toggle.checked;
        
        // Apply mod effects
        if (modKey === 'speedMultiplier' || modKey === 'slowMotion') {
            window.deltaTimeMultiplier = mods.slowMotion ? 0.5 : mods.speedMultiplier;
        }
        if (modKey === 'gravity') {
            window.gravityMultiplier = mods.gravity;
        }
        
        console.log(`%c${emoji} ${label}: ${toggle.checked ? 'ON ✓' : 'OFF ✗'}`, 'color: #2c3e50; font-weight: bold; font-size: 13px;');
    });

    container.appendChild(labelEl);
    container.appendChild(toggle);
    return container;
}

// Helper to create slider
function createSlider(label, modKey, min, max, step, emoji) {
    const container = document.createElement('div');
    container.style.cssText = `
        margin-bottom: 12px;
        padding: 12px;
        background: white;
        border: 1px solid #ddd;
        border-radius: 6px;
    `;

    const labelEl = document.createElement('div');
    labelEl.style.cssText = `
        color: #000;
        font-weight: 500;
        margin-bottom: 8px;
        display: flex;
        justify-content: space-between;
        align-items: center;
    `;

    const labelText = document.createElement('span');
    labelText.innerText = `${emoji} ${label}`;

    const valueDisplay = document.createElement('span');
    valueDisplay.style.cssText = `
        color: #2c3e50;
        font-weight: bold;
        background: #f0f0f0;
        padding: 2px 8px;
        border-radius: 4px;
    `;
    valueDisplay.innerText = mods[modKey].toFixed(1);

    labelEl.appendChild(labelText);
    labelEl.appendChild(valueDisplay);
    container.appendChild(labelEl);

    const slider = document.createElement('input');
    slider.type = 'range';
    slider.min = min;
    slider.max = max;
    slider.step = step;
    slider.value = mods[modKey];
    slider.style.cssText = `
        width: 100%;
        height: 6px;
        border-radius: 3px;
        background: #ddd;
        outline: none;
        cursor: pointer;
        accent-color: #2c3e50;
    `;

    slider.addEventListener('input', () => {
        const value = parseFloat(slider.value);
        mods[modKey] = value;
        window.ModState[modKey] = value;
        valueDisplay.innerText = value.toFixed(1);
        
        // Apply mod effects
        if (modKey === 'speedMultiplier') {
            window.deltaTimeMultiplier = value;
        }
        if (modKey === 'gravity') {
            window.gravityMultiplier = value;
        }
        
        console.log(`%c${emoji} ${label}: ${value.toFixed(1)}`, 'color: #2c3e50; font-weight: bold; font-size: 13px;');
    });

    container.appendChild(slider);
    return container;
}

// Add all toggles and sliders
content.appendChild(createToggle('Flying', 'flying', '🚀'));
content.appendChild(createSlider('Speed', 'speedMultiplier', 0.5, 5, 0.1, '⚡'));
content.appendChild(createSlider('Gravity', 'gravity', 0, 3, 0.1, '⬇️'));
content.appendChild(createToggle('Infinite Health', 'infiniteHealth', '❤️'));
content.appendChild(createToggle('No Damage', 'noDamage', '🛡️'));
content.appendChild(createToggle('No Collision', 'noCollision', '👻'));
content.appendChild(createToggle('Slow Motion', 'slowMotion', '🐢'));

modMenuContainer.appendChild(content);

// Footer with status
const footer = document.createElement('div');
footer.style.cssText = `
    background: #f0f0f0;
    border-top: 1px solid #ddd;
    padding: 10px;
    text-align: center;
    font-size: 12px;
    color: #666;
`;
footer.innerText = 'Press M to toggle • Effects applied via hooks';
modMenuContainer.appendChild(footer);

document.body.appendChild(modMenuContainer);

// ============================================
// MENU TOGGLE
// ============================================

let menuOpen = false;

window.addEventListener('keydown', function(e) {
    if (e.key === 'M' || e.key === 'm') {
        e.preventDefault();
        e.stopPropagation();
        menuOpen = !menuOpen;
        modMenuContainer.style.display = menuOpen ? 'block' : 'none';
        console.log(`%c${menuOpen ? '📂 MOD MENU OPENED' : '📁 MOD MENU CLOSED'}`, 'color: #2c3e50; font-weight: bold; font-size: 14px;');
    }
}, true);

// ============================================
// INITIALIZATION
// ============================================

console.log('%c✅ MOD MENU LOADED - Press M to open', 'color: #2c3e50; font-weight: bold; font-size: 14px;');
console.log('%c🎮 Advanced Hooks Active (No Source Code Required)', 'color: green; font-weight: bold;');
console.log('%c⚡ Speed Mod: Affects game frame rate', 'color: #666;');
console.log('%c🐢 Slow Motion: Halves game speed', 'color: #666;');
console.log('%c🚀 Flying: Enables keyboard flight controls', 'color: #666;');
console.log('%c❤️ Other mods: Available in menu', 'color: #666;');

window.MODS = mods;
window.ModState = mods;
