document.addEventListener('DOMContentLoaded', () => {
    console.log('site.js: Страница загружена, начинаю инициализацию...');

    const token = localStorage.getItem('authToken');
    console.log('site.js: Токен из localStorage:', token ? 'найден' : 'не найден');

    const isAuthPage = window.location.pathname.endsWith('auth.html');
    const isHomePage = window.location.pathname === '/' || window.location.pathname.endsWith('index.html');

    // --- КОНТРОЛЬ ДОСТУПА ---
    // Если токена нет, и мы не на главной и не на странице входа - перенаправляем на вход
    if (!token && !isAuthPage && !isHomePage) {
        console.log('site.js: Доступ запрещен. Перенаправляем на страницу входа.');
        window.location.href = '/auth.html';
        return; // Прерываем выполнение скрипта
    }

    // 2. Обновляем кнопки на ВСЕХ страницах
    updateAuthButtons(token);

    // 3. Загружаем контент для остальных страниц, если токен есть
    if (token) {
        loadPageContent();
    }
});

function updateAuthButtons(token) {
    const logoutBtn = document.getElementById('logout-btn');
    const loginLinkContainer = document.getElementById('login-link-container');

    if (logoutBtn && loginLinkContainer) {
        if (token) {
            // Пользователь авторизован: показываем "Выйти", скрываем "Вход"
            logoutBtn.style.display = 'inline-block';
            loginLinkContainer.style.display = 'none';
        } else {
            // Пользователь не авторизован: показываем "Вход", скрываем "Выйти"
            logoutBtn.style.display = 'none';
            loginLinkContainer.style.display = 'inline';
        }
        console.log(`site.js: Кнопки авторизации обновлены. Токен: ${token ? 'есть' : 'нету'}.`);
    } else {
        // Если элементы не найдены на странице, выводим ошибку в консоль для отладки
        console.error("site.js: Не удалось найти элементы кнопок авторизации на странице.");
    }
}

async function apiRequest(url, options = {}) {
    const headers = options.headers || {};
    const token = localStorage.getItem('authToken');

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
        console.log(`site.js: Запрос к ${url} с токеном`);
    }

    headers['Content-Type'] = 'application/json';

    try {
        const response = await fetch(url, {
            ...options,
            headers
        });

        if (response.status === 401) {
            console.log('site.js: 401 Unauthorized - сессия истекла, удаляю токен.');
            localStorage.removeItem('authToken');
            // Если получили 401, перенаправляем на страницу входа
            window.location.href = '/auth.html';
            return Promise.reject("Unauthorized");
        }

        return response;
    } catch (error) {
        console.error(`site.js: Ошибка запроса к ${url}:`, error);
        throw error;
    }
}

function loadPageContent() {
    const path = window.location.pathname;
    console.log(`site.js: Загрузка контента для страницы: ${path}`);

    if (path.includes('homework') && document.getElementById('homework-container')) {
        loadHomeworks();
    } else if (path.includes('consultation')) {
        // Контент для консультации загружается своим скриптом на странице
    }
    // Функция loadLessons теперь находится в materials.html
}

// === ФУНКЦИЯ ЗАГРУЗКИ ДОМАШНИХ ЗАДАНИЙ (без изменений) ===

async function loadHomeworks() {
    try {
        console.log('site.js: Загрузка домашних заданий...');
        const response = await apiRequest('/api/homeworks');

        if (!response.ok) {
            throw new Error('Ошибка загрузки заданий');
        }

        const homeworks = await response.json();
        const container = document.getElementById('homework-container');

        if (!container) {
            console.error('site.js: Контейнер для домашних заданий не найден');
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

        console.log('site.js: Домашние задания успешно загружены');
    } catch (error) {
        console.error('site.js: Ошибка загрузки заданий:', error);
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
            statusElement.textContent = result.error || 'Ошибка отправки';
            statusElement.style.color = 'red';
        }
    } catch (error) {
        statusElement.textContent = 'Ошибка подключения';
        statusElement.style.color = 'red';
    }
}

// === ОБРАБОТКА ВЫХОДА ===

// Вешаем обработчик на кнопку, если она есть на странице
document.getElementById('logout-btn')?.addEventListener('click', () => {
    console.log('site.js: Попытка выхода');
    localStorage.removeItem('authToken');
    alert('Вы успешно вышли из системы');
    window.location.href = '/auth.html';
});

console.log('site.js: Скрипт полностью загружен.');