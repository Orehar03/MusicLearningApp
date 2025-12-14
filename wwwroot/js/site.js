// 🔥 ЧИТАЕМ ТОКЕН ИЗ localStorage СРАЗУ ПРИ ЗАГРУЗКЕ СКРИПТА
let authToken = localStorage.getItem('authToken') || null;
console.log('🔑 Токен при инициализации скрипта:', authToken ? 'найден' : 'не найден');

document.addEventListener('DOMContentLoaded', () => {
    console.log('Страница мониторинга загружена');
    updateAuthButtons();

    // 🔥 ПРОВЕРЯЕМ СЕССИЮ ТОЛЬКО ПОСЛЕ ПОЛНОЙ ЗАГРУЗКИ DOM
    setTimeout(() => {
        checkSession();
        loadPageContent();
    }, 100);
});

function checkSession() {
    console.log('Проверка сессии...');

    // Читаем актуальное значение из localStorage
    const storedToken = localStorage.getItem('authToken');
    authToken = storedToken; // Обновляем глобальную переменную

    if (!storedToken) {
        console.log('Токен отсутствует в localStorage');
        handleUnauthorized();
        return;
    }

    // Проверяем валидность токена
    const tokenParts = storedToken.split('.');
    if (tokenParts.length !== 3) {
        console.log('Невалидный формат токена');
        handleUnauthorized();
        return;
    }

    console.log('Сессия активна');
    updateAuthButtons();
}

function handleUnauthorized() {
    console.log('Обработка неавторизованного доступа');

    // Не перенаправляем, если уже на странице входа
    if (window.location.pathname === '/auth.html') {
        console.log('Уже на странице входа, перенаправление не требуется');
        return;
    }

    // Показываем предупреждение с задержкой
    setTimeout(() => {
        alert('Сессия истекла или отсутствует. Пожалуйста, войдите снова.');
        window.location.href = '/auth.html';
    }, 300);
}

function updateAuthButtons() {
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.style.display = authToken ? 'inline-block' : 'none';
        console.log(`Кнопка выхода: ${authToken ? 'видна' : 'скрыта'}`);
    }
}

async function apiRequest(url, options = {}) {
    const headers = options.headers || {};
    const storedToken = localStorage.getItem('authToken');

    if (storedToken) {
        headers['Authorization'] = `Bearer ${storedToken}`;
        console.log(`Запрос к ${url} с токеном`);
    }

    headers['Content-Type'] = 'application/json';

    try {
        const response = await fetch(url, {
            ...options,
            headers
        });

        if (response.status === 401) {
            console.log('401 Unauthorized - сессия истекла');
            localStorage.removeItem('authToken');
            handleUnauthorized();
            return response;
        }

        return response;
    } catch (error) {
        console.error(`Ошибка запроса к ${url}:`, error);
        throw error;
    }
}

function loadPageContent() {
    const path = window.location.pathname;
    console.log(`Загрузка контента для страницы: ${path}`);

    if (path.includes('materials')) {
        loadLessons();
    } else if (path.includes('homework')) {
        loadHomeworks();
    } else if (path.includes('consultation')) {
        // Уже загружено в самой странице
    }
}

// === ФУНКЦИИ ЗАГРУЗКИ КОНТЕНТА ===

async function loadLessons() {
    try {
        console.log('Загрузка уроков...');
        const response = await apiRequest('/api/lessons');

        if (!response.ok) {
            throw new Error('Ошибка загрузки уроков');
        }

        const lessons = await response.json();
        const container = document.getElementById('lessons-container');

        if (!container) {
            console.error('❌ Контейнер для уроков не найден');
            return;
        }

        container.innerHTML = lessons.map(lesson => `
            <div class="lesson-item">
                <h2>${lesson.title}</h2>
                <p>${lesson.description}</p>
                <div class="video-container">
                    <video controls>
                        <source src="${lesson.videoPath}" type="video/mp4">
                        Ваш браузер не поддерживает видео
                    </video>
                </div>
            </div>
        `).join('');

        console.log('Уроки успешно загружены');
    } catch (error) {
        console.error('Ошибка загрузки уроков:', error);
        alert('Не удалось загрузить уроки. Попробуйте обновить страницу.');
    }
}

async function loadHomeworks() {
    try {
        console.log('📝 Загрузка домашних заданий...');
        const response = await apiRequest('/api/homeworks');

        if (!response.ok) {
            throw new Error('Ошибка загрузки заданий');
        }

        const homeworks = await response.json();
        const container = document.getElementById('homework-container');

        if (!container) {
            console.error('Контейнер для домашних заданий не найден');
            return;
        }

        container.innerHTML = homeworks.map(hw => {
            const deadline = new Date(hw.deadline);
            const now = new Date();
            const timeLeft = deadline - now;

            let timeText = '';
            if (timeLeft > 0) {
                const days = Math.floor(timeLeft / (1000 * 60 * 60 * 24));
                const hours = Math.floor((timeLeft % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
                const minutes = Math.floor((timeLeft % (1000 * 60 * 60)) / (1000 * 60));
                const seconds = Math.floor((timeLeft % (1000 * 60)) / 1000);

                if (days > 0) {
                    timeText = `Осталось: ${days} дн. ${hours} ч.`;
                } else {
                    timeText = `Осталось: ${hours} ч. ${minutes} мин. ${seconds} сек.`;
                }
            } else {
                timeText = 'Дедлайн истек';
            }

            return `
                <div class="homework-item">
                    <h2>Домашнее задание</h2>
                    <p>${hw.description}</p>
                    <p class="deadline-info">${timeText}</p>
                    ${timeLeft > 0 ? `
                        <textarea id="answer-${hw.id}" rows="4" placeholder="Ваш ответ"></textarea>
                        <button onclick="submitHomework(${hw.id})">Отправить</button>
                        <p id="status-${hw.id}"></p>
                    ` : ''}
                </div>
            `;
        }).join('');

        console.log('Домашние задания успешно загружены');
    } catch (error) {
        console.error('❌ Ошибка загрузки заданий:', error);
        alert('Не удалось загрузить задания. Попробуйте обновить страницу.');
    }
}

async function submitHomework(homeworkId) {
    const textAnswer = document.getElementById(`answer-${homeworkId}`).value;
    const statusElement = document.getElementById(`status-${homeworkId}`);

    try {
        const response = await apiRequest('/api/submissions', {
            method: 'POST',
            body: JSON.stringify({ homeworkId, textAnswer })
        });

        const result = await response.json();
        if (response.ok) {
            statusElement.textContent = 'Отправлено!';
            statusElement.style.color = 'green';
        } else {
            statusElement.textContent = result.error || '❌ Ошибка отправки';
            statusElement.style.color = 'red';
        }
    } catch (error) {
        statusElement.textContent = '❌ Ошибка подключения';
        statusElement.style.color = 'red';
    }
}

// === ОБРАБОТКА ВЫХОДА ===

document.getElementById('logout-btn')?.addEventListener('click', () => {
    console.log('🚪 Попытка выхода');

    localStorage.removeItem('authToken');
    authToken = null;

    updateAuthButtons();

    // Показываем уведомление и перенаправляем
    setTimeout(() => {
        alert('Вы успешно вышли из системы');
        window.location.href = '/';
    }, 300);
});

console.log('Скрипт site.js полностью загружен и инициализирован');