// TeknoSOS - Enhanced Frontend JavaScript
// Clean ES6+ modular architecture

// ==========================================
// CSRF TOKEN HELPER
// ==========================================
function getAntiForgeryToken() {
    const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenEl ? tokenEl.value : '';
}

function getDefaultHeaders() {
    const headers = {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest'
    };
    const token = getAntiForgeryToken();
    if (token) {
        headers['RequestVerificationToken'] = token;
    }
    return headers;
}

// ==========================================
// 1. NOTIFICATION SYSTEM (Enhanced with Audio & Background Support)
// ==========================================

class NotificationManager {
    constructor() {
        // Desktop elements
        this.notificationBell = document.getElementById('notificationBell');
        this.notificationDropdown = document.getElementById('notificationDropdown');
        this.notificationList = document.getElementById('notificationList');
        this.notificationBadge = document.getElementById('notificationBadge');
        // Mobile elements
        this.mobileNotificationBell = document.getElementById('mobileNotificationBell');
        this.mobileNotificationList = document.getElementById('mobileNotificationList');
        this.mobileNotificationBadge = document.getElementById('mobileNotificationBadge');
        
        this.previousCount = -1; // -1 means first load, skip sound
        this.audioElement = null;
        this.audioContext = null;
        this.hubConnection = null;
        this.keepAliveInterval = null;
        this.isTabVisible = true;
        this.init();
    }

    init() {
        if (this.notificationBell || this.mobileNotificationBell) {
            this.initAudioSystem();
            this.initBrowserNotifications();
            this.initVisibilityHandler();
            this.loadNotifications();
            setInterval(() => this.loadNotifications(), 10000);
            this.initSignalR();
        }
    }

    // Initialize audio system with HTML Audio element (works in background)
    initAudioSystem() {
        // Create audio element for notification sound (works in background tabs)
        this.audioElement = document.createElement('audio');
        this.audioElement.id = 'notification-audio';
        this.audioElement.preload = 'auto';
        this.audioElement.volume = 0.7;
        // Local notification sound (copied from user desktop)
        this.audioElement.src = '/media/1.mp3';
        // Allow cross-origin fetching if served from CDN in future
        this.audioElement.crossOrigin = 'anonymous';
        document.body.appendChild(this.audioElement);

        // Also keep Web Audio API as fallback
        this.audioContext = null;
    }

    // Play notification sound (works even when tab is in background)
    playNotificationSound() {
        try {
            // Primary: Use HTML Audio element (better background support)
            if (this.audioElement) {
                this.audioElement.currentTime = 0;
                const playPromise = this.audioElement.play();
                if (playPromise !== undefined) {
                    playPromise.catch(err => {
                        // Fallback to Web Audio API
                        this.playWebAudioSound();
                    });
                }
            } else {
                this.playWebAudioSound();
            }
        } catch (e) {
            console.debug('Notification sound not available:', e);
        }
    }

