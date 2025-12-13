let authToken = localStorage.getItem('authToken') || null;

document.addEventListener('DOMContentLoaded', () => {
    updateAuthButtons();
    loadPageContent();
});

function updateAuthButtons() {
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.style.display = authToken ? 'inline-block' : 'none';
    }
}

function loadPageContent() {
    const path = window.location.pathname;
    if (path.includes('materials')) {
        loadLessons();
    } else if (path.includes('homework')) {
        loadHomeworks();
    }
}

// 🔥 Универсальная функция для API-запросов с токеном
async function apiRequest(url, options = {}) {
    const headers = options.headers || {};
    if (authToken) {
        headers['Authorization'] = `Bearer ${authToken}`;
    }
    headers['Content-Type'] = 'application/json';

    return fetch(url, {
        ...options,
        headers
    });
}

// Загрузка уроков
async function loadLessons() {
    try {
        const response = await apiRequest('/api/lessons');
        if (!response.ok) {
            if (response.status === 401) {
                alert('Сессия истекла. Пожалуйста, войдите снова.');
                localStorage.removeItem('authToken');
                window.location.href = '/auth.html';
                return;
            }
            throw new Error('Ошибка загрузки');
        }
        const lessons = await response.json();
        const container = document.getElementById('lessons-container');
        container.innerHTML = lessons.map(lesson => `
            <div class="lesson-item">
                <h2>${lesson.title}</h2>
                <p>${lesson.description}</p>
                <div class="video-container">
                    <video controls width="100%">
                        <source src="${lesson.videoPath}" type="video/mp4">
                        Ваш браузер не поддерживает видео
                    </video>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('Ошибка загрузки уроков:', error);
        alert('Не удалось загрузить уроки. Попробуйте обновить страницу.');
    }
}

// Загрузка домашних заданий
async function loadHomeworks() {
    try {
        const response = await apiRequest('/api/homeworks');
        if (!response.ok) {
            if (response.status === 401) {
                alert('Сессия истекла. Пожалуйста, войдите снова.');
                localStorage.removeItem('authToken');
                window.location.href = '/auth.html';
                return;
            }
            throw new Error('Ошибка загрузки');
        }
        const homeworks = await response.json();
        const container = document.getElementById('homework-container');
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
    } catch (error) {
        console.error('Ошибка загрузки заданий:', error);
        alert('Не удалось загрузить задания. Попробуйте обновить страницу.');
    }
}

// Отправка домашнего задания
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

// Отправка сообщения консультации
document.getElementById('send-message-btn')?.addEventListener('click', async () => {
    const message = document.getElementById('consultation-message').value;
    const statusElement = document.getElementById('message-status');

    if (!message.trim()) {
        statusElement.textContent = 'Введите сообщение';
        statusElement.style.color = 'red';
        return;
    }

    try {
        const response = await apiRequest('/api/consultation/message', {
            method: 'POST',
            body: JSON.stringify({ text: message })
        });

        if (response.ok) {
            statusElement.textContent = 'Сообщение отправлено!';
            statusElement.style.color = 'green';
            document.getElementById('consultation-message').value = '';
        } else {
            const error = await response.json();
            statusElement.textContent = error.error || 'Ошибка отправки';
            statusElement.style.color = 'red';
        }
    } catch (error) {
        statusElement.textContent = 'Ошибка подключения';
        statusElement.style.color = 'red';
    }
});

// Выход
document.getElementById('logout-btn')?.addEventListener('click', () => {
    authToken = null;
    localStorage.removeItem('authToken');
    updateAuthButtons();
    alert('Вы вышли из системы');
    window.location.href = '/';
});