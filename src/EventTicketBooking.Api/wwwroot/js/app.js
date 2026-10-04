// AETHER SEATS — LIQUID GLASS CLIENT APP LOGIC
document.addEventListener('DOMContentLoaded', () => {
    // API Configuration
    const API_BASE = '/api';
    
    // Application State
    let currentUser = {
        token: localStorage.getItem('jwt_token') || null,
        email: localStorage.getItem('user_email') || null,
        role: localStorage.getItem('user_role') || null
    };

    let cachedEvents = [];

    // DOM Elements
    const authModal = document.getElementById('auth-modal');
    const openAuthBtn = document.getElementById('open-auth-btn');
    const closeAuthModalBtn = document.getElementById('close-auth-modal');
    const authStatusContainer = document.getElementById('auth-status-container');
    
    const navButtons = document.querySelectorAll('.nav-btn');
    const pageSections = document.querySelectorAll('.page-section');
    const adminNavBtn = document.getElementById('admin-nav-btn');
    
    const eventsGrid = document.getElementById('events-grid-container');
    const ticketsContainer = document.getElementById('tickets-container');
    const refreshEventsBtn = document.getElementById('refresh-events-btn');
    const eventSearchInput = document.getElementById('event-search-input');
    
    const loginForm = document.getElementById('login-form');
    const registerForm = document.getElementById('register-form');
    const createEventForm = document.getElementById('create-event-form');
    const modalTabs = document.querySelectorAll('.modal-tab');

    const statTotalEvents = document.getElementById('stat-total-events');
    const statAvailableSeats = document.getElementById('stat-available-seats');

    // Init App
    init();

    function init() {
        setupNavigation();
        setupAuthModal();
        updateAuthUI();
        loadEvents();
        
        refreshEventsBtn?.addEventListener('click', loadEvents);
        eventSearchInput?.addEventListener('input', filterEvents);
        createEventForm?.addEventListener('submit', handleCreateEvent);
    }

    // Navigation Tabs
    function setupNavigation() {
        navButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                const target = btn.dataset.target;
                
                navButtons.forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                
                pageSections.forEach(section => {
                    section.classList.remove('active');
                    if (section.id === target) {
                        section.classList.add('active');
                    }
                });

                if (target === 'my-tickets-section') {
                    loadUserTickets();
                }
            });
        });
    }

    // Modal & Auth Setup
    function setupAuthModal() {
        openAuthBtn?.addEventListener('click', () => authModal.classList.add('active'));
        closeAuthModalBtn?.addEventListener('click', () => authModal.classList.remove('active'));
        
        modalTabs.forEach(tab => {
            tab.addEventListener('click', () => {
                modalTabs.forEach(t => t.classList.remove('active'));
                tab.classList.add('active');

                const targetTab = tab.dataset.tab;
                document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
                document.getElementById(targetTab).classList.add('active');
            });
        });

        loginForm?.addEventListener('submit', handleLogin);
        registerForm?.addEventListener('submit', handleRegister);
    }

    // Auth Actions
    async function handleLogin(e) {
        e.preventDefault();
        const email = document.getElementById('login-email').value;
        const password = document.getElementById('login-password').value;

        try {
            const res = await fetch(`${API_BASE}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });

            const data = await res.json();
            if (!res.ok) throw new Error(data.detail || data.message || 'Login failed.');

            saveUserSession(data.token, data.email, data.role);
            showToast(`Welcome back, ${data.email}!`, 'success');
            authModal.classList.remove('active');
            loginForm.reset();
        } catch (err) {
            showToast(err.message, 'error');
        }
    }

    async function handleRegister(e) {
        e.preventDefault();
        const email = document.getElementById('register-email').value;
        const password = document.getElementById('register-password').value;

        try {
            const res = await fetch(`${API_BASE}/auth/register`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });

            const data = await res.json();
            if (!res.ok) throw new Error(data.detail || data.message || 'Registration failed.');

            showToast('Registration successful! Please sign in.', 'success');
            // Switch to login tab
            document.querySelector('[data-tab="login-tab"]').click();
            document.getElementById('login-email').value = email;
            registerForm.reset();
        } catch (err) {
            showToast(err.message, 'error');
        }
    }

    function saveUserSession(token, email, role) {
        currentUser = { token, email, role };
        localStorage.setItem('jwt_token', token);
        localStorage.setItem('user_email', email);
        localStorage.setItem('user_role', role);
        updateAuthUI();
    }

    function logout() {
        currentUser = { token: null, email: null, role: null };
        localStorage.removeItem('jwt_token');
        localStorage.removeItem('user_email');
        localStorage.removeItem('user_role');
        updateAuthUI();
        showToast('Signed out successfully.', 'info');
    }

    function updateAuthUI() {
        if (currentUser.token) {
            authStatusContainer.innerHTML = `
                <div style="display: flex; align-items: center; gap: 12px;">
                    <span style="font-size: 13px; color: var(--text-muted);"><i class="fa-solid fa-user-circle"></i> ${currentUser.email} (${currentUser.role})</span>
                    <button class="glass-btn icon-only" id="logout-btn" title="Sign Out">
                        <i class="fa-solid fa-right-from-bracket"></i>
                    </button>
                </div>
            `;
            document.getElementById('logout-btn').addEventListener('click', logout);

            if (currentUser.role === 'Admin') {
                adminNavBtn?.classList.remove('hidden');
            } else {
                adminNavBtn?.classList.add('hidden');
            }
        } else {
            authStatusContainer.innerHTML = `
                <button class="glass-btn primary" id="open-auth-btn">
                    <i class="fa-solid fa-user-astronaut"></i> Sign In / Register
                </button>
            `;
            document.getElementById('open-auth-btn').addEventListener('click', () => authModal.classList.add('active'));
            adminNavBtn?.classList.add('hidden');
        }
    }

    // Load Events
    async function loadEvents() {
        eventsGrid.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px;"><i class="fa-solid fa-circle-notch fa-spin fa-2x" style="color: var(--accent-indigo);"></i><p style="margin-top: 12px; color: var(--text-muted);">Fetching live REST API events...</p></div>`;

        try {
            const res = await fetch(`${API_BASE}/events`);
            if (!res.ok) throw new Error('Failed to load events.');
            
            cachedEvents = await res.json();
            renderEvents(cachedEvents);
            updateStats(cachedEvents);
        } catch (err) {
            eventsGrid.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px; color: var(--accent-rose);"><i class="fa-solid fa-triangle-exclamation fa-2x"></i><p style="margin-top: 12px;">${err.message}</p></div>`;
        }
    }

    function renderEvents(events) {
        if (events.length === 0) {
            eventsGrid.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px; color: var(--text-muted);"><i class="fa-solid fa-calendar-xmark fa-2x"></i><p style="margin-top: 12px;">No active events found. Admins can host new events!</p></div>`;
            return;
        }

        eventsGrid.innerHTML = events.map(ev => {
            const isSoldOut = ev.availableTickets <= 0;
            const percentFilled = Math.round(((ev.totalTickets - ev.availableTickets) / ev.totalTickets) * 100);
            const dateFormatted = new Date(ev.eventDate).toLocaleDateString('en-US', {
                weekday: 'short', month: 'short', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit'
            });

            return `
                <div class="event-card">
                    <div>
                        <div class="event-header">
                            <span class="event-badge ${isSoldOut ? 'sold-out' : ''}">${isSoldOut ? 'Sold Out' : 'Registration Open'}</span>
                            <span style="font-size: 12px; color: var(--text-dim);"><i class="fa-solid fa-hashtag"></i> EVT-${ev.id}</span>
                        </div>
                        <h3 class="event-card-title">${escapeHtml(ev.title)}</h3>
                        <p class="event-card-desc">${escapeHtml(ev.description || 'No description provided.')}</p>
                        
                        <div class="event-meta-list">
                            <div class="event-meta-item">
                                <i class="fa-regular fa-clock"></i>
                                <span>${dateFormatted}</span>
                            </div>
                            <div class="event-meta-item">
                                <i class="fa-solid fa-users"></i>
                                <span>Total Capacity: ${ev.totalTickets} seats</span>
                            </div>
                        </div>

                        <div class="capacity-progress-wrapper">
                            <div class="capacity-labels">
                                <span class="seats-left">${ev.availableTickets} Tickets Left</span>
                                <span style="color: var(--text-dim);">${percentFilled}% Booked</span>
                            </div>
                            <div class="progress-bar-bg">
                                <div class="progress-bar-fill" style="width: ${percentFilled}%;"></div>
                            </div>
                        </div>
                    </div>

                    <div class="card-action-row">
                        ${isSoldOut ? `
                            <button class="glass-btn block" disabled style="opacity: 0.5; cursor: not-allowed;">
                                <i class="fa-solid fa-ban"></i> Event Sold Out
                            </button>
                        ` : `
                            <div class="quantity-selector">
                                <button class="qty-btn minus" data-id="${ev.id}">-</button>
                                <span class="qty-number" id="qty-${ev.id}">1</span>
                                <button class="qty-btn plus" data-id="${ev.id}">+</button>
                            </div>
                            <button class="glass-btn primary book-btn" data-id="${ev.id}" style="flex: 1;">
                                <i class="fa-solid fa-bolt"></i> Instant Book
                            </button>
                        `}
                    </div>
                </div>
            `;
        }).join('');

        // Attach quantity & booking listeners
        document.querySelectorAll('.qty-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const id = btn.dataset.id;
                const qtySpan = document.getElementById(`qty-${id}`);
                let current = parseInt(qtySpan.innerText);
                if (btn.classList.contains('plus') && current < 10) qtySpan.innerText = current + 1;
                if (btn.classList.contains('minus') && current > 1) qtySpan.innerText = current - 1;
            });
        });

        document.querySelectorAll('.book-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                const id = parseInt(btn.dataset.id);
                const quantity = parseInt(document.getElementById(`qty-${id}`).innerText);
                handleBooking(id, quantity);
            });
        });
    }

    function filterEvents() {
        const query = eventSearchInput.value.toLowerCase();
        const filtered = cachedEvents.filter(e => 
            e.title.toLowerCase().includes(query) || 
            e.description.toLowerCase().includes(query)
        );
        renderEvents(filtered);
    }

    function updateStats(events) {
        statTotalEvents.innerText = events.length;
        const totalOpenSeats = events.reduce((sum, e) => sum + e.availableTickets, 0);
        statAvailableSeats.innerText = totalOpenSeats;
    }

    // Book Tickets
    async function handleBooking(eventId, quantity) {
        if (!currentUser.token) {
            showToast('Please sign in to reserve tickets.', 'info');
            authModal.classList.add('active');
            return;
        }

        try {
            const res = await fetch(`${API_BASE}/bookings`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${currentUser.token}`
                },
                body: JSON.stringify({ eventId, quantity })
            });

            const data = await res.json();
            if (!res.ok) throw new Error(data.detail || data.message || 'Booking failed.');

            showToast(`Success! ${quantity} Ticket(s) booked for '${data.eventTitle}'!`, 'success');
            loadEvents(); // Refresh capacity
        } catch (err) {
            showToast(err.message, 'error');
        }
    }

    // Load User Tickets
    async function loadUserTickets() {
        if (!currentUser.token) {
            ticketsContainer.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px; color: var(--text-muted);"><i class="fa-solid fa-lock fa-2x"></i><p style="margin-top: 12px;">Sign in to view your reserved ticket passcodes.</p></div>`;
            return;
        }

        ticketsContainer.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px;"><i class="fa-solid fa-circle-notch fa-spin fa-2x" style="color: var(--accent-indigo);"></i></div>`;

        try {
            const res = await fetch(`${API_BASE}/bookings/my`, {
                headers: { 'Authorization': `Bearer ${currentUser.token}` }
            });

            if (!res.ok) throw new Error('Failed to fetch user tickets.');
            const bookings = await res.json();

            if (bookings.length === 0) {
                ticketsContainer.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px; color: var(--text-muted);"><i class="fa-solid fa-ticket-simple fa-2x"></i><p style="margin-top: 12px;">You have no active ticket bookings yet.</p></div>`;
                return;
            }

            ticketsContainer.innerHTML = bookings.map(b => `
                <div class="ticket-passcode-card">
                    <div class="ticket-header">
                        <div>
                            <span style="font-size: 11px; text-transform: uppercase; color: var(--accent-cyan); font-weight: 700;">Booking #${b.bookingId}</span>
                            <h3 class="ticket-event-title">${escapeHtml(b.eventTitle)}</h3>
                        </div>
                        <i class="fa-solid fa-qrcode fa-2x" style="color: rgba(255,255,255,0.3);"></i>
                    </div>

                    <div style="font-size: 13px; color: var(--text-muted); margin-bottom: 12px;">
                        <i class="fa-regular fa-calendar-check"></i> Booked on: ${new Date(b.bookingDate).toLocaleString()} | Quantity: ${b.quantity}
                    </div>

                    <div class="ticket-codes-list">
                        ${b.tickets.map(t => `
                            <div class="ticket-code-pill">
                                <div>
                                    <div style="font-size: 10px; color: var(--text-dim); text-transform: uppercase;">Passcode Token</div>
                                    <div class="code-text">${t.ticketCode}</div>
                                </div>
                                <i class="fa-solid fa-shield-check" style="color: var(--accent-emerald);"></i>
                            </div>
                        `).join('')}
                    </div>
                </div>
            `).join('');
        } catch (err) {
            ticketsContainer.innerHTML = `<div style="grid-column: 1/-1; text-align: center; padding: 40px; color: var(--accent-rose);"><p>${err.message}</p></div>`;
        }
    }

    // Host Event (Admin)
    async function handleCreateEvent(e) {
        e.preventDefault();
        if (!currentUser.token || currentUser.role !== 'Admin') {
            showToast('Admin authorization required.', 'error');
            return;
        }

        const title = document.getElementById('event-title').value;
        const eventDate = document.getElementById('event-date').value;
        const totalTickets = parseInt(document.getElementById('event-capacity').value);
        const description = document.getElementById('event-description').value;

        try {
            const res = await fetch(`${API_BASE}/events`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${currentUser.token}`
                },
                body: JSON.stringify({ title, description, eventDate, totalTickets })
            });

            const data = await res.json();
            if (!res.ok) throw new Error(data.detail || data.message || 'Failed to create event.');

            showToast(`Event '${data.title}' created successfully!`, 'success');
            createEventForm.reset();
            // Switch to explore events
            document.querySelector('[data-target="events-section"]').click();
            loadEvents();
        } catch (err) {
            showToast(err.message, 'error');
        }
    }

    // Toast Helpers
    function showToast(message, type = 'info') {
        const toastContainer = document.getElementById('toast-container');
        const toast = document.createElement('div');
        toast.className = `glass-toast ${type}`;
        
        const iconMap = {
            success: 'fa-circle-check',
            error: 'fa-circle-exclamation',
            info: 'fa-circle-info'
        };

        toast.innerHTML = `<i class="fa-solid ${iconMap[type]}"></i> <span>${escapeHtml(message)}</span>`;
        toastContainer.appendChild(toast);

        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(30px)';
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }

    function escapeHtml(str) {
        return str ? str.replace(/[&<>'"]/g, 
            tag => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[tag] || tag)
        ) : '';
    }
});