    // Fallback: Web Audio API sound
    playWebAudioSound() {
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            const playTone = (freq, start, dur) => {
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();
                osc.connect(gain);
                gain.connect(ctx.destination);
                osc.frequency.setValueAtTime(freq, ctx.currentTime + start);
                gain.gain.setValueAtTime(0.4, ctx.currentTime + start);
                gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + start + dur);
                osc.start(ctx.currentTime + start);
                osc.stop(ctx.currentTime + start + dur);
            };
            playTone(880, 0, 0.15);
            playTone(1320, 0.18, 0.25);
        } catch (e) { /* silent */ }
    }

    // Request browser notification permission
    initBrowserNotifications() {
        if ('Notification' in window && Notification.permission === 'default') {
            // Request permission after user interaction
            document.addEventListener('click', () => {
                if (Notification.permission === 'default') {
                    Notification.requestPermission();
                }
            }, { once: true });
        }
    }

    // Show browser notification (works when tab is hidden)
    showBrowserNotification(title, message, link) {
        if (!('Notification' in window) || Notification.permission !== 'granted') return;
        
        try {
            const notification = new Notification(title, {
                body: message,
                icon: '/favicon.ico',
                badge: '/favicon.ico',
                tag: 'teknosos-notification',
                requireInteraction: false,
                silent: false
            });

            notification.onclick = () => {
                window.focus();
                if (link) window.location.href = link;
                notification.close();
            };

            // Auto close after 8 seconds
            setTimeout(() => notification.close(), 8000);
        } catch (e) {
            console.debug('Browser notification error:', e);
        }
    }

    // Handle tab visibility changes (keep session alive)
    initVisibilityHandler() {
        document.addEventListener('visibilitychange', () => {
            this.isTabVisible = !document.hidden;
            
            if (this.isTabVisible) {
                // Tab became visible - refresh connection and notifications
                this.loadNotifications();
                this.ensureSignalRConnection();
            }
        });

        // Also handle window focus/blur for additional reliability
        window.addEventListener('focus', () => {
            this.isTabVisible = true;
            this.loadNotifications();
            this.ensureSignalRConnection();
        });

        window.addEventListener('blur', () => {
            this.isTabVisible = false;
        });
    }

    // Ensure SignalR connection is active
    ensureSignalRConnection() {
        if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Disconnected) {
            this.hubConnection.start().catch(err => console.debug('SignalR reconnect error:', err));
        }
    }

    initSignalR() {
        if (typeof signalR === 'undefined') return;
        try {
            this.hubConnection = new signalR.HubConnectionBuilder()
                .withUrl('/notificationhub')
                .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Aggressive reconnect
                .configureLogging(signalR.LogLevel.Warning)
                .build();

            // Handle real-time notifications
            this.hubConnection.on('ReceiveNotification', (notification) => {
                this.loadNotifications();
                this.playNotificationSound();
                
                // For chat messages (type 8), link to Chat page; for others, link to DefectDetails
                const link = notification.serviceRequestId 
                    ? (notification.type === 8 
                        ? `/Chat?defectId=${notification.serviceRequestId}` 
                        : `/DefectDetails/${notification.serviceRequestId}`)
                    : null;
                
                // Show toast if tab is visible, otherwise show browser notification
                if (this.isTabVisible) {
                    this.showToast(notification.title, notification.message, notification.serviceRequestId, notification.type);
                } else {
                    this.showBrowserNotification(notification.title, notification.message, link);
                }
            });

            // Handle real-time chat messages
            this.hubConnection.on('ReceiveChatNotification', (data) => {
                this.loadNotifications();
                this.playNotificationSound();
                
                const title = 'Mesazh i ri';
                const message = data.senderName + ': ' + data.content;
                const link = data.serviceRequestId ? `/Chat?defectId=${data.serviceRequestId}` : null;
                
                if (this.isTabVisible) {
                    this.showToast(title, message, data.serviceRequestId, 8); // type 8 = NewMessage
                } else {
                    this.showBrowserNotification(title, message, link);
                }
            });

            // Connection state handlers
            this.hubConnection.onclose(() => {
                console.debug('SignalR connection closed');
                // Try to reconnect after a delay
                setTimeout(() => this.ensureSignalRConnection(), 5000);
            });

            this.hubConnection.onreconnecting(() => {
                console.debug('SignalR reconnecting...');
            });

            this.hubConnection.onreconnected(() => {
                console.debug('SignalR reconnected');
                this.loadNotifications();
            });

            // Start connection
            this.hubConnection.start().catch(err => console.debug('NotificationHub connect error:', err));

            // Keep-alive: ping server every 30 seconds to prevent disconnection
            this.keepAliveInterval = setInterval(() => {
                if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
                    this.hubConnection.invoke('Ping').catch(() => {});
                }
            }, 30000);

        } catch (e) { console.debug('SignalR not available for notifications'); }
    }

    showToast(title, message, serviceRequestId, notificationType = null) {
        // Create a toast notification
        let toastContainer = document.getElementById('notification-toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'notification-toast-container';
            toastContainer.className = 'position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }
        const toastId = 'toast-' + Date.now();
        // For chat messages (type 8), link to Chat page; for others, link to DefectDetails
        const link = serviceRequestId 
            ? (notificationType === 8 ? `/Chat?defectId=${serviceRequestId}` : `/DefectDetails/${serviceRequestId}`) 
            : '#';
        toastContainer.insertAdjacentHTML('beforeend', `
            <div id="${toastId}" class="toast show border-0 shadow-lg" role="alert" style="min-width:320px;">
                <div class="toast-header bg-primary text-white">
                    <i class="bi bi-bell-fill me-2"></i>
                    <strong class="me-auto">${Utils.escapeHtml(title)}</strong>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
                </div>
                <a href="${link}" class="text-decoration-none text-dark">
                    <div class="toast-body">${Utils.escapeHtml(message)}</div>
                </a>
            </div>
        `);
        setTimeout(() => document.getElementById(toastId)?.remove(), 8000);
    }

    async loadNotifications() {
        try {
            const response = await fetch('/api/notifications/unread', {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!response.ok) return;

            const notifications = await response.json();
            const newCount = notifications.length;

            // Play sound if new notifications arrived (skip on first load)
            if (this.previousCount >= 0 && newCount > this.previousCount) {
                this.playNotificationSound();
                // Show browser notification if tab is hidden
                if (!this.isTabVisible && notifications.length > 0) {
                    const latestNotif = notifications[0];
                    const link = latestNotif.serviceRequestId ? `/DefectDetails/${latestNotif.serviceRequestId}` : null;
                    this.showBrowserNotification(latestNotif.title, latestNotif.message, link);
                }
            }
            this.previousCount = newCount;

            this.updateBadge(newCount);
            this.renderNotifications(notifications);
        } catch (error) {
            console.debug('Notifications not available:', error.message);
        }
    }

    updateBadge(count) {
        // Update desktop badge
        if (this.notificationBadge) {
            if (count > 0) {
                this.notificationBadge.textContent = count > 9 ? '9+' : count;
                this.notificationBadge.style.display = '';
                this.notificationBadge.classList.add('bg-danger');
            } else {
                this.notificationBadge.style.display = 'none';
            }
        }
        // Update mobile badge
        if (this.mobileNotificationBadge) {
            if (count > 0) {
                this.mobileNotificationBadge.textContent = count > 9 ? '9+' : count;
                this.mobileNotificationBadge.style.display = '';
            } else {
                this.mobileNotificationBadge.style.display = 'none';
            }
        }
    }

    renderNotifications(notifications) {
        const notificationHtml = this.generateNotificationHtml(notifications);
        
        // Update desktop list
        if (this.notificationList) {
            this.notificationList.innerHTML = notificationHtml;
        }
        // Update mobile list
        if (this.mobileNotificationList) {
            this.mobileNotificationList.innerHTML = notificationHtml;
        }
    }

    generateNotificationHtml(notifications) {
        if (!notifications || notifications.length === 0) {
            return '<div class="p-3 text-center text-muted"><small>Nuk ka njoftime të reja</small></div>';
        }

        return notifications.map(n => {
            const icon = this.getNotificationIcon(n.type);
            // For chat messages (type 8), link to Chat page; for others, link to DefectDetails
            const href = n.type === 8 
                ? `/Chat?defectId=${n.serviceRequestId}` 
                : `/DefectDetails/${n.serviceRequestId}`;
            return `
            <a class="dropdown-item py-2 border-bottom ${n.isRead ? '' : 'fw-bold bg-light'}" 
               href="${href}" 
               onclick="notificationManager.markAsRead(${n.id})">
                <div class="d-flex align-items-start gap-2">
                    <i class="bi ${icon} text-primary mt-1"></i>
                    <div class="flex-grow-1">
                        <div class="small fw-semibold">${Utils.escapeHtml(n.title)}</div>
                        <div class="small text-muted text-truncate" style="max-width:250px;">${Utils.escapeHtml(n.message)}</div>
                        <div class="small text-muted mt-1"><i class="bi bi-clock"></i> ${Utils.formatDate(n.createdDate)}</div>
                    </div>
                </div>
            </a>`;
        }).join('') + `
            <div class="p-2 text-center border-top">
                <a href="#" class="small text-primary" onclick="notificationManager.markAllAsRead(); return false;">Shëno të gjitha si të lexuara</a>
            </div>`;
    }

    getNotificationIcon(type) {
        const icons = {
            0: 'bi-plus-circle',       // CaseCreated
            1: 'bi-people',            // CaseMatched
            2: 'bi-gear',              // CaseInProgress
            3: 'bi-check-circle',      // CaseCompleted
            4: 'bi-star',              // ReviewReceived
            5: 'bi-hand-index',        // NewInterest
            6: 'bi-person-check',      // TechnicianAssigned
            7: 'bi-exclamation-triangle', // Emergency
            8: 'bi-chat-dots'          // NewMessage
        };
        return icons[type] || 'bi-bell';
    }

    async markAsRead(notificationId) {
        try {
            await fetch(`/api/notifications/${notificationId}/read`, {
                method: 'POST',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
        } catch (e) { /* silent */ }
    }

    async markAllAsRead() {
        try {
            await fetch('/api/notifications/read-all', {
                method: 'POST',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            this.loadNotifications();
        } catch (e) { /* silent */ }
    }
}

// ==========================================
// 2. UI ENHANCEMENTS
// ==========================================

class UIEnhancer {
    constructor() {
        this.init();
    }

    init() {
        this.setupLazyLoading();
        this.setupSmoothScrolling();
        this.setupFormValidation();
        this.setupTooltips();
        this.setupLoadingStates();
        this.setupAnimations();
    }

    setupLazyLoading() {
        if ('IntersectionObserver' in window) {
            const observer = new IntersectionObserver((entries, obs) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        if (img.dataset.src) {
                            img.src = img.dataset.src;
                            img.classList.remove('lazy');
                        }
                        obs.unobserve(img);
                    }
                });
            });
            document.querySelectorAll('img[data-src]').forEach(img => observer.observe(img));
        }
    }

    setupSmoothScrolling() {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                if (href === '#') return;
                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            });
        });
    }

    setupFormValidation() {
        document.querySelectorAll('form').forEach(form => {
            form.addEventListener('submit', this.handleFormSubmit.bind(this));
            form.querySelectorAll('input, textarea, select').forEach(input => {
                input.addEventListener('blur', this.validateField.bind(this));
                input.addEventListener('input', this.clearFieldError.bind(this));
            });
        });
    }

    validateField(e) {
        const field = e.target;
        const value = field.value.trim();
        let isValid = true;
        let message = '';

        if (field.hasAttribute('required') && !value) {
            isValid = false;
            message = 'This field is required';
        } else if (field.type === 'email' && value && !Utils.isValidEmail(value)) {
            isValid = false;
            message = 'Please enter a valid email address';
        }

        this.updateFieldState(field, isValid, message);
    }

    updateFieldState(field, isValid, message) {
        const parent = field.closest('.form-group') || field.parentElement;
        let errorEl = parent.querySelector('.field-error');

        if (!isValid) {
            field.classList.add('is-invalid');
            if (!errorEl) {
                errorEl = document.createElement('div');
                errorEl.className = 'field-error text-danger small mt-1';
                parent.appendChild(errorEl);
            }
            errorEl.textContent = message;
        } else {
            field.classList.remove('is-invalid');
            if (errorEl) errorEl.remove();
        }
    }

    clearFieldError(e) {
        const field = e.target;
        field.classList.remove('is-invalid');
        const parent = field.closest('.form-group') || field.parentElement;
        const errorEl = parent.querySelector('.field-error');
        if (errorEl) errorEl.remove();
    }

    handleFormSubmit(e) {
        const form = e.target;
        const btn = form.querySelector('[type="submit"]');
        if (btn && !btn.disabled) {
            btn.classList.add('loading');
            btn.disabled = true;
            setTimeout(() => {
                btn.classList.remove('loading');
                btn.disabled = false;
            }, 5000);
        }
    }

    setupTooltips() {
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
                new bootstrap.Tooltip(el);
            });
        }
    }

    setupLoadingStates() {
        document.addEventListener('click', (e) => {
            const btn = e.target.closest('.btn[data-loading]');
            if (btn) {
                const originalText = btn.innerHTML;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> Loading...';
                btn.disabled = true;
                setTimeout(() => {
                    btn.innerHTML = originalText;
                    btn.disabled = false;
                }, 10000);
            }
        });
    }

    setupAnimations() {
        if ('IntersectionObserver' in window) {
            const observer = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('fade-in');
                        observer.unobserve(entry.target);
                    }
                });
            }, { threshold: 0.1 });
            document.querySelectorAll('.animate-on-scroll').forEach(el => observer.observe(el));
        }
    }
}

