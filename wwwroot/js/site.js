// 🔥 ВСЕГДА читаем токен из localStorage при загрузке
let authToken = localStorage.getItem('authToken') || null;
console.log('Токен при загрузке:', authToken ? 'есть' : 'нет');

document.addEventListener('DOMContentLoaded', () => {
    updateAuthButtons();
    loadPageContent();

    // 🔥 Автоматическая проверка сессии
    checkSession();
});

function checkSession() {
    if (!authToken && window.location.pathname !== '/auth.html') {
        console.log('Нет токена, редирект на вход');
        window.location.href = '/auth.html';
    }
}

function updateAuthButtons() {
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.style.display = authToken ? 'inline-block' : 'none';
    }
}

// 🔥 Универсальная функция с отладкой
async function apiRequest(url, options = {}) {
    const headers = options.headers || {};
    if (authToken) {
        headers['Authorization'] = `Bearer ${authToken}`;
        console.log(`📡 Запрос к ${url} с токеном`);
    }

    headers['Content-Type'] = 'application/json';

    try {
        const response = await fetch(url, {
            ...options,
            headers
        });

        if (response.status === 401) {
            console.log('⚠️ 401 Unauthorized - сессия истекла');
            localStorage.removeItem('authToken');
            authToken = null;
            alert('Сессия истекла. Пожалуйста, войдите снова.');
            window.location.href = '/auth.html';
            return null;
        }

        return response;
    } catch (error) {
        console.error(`🔥 Ошибка запроса к ${url}:`, error);
        throw error;
    }
}