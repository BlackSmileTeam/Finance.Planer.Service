USE financial_planner;

-- Add credit_account_id to expenses (for loan payment confirmations, when no CreditPaymentSchedule exists)
SET @col_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'financial_planner'
      AND TABLE_NAME = 'expenses'
      AND COLUMN_NAME = 'credit_account_id'
);

SET @sql = IF(@col_exists = 0,
    'ALTER TABLE expenses ADD COLUMN credit_account_id CHAR(36) NULL AFTER credit_payment_schedule_id',
    'SELECT "Column credit_account_id already exists in expenses, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add FK to credit_accounts (SET NULL when account is deleted)
SET @fk_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'financial_planner'
      AND TABLE_NAME = 'expenses'
      AND CONSTRAINT_NAME = 'fk_expenses_credit_account'
);

SET @sql = IF(@fk_exists = 0 AND @col_exists = 0,
    'ALTER TABLE expenses ADD CONSTRAINT fk_expenses_credit_account FOREIGN KEY (credit_account_id) REFERENCES credit_accounts(id) ON DELETE SET NULL',
    'SELECT "FK fk_expenses_credit_account already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Index for lookups
SET @idx_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = 'financial_planner'
      AND TABLE_NAME = 'expenses'
      AND INDEX_NAME = 'ix_expenses_credit_account_id'
);

SET @sql = IF(@idx_exists = 0 AND @col_exists = 0,
    'CREATE INDEX ix_expenses_credit_account_id ON expenses (credit_account_id)',
    'SELECT "Index ix_expenses_credit_account_id already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