// ==========================================
// 3. DASHBOARD MANAGER
// ==========================================

class DashboardManager {
    constructor() {
        this.refreshInterval = null;
        this.init();
    }

    init() {
        this.setupRefreshButton();
        this.setupFilters();
        this.startAutoRefresh();
    }

    startAutoRefresh() {
        this.refreshInterval = setInterval(() => this.refreshData(), 60000);
    }

    async refreshData() {
        try {
            const response = await fetch('/api/dashboard/status');
            if (response.ok) {
                const data = await response.json();
                this.updateUI(data);
            }
        } catch (error) {
            console.debug('Dashboard refresh not available:', error.message);
        }
    }

    updateUI(data) {
        if (data && data.stats) {
            Object.keys(data.stats).forEach(key => {
                const el = document.querySelector(`[data-stat="${key}"]`);
                if (el) el.textContent = data.stats[key];
            });
        }
    }

    setupRefreshButton() {
        const btn = document.querySelector('[data-action="refresh"]');
        if (btn) {
            btn.addEventListener('click', () => {
                btn.classList.add('loading');
                this.refreshData().finally(() => btn.classList.remove('loading'));
            });
        }
    }

    setupFilters() {
        document.querySelectorAll('.dashboard-filter').forEach(select => {
            select.addEventListener('change', (e) => {
                const filterType = e.target.name;
                const value = e.target.value;
                document.querySelectorAll(`[data-${filterType}]`).forEach(item => {
                    item.style.display = (value === 'all' || item.dataset[filterType] === value) ? '' : 'none';
                });
            });
        });
    }
}

