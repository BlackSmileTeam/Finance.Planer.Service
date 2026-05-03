-- Add start_date and end_date fields to income_cycles table
-- This allows income cycles to have a start and optional end date, similar to recurring expenses

USE financial_planner;

-- Check if start_date column exists, add if not
SET @col_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_cycles' 
    AND COLUMN_NAME = 'start_date'
);

-- Add start_date column (required, defaults to received_date for existing records)
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE income_cycles ADD COLUMN start_date DATE NOT NULL DEFAULT (CURDATE()) AFTER received_date',
    'SELECT "Column start_date already exists, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Update existing records to use received_date as start_date (only if column was just added)
SET @sql = IF(@col_exists = 0,
    'UPDATE income_cycles SET start_date = received_date WHERE id IN (SELECT id FROM (SELECT id FROM income_cycles WHERE start_date = CURDATE() OR start_date IS NULL) AS temp)',
    'SELECT "Column start_date already exists, skipping update" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Remove default after setting values (only if column was just added)
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE income_cycles MODIFY COLUMN start_date DATE NOT NULL',
    'SELECT "Column start_date already exists, skipping modification" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if end_date column exists
SET @col_exists_end = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_cycles' 
    AND COLUMN_NAME = 'end_date'
);

-- Add end_date column (optional)
SET @sql = IF(@col_exists_end = 0,
    'ALTER TABLE income_cycles ADD COLUMN end_date DATE NULL AFTER start_date',
    'SELECT "Column end_date already exists, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if index for start_date exists
SET @idx_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_cycles' 
    AND INDEX_NAME = 'ix_income_cycles_start_date'
);

-- Add index for start_date for better query performance
SET @sql = IF(@idx_exists = 0,
    'ALTER TABLE income_cycles ADD INDEX ix_income_cycles_start_date (start_date)',
    'SELECT "Index ix_income_cycles_start_date already exists, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if index for end_date exists
SET @idx_exists_end = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_cycles' 
    AND INDEX_NAME = 'ix_income_cycles_end_date'
);

-- Add index for end_date for better query performance
SET @sql = IF(@idx_exists_end = 0,
    'ALTER TABLE income_cycles ADD INDEX ix_income_cycles_end_date (end_date)',
    'SELECT "Index ix_income_cycles_end_date already exists, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
