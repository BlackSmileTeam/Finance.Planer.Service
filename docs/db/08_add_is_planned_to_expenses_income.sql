USE financial_planner;

-- Check if is_planned column exists in expenses, add if not
SET @col_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'expenses' 
    AND COLUMN_NAME = 'is_planned'
);

-- Add is_planned column to expenses (default false for existing records)
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE expenses ADD COLUMN is_planned TINYINT(1) NOT NULL DEFAULT 0 AFTER account_id',
    'SELECT "Column is_planned already exists in expenses, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add index for expenses.is_planned
SET @idx_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'expenses' 
    AND INDEX_NAME = 'ix_expenses_is_planned'
);

SET @sql = IF(@idx_exists = 0 AND @col_exists = 0,
    'CREATE INDEX ix_expenses_is_planned ON expenses (is_planned, expense_date)',
    'SELECT "Index ix_expenses_is_planned already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if is_planned column exists in income_records, add if not
SET @col_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_records' 
    AND COLUMN_NAME = 'is_planned'
);

-- Add is_planned column to income_records (default false for existing records)
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE income_records ADD COLUMN is_planned TINYINT(1) NOT NULL DEFAULT 0 AFTER credit_transaction_id',
    'SELECT "Column is_planned already exists in income_records, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add index for income_records.is_planned
SET @idx_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_records' 
    AND INDEX_NAME = 'ix_income_records_is_planned'
);

SET @sql = IF(@idx_exists = 0 AND @col_exists = 0,
    'CREATE INDEX ix_income_records_is_planned ON income_records (is_planned, received_date)',
    'SELECT "Index ix_income_records_is_planned already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if is_planned column exists in income_cycles, add if not
SET @col_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_cycles' 
    AND COLUMN_NAME = 'is_planned'
);

-- Add is_planned column to income_cycles (default false for existing records)
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE income_cycles ADD COLUMN is_planned TINYINT(1) NOT NULL DEFAULT 0 AFTER account_id',
    'SELECT "Column is_planned already exists in income_cycles, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add index for income_cycles.is_planned
SET @idx_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_cycles' 
    AND INDEX_NAME = 'ix_income_cycles_is_planned'
);

SET @sql = IF(@idx_exists = 0 AND @col_exists = 0,
    'CREATE INDEX ix_income_cycles_is_planned ON income_cycles (is_planned, start_date)',
    'SELECT "Index ix_income_cycles_is_planned already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