// ==========================================
// 4. MESSAGE MANAGER
// ==========================================

class MessageManager {
    async sendMessage(receiverId, serviceRequestId, content) {
        try {
            const response = await fetch('/api/messages', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ receiverId, serviceRequestId, content })
            });
            if (!response.ok) throw new Error('Failed to send message');
            return await response.json();
        } catch (error) {
            console.error('Error sending message:', error);
            throw error;
        }
    }

    async getConversation(userId, serviceRequestId) {
        try {
            const response = await fetch(`/api/messages/conversation/${userId}/${serviceRequestId}`);
            if (!response.ok) throw new Error('Failed to load conversation');
            return await response.json();
        } catch (error) {
            console.error('Error loading conversation:', error);
            return [];
        }
    }
}

// ==========================================
// 5. INTEREST MANAGER
// ==========================================

class InterestManager {
    async expressInterest(serviceRequestId, preventiveOffer, estimatedCost, estimatedTime) {
        try {
            const response = await fetch(`/api/technicianinterests/${serviceRequestId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ preventiveOffer, estimatedCost, estimatedTimeInHours: estimatedTime })
            });
            if (!response.ok) throw new Error('Failed to express interest');
            return await response.json();
        } catch (error) {
            console.error('Error expressing interest:', error);
            throw error;
        }
    }

    async withdrawInterest(interestId) {
        try {
            const response = await fetch(`/api/technicianinterests/${interestId}/withdraw`, { method: 'DELETE' });
            if (!response.ok) throw new Error('Failed to withdraw interest');
        } catch (error) {
            console.error('Error withdrawing interest:', error);
            throw error;
        }
    }
}

// ==========================================
// 6. UTILITY FUNCTIONS
// ==========================================

class Utils {
    static showToast(message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px; max-width: 400px;';
        toast.innerHTML = `
            ${Utils.escapeHtml(message)}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        document.body.appendChild(toast);
        setTimeout(() => { if (toast.parentNode) toast.remove(); }, 5000);
    }

    static formatDate(date) {
        if (!date) return '';
        return new Intl.DateTimeFormat('en-US', {
            year: 'numeric', month: 'short', day: 'numeric',
            hour: '2-digit', minute: '2-digit'
        }).format(new Date(date));
    }

    static formatCurrency(amount) {
        return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'EUR' }).format(amount);
    }

