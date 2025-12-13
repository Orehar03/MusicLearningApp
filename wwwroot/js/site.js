// Глобальная переменная для токена
let authToken = localStorage.getItem('authToken') || null;

// Проверка авторизации при загрузке страницы
document.addEventListener('DOMContentLoaded', () => {
    updateAuthButtons();
    loadPageContent();
});

// Обновление состояния кнопок авторизации
function updateAuthButtons() {
    const loginBtn = document.getElementById('login-btn');
    const registerBtn = document.getElementById('register-btn');
    const logoutBtn = document.getElementById('logout-btn');

    if (authToken) {
        loginBtn.style.display = 'none';
        registerBtn.style.display = 'none';
        logoutBtn.style.display = 'inline-block';
    } else {
        loginBtn.style.display = 'inline-block';
        registerBtn.style.display = 'inline-block';
        logoutBtn.style.display = 'none';
    }
}

// Загрузка контента в зависимости от страницы
function loadPageContent() {
    const path = window.location.pathname;

    if (path.includes('materials')) {
        loadLessons();
    } else if (path.includes('homework')) {
        loadHomeworks();
    }
}

// Загрузка уроков
async function loadLessons() {
    try {
        const response = await fetch('/api/lessons', {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });

        if (!response.ok) throw new Error('Ошибка загрузки уроков');

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
        console.error('Ошибка:', error);
        alert('Не удалось загрузить уроки. Пожалуйста, авторизуйтесь.');
    }
}

// Загрузка домашних заданий
async function loadHomeworks() {
    try {
        const response = await fetch('/api/homeworks', {
            headers: { 'Authorization': `Bearer ${authToken}` }
        });

        if (!response.ok) throw new Error('Ошибка загрузки заданий');

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
        console.error('Ошибка:', error);
        alert('Не удалось загрузить задания. Пожалуйста, авторизуйтесь.');
    }
}

// Отправка домашнего задания
async function submitHomework(homeworkId) {
    const textAnswer = document.getElementById(`answer-${homeworkId}`).value;
    const statusElement = document.getElementById(`status-${homeworkId}`);

    try {
        const response = await fetch('/api/submissions', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({ homeworkId, textAnswer })
        });

        const result = await response.json();
        statusElement.textContent = response.ok ? 'Отправлено!' : result;
        statusElement.style.color = response.ok ? 'green' : 'red';
    } catch (error) {
        statusElement.textContent = 'Ошибка отправки';
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
        const response = await fetch('/api/consultation/message', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${authToken}`
            },
            body: JSON.stringify({ text: message })
        });

        statusElement.textContent = response.ok ? 'Сообщение отправлено!' : 'Ошибка отправки';
        statusElement.style.color = response.ok ? 'green' : 'red';

        if (response.ok) {
            document.getElementById('consultation-message').value = '';
        }
    } catch (error) {
        statusElement.textContent = 'Ошибка подключения';
        statusElement.style.color = 'red';
    }
});

// Обработчики кнопок авторизации
document.getElementById('login-btn')?.addEventListener('click', () => {
    const email = prompt('Email:');
    const password = prompt('Пароль:');
    if (email && password) {
        loginUser(email, password);
    }
});

document.getElementById('register-btn')?.addEventListener('click', () => {
    const email = prompt('Email:');
    const password = prompt('Пароль:');
    const name = prompt('Имя:');
    const gender = prompt('Пол (Male/Female/Other):') || 'Other';
    const birthDate = prompt('Дата рождения (ГГГГ-ММ-ДД):');

    if (email && password && name && birthDate) {
        registerUser(email, password, name, gender, birthDate);
    }
});

document.getElementById('logout-btn')?.addEventListener('click', () => {
    authToken = null;
    localStorage.removeItem('authToken');
    updateAuthButtons();
    alert('Вы вышли из системы');
});

// Функция входа
async function loginUser(email, password) {
    try {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        if (!response.ok) throw new Error('Неверные данные');

        const data = await response.json();
        authToken = data.token;
        localStorage.setItem('authToken', authToken);
        updateAuthButtons();
        alert('Вход успешен!');
    } catch (error) {
        alert('Ошибка входа: ' + error.message);
    }
}

// Функция регистрации
async function registerUser(email, password, name, gender, birthDate) {
    try {
        const response = await fetch('/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password, name, gender, birthDate })
        });

        if (!response.ok) throw new Error('Ошибка регистрации');

        alert('Регистрация успешна! Теперь войдите.');
    } catch (error) {
        alert('Ошибка регистрации: ' + error.message);
    }
}