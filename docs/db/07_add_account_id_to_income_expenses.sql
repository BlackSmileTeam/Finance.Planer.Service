-- Add account_id fields to income_cycles and expenses tables
-- This allows tracking which account receives income or from which account expenses are made

USE financial_planner;

-- Check if account_id column exists in income_cycles, add if not
SET @col_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_cycles' 
    AND COLUMN_NAME = 'account_id'
);

-- Add account_id column to income_cycles (optional, nullable)
SET @sql = IF(@col_exists = 0,
    'ALTER TABLE income_cycles ADD COLUMN account_id CHAR(36) NULL AFTER user_id',
    'SELECT "Column account_id already exists in income_cycles, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add foreign key constraint for income_cycles.account_id
SET @fk_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_cycles' 
    AND CONSTRAINT_NAME = 'fk_income_cycles_account'
);

SET @sql = IF(@fk_exists = 0 AND @col_exists = 0,
    'ALTER TABLE income_cycles ADD CONSTRAINT fk_income_cycles_account FOREIGN KEY (account_id) REFERENCES accounts(id) ON DELETE SET NULL',
    'SELECT "Foreign key fk_income_cycles_account already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add index for account_id in income_cycles
SET @idx_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'income_cycles' 
    AND INDEX_NAME = 'ix_income_cycles_account_id'
);

SET @sql = IF(@idx_exists = 0 AND @col_exists = 0,
    'ALTER TABLE income_cycles ADD INDEX ix_income_cycles_account_id (account_id)',
    'SELECT "Index ix_income_cycles_account_id already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if account_id column exists in expenses, add if not
SET @col_exists_exp = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'expenses' 
    AND COLUMN_NAME = 'account_id'
);

-- Add account_id column to expenses (optional, nullable)
SET @sql = IF(@col_exists_exp = 0,
    'ALTER TABLE expenses ADD COLUMN account_id CHAR(36) NULL AFTER user_id',
    'SELECT "Column account_id already exists in expenses, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add foreign key constraint for expenses.account_id
SET @fk_exists_exp = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'expenses' 
    AND CONSTRAINT_NAME = 'fk_expenses_account'
);

SET @sql = IF(@fk_exists_exp = 0 AND @col_exists_exp = 0,
    'ALTER TABLE expenses ADD CONSTRAINT fk_expenses_account FOREIGN KEY (account_id) REFERENCES accounts(id) ON DELETE SET NULL',
    'SELECT "Foreign key fk_expenses_account already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add index for account_id in expenses
SET @idx_exists_exp = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.STATISTICS 
    WHERE TABLE_SCHEMA = 'financial_planner' 
    AND TABLE_NAME = 'expenses' 
    AND INDEX_NAME = 'ix_expenses_account_id'
);

SET @sql = IF(@idx_exists_exp = 0 AND @col_exists_exp = 0,
    'ALTER TABLE expenses ADD INDEX ix_expenses_account_id (account_id)',
    'SELECT "Index ix_expenses_account_id already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
