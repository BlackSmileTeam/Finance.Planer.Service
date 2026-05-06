-- Add accounts table for user accounts (cards, bank accounts, cash, savings)
-- This script creates the accounts table to store user financial accounts

USE financial_planner;

CREATE TABLE IF NOT EXISTS accounts (
    id CHAR(36) NOT NULL PRIMARY KEY,
    user_id CHAR(36) NOT NULL,
    name VARCHAR(150) NOT NULL,
    account_number VARCHAR(50) NULL,
    account_type ENUM('Cash', 'Bank', 'Card', 'Savings') NOT NULL DEFAULT 'Card',
    balance DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    expiry_date VARCHAR(10) NULL,
    color VARCHAR(7) NULL DEFAULT '#3b82f6',
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_accounts_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX ix_accounts_user (user_id),
    INDEX ix_accounts_user_active (user_id, is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