    static debounce(func, wait) {
        let timeout;
        return function (...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }

    static isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    static escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text || '';
        return div.innerHTML;
    }

    static async copyToClipboard(text) {
        try {
            await navigator.clipboard.writeText(text);
            Utils.showToast('Copied to clipboard!', 'success');
        } catch (err) {
            Utils.showToast('Failed to copy', 'warning');
        }
    }
}

// ==========================================
// 7. INITIALIZATION
// ==========================================

document.addEventListener('DOMContentLoaded', function () {
    // Core managers
    window.notificationManager = new NotificationManager();
    window.uiEnhancer = new UIEnhancer();
    window.dashboardManager = new DashboardManager();
    window.messageManager = new MessageManager();
    window.interestManager = new InterestManager();

    // Close dropdowns on outside click
    document.addEventListener('click', function (event) {
        const bell = document.getElementById('notificationBell');
        const dropdown = document.getElementById('notificationDropdown');
        if (bell && dropdown && !bell.contains(event.target) && !dropdown.contains(event.target)) {
            dropdown.style.display = 'none';
        }
    });

    // Global unhandled promise rejection handler
    window.addEventListener('unhandledrejection', function (event) {
        console.error('Unhandled promise rejection:', event.reason);
    });

    // ==========================================
    // 8. DARK MODE TOGGLE — Only enabled in admin area
    // ==========================================
    const themeToggle = document.getElementById('themeToggle');
    // Only allow theme toggling in admin area
    const isAdminPage = window.location.pathname.toLowerCase().startsWith('/admin');
    const ThemeManager = {
        getSystemPreference() {
            return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        },
        getCurrentTheme() {
            if (!isAdminPage) return 'light';
            const saved = localStorage.getItem('ts-theme');
            if (saved === 'light' || saved === 'dark') return saved;
            return this.getSystemPreference();
        },
        setTheme(theme, save = true) {
            if (!isAdminPage) theme = 'light';
            document.documentElement.setAttribute('data-theme', theme);
            if (isAdminPage && save) localStorage.setItem('ts-theme', theme);
            this.updateIcon();
            window.dispatchEvent(new CustomEvent('themechange', { detail: { theme } }));
        },
        updateIcon() {
            if (!themeToggle) return;
            const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
            themeToggle.innerHTML = isDark
                ? '<i class="bi bi-sun-fill"></i>'
                : '<i class="bi bi-moon-fill"></i>';
            themeToggle.setAttribute('aria-label', isDark ? 'Switch to light mode' : 'Switch to dark mode');
            themeToggle.setAttribute('title', isDark ? 'Switch to light mode' : 'Switch to dark mode');
        },
        toggle() {
            if (!isAdminPage) return;
            const current = document.documentElement.getAttribute('data-theme');
            const newTheme = current === 'dark' ? 'light' : 'dark';
            this.setTheme(newTheme);
        },
        init() {
            this.setTheme(this.getCurrentTheme(), false);
            if (isAdminPage) {
                window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                    const saved = localStorage.getItem('ts-theme');
                    if (!saved) {
                        this.setTheme(e.matches ? 'dark' : 'light', false);
                    }
                });
            }
        }
    };
    ThemeManager.init();
    if (themeToggle && isAdminPage) {
        themeToggle.addEventListener('click', () => ThemeManager.toggle());
        themeToggle.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                ThemeManager.toggle();
            }
        });
    }
    window.TeknoSOSTheme = ThemeManager;

    // ==========================================
    // 9. SCROLL FADE-UP ANIMATIONS
    // ==========================================
    const fadeEls = document.querySelectorAll('.fade-up');
    if (fadeEls.length > 0 && 'IntersectionObserver' in window) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry, idx) => {
                if (entry.isIntersecting) {
                    // Stagger the animations slightly
                    setTimeout(() => {
                        entry.target.classList.add('visible');
                    }, idx * 60);
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });

        fadeEls.forEach(el => observer.observe(el));
    } else {
        // Fallback: make all visible immediately
        fadeEls.forEach(el => el.classList.add('visible'));
    }

    // ==========================================
    // 10. NAVBAR SCROLL EFFECT
    // ==========================================
    const navbar = document.querySelector('.navbar.sticky-top');
    if (navbar) {
        let lastScroll = 0;
        window.addEventListener('scroll', () => {
            const scrollY = window.scrollY;
            if (scrollY > 10) {
                navbar.style.boxShadow = 'var(--ts-shadow-md)';
            } else {
                navbar.style.boxShadow = 'var(--ts-shadow-sm)';
            }
            lastScroll = scrollY;
        }, { passive: true });
    }

    // ==========================================
    // 11. MOBILE SLIDE MENU
    // ==========================================
    const MobileMenuManager = {
        menuToggle: document.getElementById('mobileMenuToggle'),
        menuClose: document.getElementById('mobileMenuClose'),
        slideMenu: document.getElementById('mobileSlideMenu'),
        overlay: document.getElementById('mobileMenuOverlay'),
        mobileThemeToggle: document.getElementById('mobileThemeToggle'),
        
        init() {
            if (!this.slideMenu) return;
            
            // Open menu
            if (this.menuToggle) {
                this.menuToggle.addEventListener('click', () => this.open());
            }
            
            // Close menu
            if (this.menuClose) {
                this.menuClose.addEventListener('click', () => this.close());
            }
            
            // Close on overlay click
            if (this.overlay) {
                this.overlay.addEventListener('click', () => this.close());
            }
            
            // Close on escape key
            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape' && this.isOpen()) {
                    this.close();
                }
            });
            
            // Mobile theme toggle
            if (this.mobileThemeToggle) {
                this.mobileThemeToggle.addEventListener('click', () => {
                    if (window.TeknoSOSTheme) {
                        window.TeknoSOSTheme.toggle();
                        this.updateThemeButton();
                    }
                });
                this.updateThemeButton();
            }
            
            // Set active state for current page
            this.setActiveLink();
            
            // Handle swipe to close
            this.initSwipeGesture();
        },
        
        open() {
            if (!this.slideMenu || !this.overlay) return;
            this.slideMenu.classList.add('active');
            this.overlay.classList.add('active');
            document.body.style.overflow = 'hidden';
            // Focus management for accessibility
            this.menuClose?.focus();
        },
        
        close() {
            if (!this.slideMenu || !this.overlay) return;
            this.slideMenu.classList.remove('active');
            this.overlay.classList.remove('active');
            document.body.style.overflow = '';
            // Return focus to toggle button
            this.menuToggle?.focus();
        },
        
        isOpen() {
            return this.slideMenu?.classList.contains('active') ?? false;
        },
        
        setActiveLink() {
            const path = window.location.pathname.toLowerCase();
            const links = this.slideMenu?.querySelectorAll('.mobile-menu-link');
            links?.forEach(link => {
                const href = link.getAttribute('href')?.toLowerCase();
                if (href && (path === href || (path === '/' && href === '/'))) {
                    link.classList.add('active');
                }
            });
        },
        
        updateThemeButton() {
            if (!this.mobileThemeToggle) return;
            const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
            this.mobileThemeToggle.innerHTML = isDark 
                ? '<i class="bi bi-sun"></i> Light Mode' 
                : '<i class="bi bi-moon"></i> Dark Mode';
        },
        
        initSwipeGesture() {
            if (!this.slideMenu) return;
            
            let startX = 0;
            let currentX = 0;
            let isDragging = false;
            
            this.slideMenu.addEventListener('touchstart', (e) => {
                startX = e.touches[0].clientX;
                isDragging = true;
            }, { passive: true });
            
            this.slideMenu.addEventListener('touchmove', (e) => {
                if (!isDragging) return;
                currentX = e.touches[0].clientX;
                const diff = startX - currentX;
                
                // Only allow dragging to the left to close
                if (diff > 0 && diff < 100) {
                    this.slideMenu.style.transform = `translateX(-${diff}px)`;
                }
            }, { passive: true });
            
            this.slideMenu.addEventListener('touchend', () => {
                if (!isDragging) return;
                isDragging = false;
                
                const diff = startX - currentX;
                
                // If dragged more than 80px, close the menu
                if (diff > 80) {
                    this.close();
                }
                
                // Reset transform
                this.slideMenu.style.transform = '';
            }, { passive: true });
        }
    };
    
    MobileMenuManager.init();
    
    // Listen for theme changes to update mobile button
    window.addEventListener('themechange', () => {
        MobileMenuManager.updateThemeButton();
    });

    // ==========================================
    // 12. MOBILE BOTTOM NAV ACTIVE STATE
    // ==========================================
    const MobileNavManager = {
        init() {
            const path = window.location.pathname.toLowerCase();
            const mobileNavLinks = document.querySelectorAll('.mobile-bottom-nav .mobile-nav-link');
            
            mobileNavLinks.forEach(link => {
                const dataPage = link.getAttribute('data-page');
                const href = link.getAttribute('href')?.toLowerCase();
                
                // Reset all
                link.classList.remove('active');
                
                // Check for match
                if (dataPage === 'home' && path === '/') {
                    link.classList.add('active');
                } else if (href && path === href) {
                    link.classList.add('active');
                } else if (href && path.startsWith(href) && href !== '/') {
                    link.classList.add('active');
                }
            });
        }
    };
    
    MobileNavManager.init();

    // ==========================================
    // 13. TOUCH FEEDBACK FOR MOBILE
    // ==========================================
    if ('ontouchstart' in window) {
        // Add ripple class to all buttons and cards on mobile
        document.querySelectorAll('.btn, .card, .mobile-nav-link, .mobile-menu-link').forEach(el => {
            if (!el.classList.contains('ripple')) {
                el.classList.add('ripple');
            }
        });
    }

    // ==========================================
    // 14. PULL TO REFRESH INDICATOR (Visual only)
    // ==========================================
    const PullToRefreshIndicator = {
        indicator: null,
        startY: 0,
        
        init() {
            // Only on mobile
            if (window.innerWidth > 768) return;
            
            // Create indicator element
            this.indicator = document.createElement('div');
            this.indicator.className = 'pull-to-refresh';
            this.indicator.innerHTML = '<i class="bi bi-arrow-clockwise"></i> Pull to refresh';
            document.body.appendChild(this.indicator);
            
            let pulling = false;
            
            window.addEventListener('touchstart', (e) => {
                if (window.scrollY === 0) {
                    this.startY = e.touches[0].clientY;
                    pulling = true;
                }
            }, { passive: true });
            
            window.addEventListener('touchmove', (e) => {
                if (!pulling) return;
                const currentY = e.touches[0].clientY;
                const diff = currentY - this.startY;
                
                if (diff > 60 && window.scrollY === 0) {
                    this.indicator.classList.add('active');
                }
            }, { passive: true });
            
            window.addEventListener('touchend', () => {
                if (this.indicator.classList.contains('active')) {
                    // Trigger page refresh after showing indicator
                    setTimeout(() => {
                        window.location.reload();
                    }, 500);
                }
                pulling = false;
            }, { passive: true });
        }
    };
    
    PullToRefreshIndicator.init();
});
