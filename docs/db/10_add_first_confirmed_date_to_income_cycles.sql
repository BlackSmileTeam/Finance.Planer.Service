-- Add first_confirmed_date field to income_cycles table
-- This allows tracking when an income cycle was first confirmed, so it won't appear
-- in pending planned income list even if the actual record is deleted

USE financial_planner;

-- Check if first_confirmed_date column exists in income_cycles, add if not
SET @col_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'financial_planner'
    AND TABLE_NAME = 'income_cycles'
    AND COLUMN_NAME = 'first_confirmed_date'
);

-- Add first_confirmed_date column to income_cycles (nullable date)
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE income_cycles ADD COLUMN first_confirmed_date DATE NULL AFTER end_date',
    'SELECT "Column first_confirmed_date already exists in income_cycles, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
