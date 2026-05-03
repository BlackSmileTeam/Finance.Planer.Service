-- SQL скрипт для предоставления доступа пользователю MySQL с любого IP адреса
-- Выполните этот скрипт от имени пользователя root или администратора MySQL

-- Вариант 1: Если пользователь уже существует, предоставьте ему доступ с любого хоста
-- Сначала удалите существующего пользователя (если он был создан только для localhost)
DROP USER IF EXISTS 'financial_planner_user'@'localhost';
DROP USER IF EXISTS 'financial_planner_user'@'%';

-- Создайте пользователя с доступом с любого IP адреса (% означает любой хост)
CREATE USER IF NOT EXISTS 'financial_planner_user'@'%' IDENTIFIED BY 'your_secure_password_here';

-- Предоставьте все привилегии на базу данных financial_planner
GRANT ALL PRIVILEGES ON financial_planner.* TO 'financial_planner_user'@'%';

-- Или если база данных еще не создана, можно предоставить привилегии на все базы данных
-- (НЕ рекомендуется для продакшена, используйте только для разработки)
-- GRANT ALL PRIVILEGES ON *.* TO 'financial_planner_user'@'%';

-- Примените изменения
FLUSH PRIVILEGES;

-- Проверьте созданного пользователя
SELECT User, Host FROM mysql.user WHERE User = 'financial_planner_user';

-- Проверьте привилегии пользователя
SHOW GRANTS FOR 'financial_planner_user'@'%';

