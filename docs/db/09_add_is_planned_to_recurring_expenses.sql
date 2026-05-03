-- Add is_planned field to recurring_expenses table
-- This allows recurring expenses to be marked as planned, and all generated expenses will also be planned

USE financial_planner;

-- Check if is_planned column exists in recurring_expenses, add if not
SET @col_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'financial_planner'
    AND TABLE_NAME = 'recurring_expenses'
    AND COLUMN_NAME = 'is_planned'
);

-- Add is_planned column to recurring_expenses (default to false)
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE recurring_expenses ADD COLUMN is_planned TINYINT(1) NOT NULL DEFAULT 0 AFTER is_active',
    'SELECT "Column is_planned already exists in recurring_expenses, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add index for recurring_expenses.is_planned
SET @idx_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = 'financial_planner'
    AND TABLE_NAME = 'recurring_expenses'
    AND INDEX_NAME = 'ix_recurring_expenses_is_planned'
);

SET @sql = IF(@idx_exists = 0,
    'CREATE INDEX ix_recurring_expenses_is_planned ON recurring_expenses (is_planned)',
    'SELECT "Index ix_recurring_expenses_is_planned already exists, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
