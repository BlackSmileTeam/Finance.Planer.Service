-- Financial Planner MySQL schema - New features
-- This script adds support for:
-- 1. Subcategories and icons for categories
-- 2. Recurring expenses
-- 3. Credit cards and loans
-- 4. Investments
-- 5. Income entries (actual income records)

USE financial_planner;

-- Add parent_id and icon to categories table
ALTER TABLE categories 
ADD COLUMN parent_id CHAR(36) NULL AFTER user_id,
ADD COLUMN icon VARCHAR(100) NULL AFTER hex_color,
ADD CONSTRAINT fk_categories_parent FOREIGN KEY (parent_id) REFERENCES categories(id) ON DELETE CASCADE,
ADD INDEX ix_categories_parent (parent_id);

-- Create recurring expenses table
CREATE TABLE IF NOT EXISTS recurring_expenses (
    id CHAR(36) NOT NULL PRIMARY KEY,
    user_id CHAR(36) NOT NULL,
    category_id CHAR(36) NOT NULL,
    subcategory_id CHAR(36) NULL,
    title VARCHAR(150) NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NULL,
    frequency ENUM('Weekly','BiWeekly','Monthly','Quarterly','Yearly') NOT NULL DEFAULT 'Monthly',
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    notes VARCHAR(500) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_recurring_expenses_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_recurring_expenses_category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT,
    CONSTRAINT fk_recurring_expenses_subcategory FOREIGN KEY (subcategory_id) REFERENCES categories(id) ON DELETE SET NULL,
    INDEX ix_recurring_expenses_user (user_id),
    INDEX ix_recurring_expenses_category (category_id),
    INDEX ix_recurring_expenses_start_date (start_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create credit cards and loans table
CREATE TABLE IF NOT EXISTS credit_accounts (
    id CHAR(36) NOT NULL PRIMARY KEY,
    user_id CHAR(36) NOT NULL,
    name VARCHAR(150) NOT NULL,
    account_type ENUM('CreditCard','Loan') NOT NULL DEFAULT 'CreditCard',
    credit_limit DECIMAL(12,2) NULL,
    current_balance DECIMAL(12,2) NOT NULL DEFAULT 0,
    interest_rate DECIMAL(5,2) NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    notes VARCHAR(500) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_credit_accounts_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX ix_credit_accounts_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create credit transactions table (withdrawals from credit cards)
CREATE TABLE IF NOT EXISTS credit_transactions (
    id CHAR(36) NOT NULL PRIMARY KEY,
    user_id CHAR(36) NOT NULL,
    credit_account_id CHAR(36) NOT NULL,
    category_id CHAR(36) NOT NULL,
    subcategory_id CHAR(36) NULL,
    transaction_date DATE NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    description VARCHAR(400) NULL,
    is_income_recorded TINYINT(1) NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_credit_transactions_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_credit_transactions_account FOREIGN KEY (credit_account_id) REFERENCES credit_accounts(id) ON DELETE CASCADE,
    CONSTRAINT fk_credit_transactions_category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT,
    CONSTRAINT fk_credit_transactions_subcategory FOREIGN KEY (subcategory_id) REFERENCES categories(id) ON DELETE SET NULL,
    INDEX ix_credit_transactions_user (user_id),
    INDEX ix_credit_transactions_account (credit_account_id),
    INDEX ix_credit_transactions_date (transaction_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create credit payment schedule table (split payments over months)
CREATE TABLE IF NOT EXISTS credit_payment_schedule (
    id CHAR(36) NOT NULL PRIMARY KEY,
    credit_transaction_id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    scheduled_year INT NOT NULL,
    scheduled_month INT NOT NULL,
    payment_amount DECIMAL(12,2) NOT NULL,
    is_paid TINYINT(1) NOT NULL DEFAULT 0,
    paid_date DATE NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_credit_payment_schedule_transaction FOREIGN KEY (credit_transaction_id) REFERENCES credit_transactions(id) ON DELETE CASCADE,
    CONSTRAINT fk_credit_payment_schedule_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX ix_credit_payment_schedule_transaction (credit_transaction_id),
    INDEX ix_credit_payment_schedule_user (user_id),
    INDEX ix_credit_payment_schedule_date (scheduled_year, scheduled_month)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create investments table
CREATE TABLE IF NOT EXISTS investments (
    id CHAR(36) NOT NULL PRIMARY KEY,
    user_id CHAR(36) NOT NULL,
    title VARCHAR(150) NOT NULL,
    investment_type ENUM('Stock','Bond','ETF','Crypto','RealEstate','Other') NOT NULL DEFAULT 'Stock',
    amount DECIMAL(12,2) NOT NULL,
    purchase_date DATE NOT NULL,
    current_value DECIMAL(12,2) NULL,
    notes VARCHAR(500) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_investments_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX ix_investments_user (user_id),
    INDEX ix_investments_purchase_date (purchase_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Create actual income records table (separate from income cycles)
CREATE TABLE IF NOT EXISTS income_records (
    id CHAR(36) NOT NULL PRIMARY KEY,
    user_id CHAR(36) NOT NULL,
    income_cycle_id CHAR(36) NULL,
    title VARCHAR(150) NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    received_date DATE NOT NULL,
    is_from_credit TINYINT(1) NOT NULL DEFAULT 0,
    credit_transaction_id CHAR(36) NULL,
    notes VARCHAR(500) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_income_records_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_income_records_cycle FOREIGN KEY (income_cycle_id) REFERENCES income_cycles(id) ON DELETE SET NULL,
    CONSTRAINT fk_income_records_credit FOREIGN KEY (credit_transaction_id) REFERENCES credit_transactions(id) ON DELETE SET NULL,
    INDEX ix_income_records_user (user_id),
    INDEX ix_income_records_date (received_date),
    INDEX ix_income_records_cycle (income_cycle_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Add subcategory_id to expenses table
ALTER TABLE expenses 
ADD COLUMN subcategory_id CHAR(36) NULL AFTER category_id,
ADD CONSTRAINT fk_expenses_subcategory FOREIGN KEY (subcategory_id) REFERENCES categories(id) ON DELETE SET NULL,
ADD INDEX ix_expenses_subcategory (subcategory_id);

-- Add subcategory_id to planned_budgets table
ALTER TABLE planned_budgets 
ADD COLUMN subcategory_id CHAR(36) NULL AFTER category_id,
ADD CONSTRAINT fk_planned_budgets_subcategory FOREIGN KEY (subcategory_id) REFERENCES categories(id) ON DELETE SET NULL,
ADD INDEX ix_planned_budgets_subcategory (subcategory_id);

