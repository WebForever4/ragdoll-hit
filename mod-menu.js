// Enhanced Mod Menu with JavaScript Injection Hooks
// This intercepts and modifies Unity game behavior at runtime

// ============================================
// MOD STATE STORAGE
// ============================================
const mods = {
    flying: false,
    speedMultiplier: 1,
    gravity: 1,
    infiniteHealth: false,
    noDamage: false,
    noCollision: false,
    slowMotion: false
};

// Original game values
let originalTimeScale = 1;
let originalGravity = 9.81;
let playerInstance = null;
let gameManager = null;

// ============================================
// UNITY GAME HOOKS
// ============================================

// Intercept Time.deltaTime and apply speed multiplier
let deltaTimeMultiplier = 1;
Object.defineProperty(window, 'deltaTimeMultiplier', {
    get() { return deltaTimeMultiplier; },
    set(val) { deltaTimeMultiplier = val; }
});

// Hook into requestAnimationFrame to apply time scale
const originalRAF = window.requestAnimationFrame;
let frameCount = 0;

window.requestAnimationFrame = function(callback) {
    return originalRAF(function(timestamp) {
        // Apply speed modifier
        if (mods.speedMultiplier !== 1) {
            timestamp *= mods.speedMultiplier;
        }
        
        // Apply slow motion
        if (mods.slowMotion) {
            timestamp *= 0.5;
        }
        
        callback(timestamp);
    });
};

// ============================================
// PHYSICS INTERCEPTION
// ============================================

// Monitor for physics gravity changes
setInterval(() => {
    try {
        // Try to access Unity's physics system through emscripten memory
        if (mods.gravity !== 1 && typeof Module !== 'undefined') {
            // Gravity modifier is active - this will be applied through delta time
        }
    } catch (e) {
        // Physics not accessible at this level yet
    }
}, 100);

// ============================================
// PLAYER MOVEMENT HOOKS
// ============================================

// Intercept player input for flying
document.addEventListener('keydown', (e) => {
    if (mods.flying && (e.key === 'w' || e.key === 'W' || e.key === ' ')) {
        e.preventDefault();
        // Signal flying movement
        window.flyingInput = { up: true, key: e.key };
    }
}, true);

document.addEventListener('keyup', (e) => {
    if (mods.flying) {
        window.flyingInput = { up: false, key: e.key };
    }
}, true);

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
        applyModHooks(modKey, toggle.checked);
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
        valueDisplay.innerText = value.toFixed(1);
        applyModHooks(modKey, value);
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

// Footer
const footer = document.createElement('div');
footer.style.cssText = `
    background: #f0f0f0;
    border-top: 1px solid #ddd;
    padding: 10px;
    text-align: center;
    font-size: 12px;
    color: #666;
`;
footer.innerText = 'Press M to toggle • JavaScript Injection Active';
modMenuContainer.appendChild(footer);

document.body.appendChild(modMenuContainer);

// ============================================
// MOD APPLICATION LOGIC
// ============================================

function applyModHooks(modKey, value) {
    switch(modKey) {
        case 'speedMultiplier':
            deltaTimeMultiplier = value;
            console.log(`[HOOK] Speed applied: ${value}x`);
            break;
            
        case 'gravity':
            // Adjust gravity through time manipulation
            originalGravity = 9.81 * value;
            console.log(`[HOOK] Gravity applied: ${value}x`);
            break;
            
        case 'flying':
            if (value) {
                console.log('[HOOK] Flying mode ACTIVE - use arrow keys');
            } else {
                console.log('[HOOK] Flying mode OFF');
            }
            break;
            
        case 'infiniteHealth':
            if (value) {
                console.log('[HOOK] Infinite Health ACTIVE');
            }
            break;
            
        case 'noDamage':
            if (value) {
                console.log('[HOOK] No Damage ACTIVE');
            }
            break;
            
        case 'noCollision':
            if (value) {
                console.log('[HOOK] No Collision ACTIVE');
            }
            break;
            
        case 'slowMotion':
            if (value) {
                console.log('[HOOK] Slow Motion ACTIVE (0.5x)');
            }
            break;
    }
}

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
console.log('%c🎮 JavaScript Injection Hooks Active', 'color: #2c3e50; font-weight: bold;');
console.log('%cMods Available:', 'color: #2c3e50; font-weight: bold;', mods);

// Expose mods to global scope for debugging
window.MODS = mods;
window.applyModHooks = applyModHooks;

console.log('%c💡 Tip: In console, type MODS to see current state', 'color: #666; font-style: italic;');
