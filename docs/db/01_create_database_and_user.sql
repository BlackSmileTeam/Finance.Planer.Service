-- Совпадает с mysql_root_setup.sql (удобно для старых инструкций).
-- Выполнение: sudo mysql < 01_create_database_and_user.sql

CREATE DATABASE IF NOT EXISTS financial_planner
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

CREATE USER IF NOT EXISTS 'fp_app'@'%' IDENTIFIED BY 'FpDb_User_9xK4mQ2vR7nW2026!';
GRANT ALL PRIVILEGES ON financial_planner.* TO 'fp_app'@'%';

CREATE USER IF NOT EXISTS 'fp_workbench'@'%' IDENTIFIED BY 'FpWb_Schema_8nL3pQ5rT9vX2026!';
GRANT ALL PRIVILEGES ON *.* TO 'fp_workbench'@'%' WITH GRANT OPTION;

CREATE USER IF NOT EXISTS 'fp_workbench'@'localhost' IDENTIFIED BY 'FpWb_Schema_8nL3pQ5rT9vX2026!';
GRANT ALL PRIVILEGES ON *.* TO 'fp_workbench'@'localhost' WITH GRANT OPTION;

FLUSH PRIVILEGES;
