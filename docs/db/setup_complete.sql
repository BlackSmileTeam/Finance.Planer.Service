-- Полная первичная установка БД (новый сервер):
-- 1) sudo mysql < mysql_root_setup.sql
-- 2) В MySQL Workbench: fp_workbench / FpWb_Schema_8nL3pQ5rT9vX2026! — права на все БД (*.*); выполнить workbench_full_schema_and_seed_categories.sql в financial_planner
--    (fp_app — только для API)

SELECT 'Шаг 1: sudo mysql < mysql_root_setup.sql' AS step_1;
SELECT 'Шаг 2: Workbench как fp_workbench → workbench_full_schema_and_seed_categories.sql' AS step_2;
