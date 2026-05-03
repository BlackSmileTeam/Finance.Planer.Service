-- Более безопасный вариант: предоставление доступа только с конкретных IP адресов
-- Используйте этот вариант, если знаете IP адреса ваших серверов

-- Удалите существующих пользователей
DROP USER IF EXISTS 'financial_planner_user'@'localhost';
DROP USER IF EXISTS 'financial_planner_user'@'%';

-- Создайте пользователя для localhost (для локальной разработки)
CREATE USER IF NOT EXISTS 'financial_planner_user'@'localhost' IDENTIFIED BY 'your_secure_password_here';
GRANT ALL PRIVILEGES ON financial_planner.* TO 'financial_planner_user'@'localhost';

-- Создайте пользователя для конкретного IP адреса сервера (178.70.152.47)
CREATE USER IF NOT EXISTS 'financial_planner_user'@'178.70.152.47' IDENTIFIED BY 'your_secure_password_here';
GRANT ALL PRIVILEGES ON financial_planner.* TO 'financial_planner_user'@'178.70.152.47';

-- Если нужно добавить еще IP адреса, используйте:
-- CREATE USER IF NOT EXISTS 'financial_planner_user'@'another.ip.address' IDENTIFIED BY 'your_secure_password_here';
-- GRANT ALL PRIVILEGES ON financial_planner.* TO 'financial_planner_user'@'another.ip.address';

-- Примените изменения
FLUSH PRIVILEGES;

-- Проверьте созданных пользователей
SELECT User, Host FROM mysql.user WHERE User = 'financial_planner_user';

