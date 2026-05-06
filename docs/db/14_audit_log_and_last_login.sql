-- Audit trail (EF SaveChanges) and last login timestamp on users.
-- Safe to re-run: skips existing column / table.
USE financial_planner;

SET @db := DATABASE();
SET @col_exists := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @db AND TABLE_NAME = 'users' AND COLUMN_NAME = 'last_login_at'
);
SET @sql := IF(@col_exists = 0,
    'ALTER TABLE users ADD COLUMN last_login_at DATETIME(6) NULL AFTER is_administrator',
    'SELECT ''users.last_login_at already exists'' AS info');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

CREATE TABLE IF NOT EXISTS audit_log (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NULL,
    action VARCHAR(20) NOT NULL,
    entity_type VARCHAR(120) NOT NULL,
    entity_id_json LONGTEXT NULL,
    state_before_json LONGTEXT NULL,
    state_after_json LONGTEXT NULL,
    created_at_utc DATETIME(6) NOT NULL,
    PRIMARY KEY (id),
    KEY ix_audit_log_user (user_id),
    KEY ix_audit_log_created (created_at_utc),
    KEY ix_audit_log_entity_created (entity_type, created_at_utc),
    CONSTRAINT fk_audit_log_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
