-- SQL команда для добавления пользователя sekisov в базу данных
-- Пароль: Sekisov2024!Secure
-- Хеш пароля (SHA256 + Base64): m+dLfz9Dr0f6uM2nSAr4HkzQedJHxbmqQZtX9ZO8/ME=

USE financial_planner;

-- Удалите пользователя, если он уже существует (опционально)
DELETE FROM users WHERE username = 'sekisov';

-- Добавление пользователя sekisov
INSERT INTO users (
    id,
    username,
    email,
    password_hash,
    full_name,
    is_active,
    created_at,
    updated_at
) VALUES (
    UUID(),
    'sekisov',
    'sekisov@example.com',  -- Замените на реальный email при необходимости
    'm+dLfz9Dr0f6uM2nSAr4HkzQedJHxbmqQZtX9ZO8/ME=',  -- Хеш пароля: Sekisov2024!Secure
    'Sekisov User',  -- Полное имя (опционально, можно оставить NULL)
    1,
    NOW(),
    NOW()
);

-- Проверка добавленного пользователя
SELECT id, username, email, full_name, is_active, created_at 
FROM users 
WHERE username = 'sekisov';

