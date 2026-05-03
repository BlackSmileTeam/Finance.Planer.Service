    -- Add first_confirmed_date field to income_records table
    -- This allows tracking when a planned income record was first confirmed, so it won't appear
    -- in pending planned income list even if the actual record is deleted
    --
    -- Вариант 1: выполните одну команду (если колонки ещё нет):
    --   ALTER TABLE financial_planner.income_records ADD COLUMN first_confirmed_date DATE NULL AFTER is_planned;
    --
    -- Вариант 2: скрипт с проверкой (выполнять целиком в MySQL):

    USE financial_planner;

    -- Check if first_confirmed_date column exists in income_records, add if not
    SET @col_exists = (
        SELECT COUNT(*)
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = 'financial_planner'
        AND TABLE_NAME = 'income_records'
        AND COLUMN_NAME = 'first_confirmed_date'
    );

    -- Add first_confirmed_date column to income_records (nullable date)
    SET @sql = IF(@col_exists = 0,
        'ALTER TABLE income_records ADD COLUMN first_confirmed_date DATE NULL AFTER is_planned',
        'SELECT "Column first_confirmed_date already exists in income_records, skipping" AS message'
    );
    PREPARE stmt FROM @sql;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
