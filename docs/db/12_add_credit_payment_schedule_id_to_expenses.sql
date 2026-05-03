USE financial_planner;

-- Add credit_payment_schedule_id to expenses (expenses created when confirming credit payments)
SET @col_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'financial_planner'
      AND TABLE_NAME = 'expenses'
      AND COLUMN_NAME = 'credit_payment_schedule_id'
);

SET @sql = IF(@col_exists = 0,
    'ALTER TABLE expenses ADD COLUMN credit_payment_schedule_id CHAR(36) NULL AFTER planned_budget_id',
    'SELECT "Column credit_payment_schedule_id already exists in expenses, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Add FK to credit_payment_schedule (SET NULL when schedule is deleted)
SET @fk_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'financial_planner'
      AND TABLE_NAME = 'expenses'
      AND CONSTRAINT_NAME = 'fk_expenses_credit_payment_schedule'
);

SET @sql = IF(@fk_exists = 0 AND @col_exists = 0,
    'ALTER TABLE expenses ADD CONSTRAINT fk_expenses_credit_payment_schedule FOREIGN KEY (credit_payment_schedule_id) REFERENCES credit_payment_schedule(id) ON DELETE SET NULL',
    'SELECT "FK fk_expenses_credit_payment_schedule already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Index for lookups when deleting credit transaction
SET @idx_exists = (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = 'financial_planner'
      AND TABLE_NAME = 'expenses'
      AND INDEX_NAME = 'ix_expenses_credit_payment_schedule_id'
);

SET @sql = IF(@idx_exists = 0 AND @col_exists = 0,
    'CREATE INDEX ix_expenses_credit_payment_schedule_id ON expenses (credit_payment_schedule_id)',
    'SELECT "Index ix_expenses_credit_payment_schedule_id already exists or column not added, skipping" AS message'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
