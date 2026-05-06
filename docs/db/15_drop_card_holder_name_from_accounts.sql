-- Remove deprecated card holder name from accounts.
USE financial_planner;

SET @db := DATABASE();
SET @col_exists := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'accounts' AND COLUMN_NAME = 'card_holder_name'
);
SET @sql := IF(@col_exists = 1,
    'ALTER TABLE accounts DROP COLUMN card_holder_name',
    'SELECT ''accounts.card_holder_name does not exist'' AS info');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
