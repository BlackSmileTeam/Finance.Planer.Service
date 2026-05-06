-- MySQL Workbench: пользователь fp_workbench (глобальные права *.*, см. mysql_root_setup.sql).
-- Для API используйте fp_app.

USE financial_planner;

SET NAMES utf8mb4;

-- ---------------------------------------------------------------------------
-- users
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS users (
    id CHAR(36) NOT NULL,
    username VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(150) NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    is_administrator TINYINT(1) NOT NULL DEFAULT 0,
    last_login_at DATETIME(6) NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY ix_users_username (username),
    UNIQUE KEY ix_users_email (email),
    KEY ix_users_email_lookup (email),
    KEY ix_users_username_lookup (username)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- audit_log (change history for future rollback)
-- ---------------------------------------------------------------------------
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

-- ---------------------------------------------------------------------------
-- accounts (до income_cycles из‑за FK account_id)
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS accounts (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    name VARCHAR(150) NOT NULL,
    account_number VARCHAR(50) NULL,
    account_type VARCHAR(20) NOT NULL,
    balance DECIMAL(12,2) NOT NULL DEFAULT 0.00,
    expiry_date VARCHAR(10) NULL,
    color VARCHAR(7) NULL,
    currency VARCHAR(10) NULL DEFAULT 'RUB',
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    CONSTRAINT fk_accounts_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    KEY ix_accounts_user (user_id),
    KEY ix_accounts_user_active (user_id, is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- categories
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS categories (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    parent_id CHAR(36) NULL,
    name VARCHAR(120) NOT NULL,
    hex_color VARCHAR(7) NOT NULL DEFAULT '#3B82F6',
    icon VARCHAR(100) NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY ux_categories_user_name (user_id, name),
    KEY ix_categories_user (user_id),
    KEY ix_categories_parent (parent_id),
    CONSTRAINT fk_categories_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_categories_parent FOREIGN KEY (parent_id) REFERENCES categories(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- monthly_plans
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS monthly_plans (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    plan_year INT NOT NULL,
    plan_month INT NOT NULL,
    planned_income DECIMAL(12,2) NOT NULL DEFAULT 0,
    planned_expense DECIMAL(12,2) NOT NULL DEFAULT 0,
    carry_over DECIMAL(12,2) NOT NULL DEFAULT 0,
    expected_pay_cycles INT NOT NULL DEFAULT 2,
    notes VARCHAR(500) NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY ux_monthly_plan_user_year_month (user_id, plan_year, plan_month),
    KEY ix_monthly_plans_user (user_id),
    KEY ix_monthly_plans_year_month (plan_year, plan_month),
    CONSTRAINT fk_monthly_plans_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- monthly_snapshots
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS monthly_snapshots (
    id CHAR(36) NOT NULL,
    plan_id CHAR(36) NOT NULL,
    actual_income DECIMAL(12,2) NOT NULL DEFAULT 0,
    actual_expense DECIMAL(12,2) NOT NULL DEFAULT 0,
    closing_balance DECIMAL(12,2) NOT NULL DEFAULT 0,
    generated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY ux_monthly_snapshots_plan (plan_id),
    CONSTRAINT fk_monthly_snapshots_plan FOREIGN KEY (plan_id) REFERENCES monthly_plans(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- planned_budgets
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS planned_budgets (
    id CHAR(36) NOT NULL,
    plan_id CHAR(36) NOT NULL,
    category_id CHAR(36) NOT NULL,
    subcategory_id CHAR(36) NULL,
    planned_amount DECIMAL(12,2) NOT NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY ux_planned_budget_plan_category (plan_id, category_id),
    KEY ix_planned_budgets_plan (plan_id),
    KEY ix_planned_budgets_category (category_id),
    KEY ix_planned_budgets_subcategory (subcategory_id),
    CONSTRAINT fk_planned_budgets_plan FOREIGN KEY (plan_id) REFERENCES monthly_plans(id) ON DELETE CASCADE,
    CONSTRAINT fk_planned_budgets_category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT,
    CONSTRAINT fk_planned_budgets_subcategory FOREIGN KEY (subcategory_id) REFERENCES categories(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- income_cycles
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS income_cycles (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    account_id CHAR(36) NULL,
    title VARCHAR(150) NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    received_date DATE NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NULL,
    first_confirmed_date DATE NULL,
    frequency VARCHAR(20) NOT NULL DEFAULT 'BiWeekly',
    notes VARCHAR(500) NULL,
    is_planned TINYINT(1) NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    KEY ix_income_cycles_user (user_id),
    KEY ix_income_cycles_received_date (received_date),
    KEY ix_income_cycles_account_id (account_id),
    KEY ix_income_cycles_is_planned_start_date (is_planned, start_date),
    CONSTRAINT fk_income_cycles_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_income_cycles_account FOREIGN KEY (account_id) REFERENCES accounts(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- credit_accounts
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS credit_accounts (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    name VARCHAR(150) NOT NULL,
    account_type VARCHAR(20) NOT NULL DEFAULT 'CreditCard',
    credit_limit DECIMAL(12,2) NULL,
    monthly_payment DECIMAL(12,2) NULL,
    total_amount DECIMAL(12,2) NULL,
    term_months INT NULL,
    payment_start_date DATE NULL,
    current_balance DECIMAL(12,2) NOT NULL DEFAULT 0,
    interest_rate DECIMAL(5,2) NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    notes VARCHAR(500) NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    KEY ix_credit_accounts_user (user_id),
    CONSTRAINT fk_credit_accounts_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- credit_transactions
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS credit_transactions (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    credit_account_id CHAR(36) NOT NULL,
    category_id CHAR(36) NOT NULL,
    subcategory_id CHAR(36) NULL,
    transaction_date DATE NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    description VARCHAR(400) NULL,
    is_income_recorded TINYINT(1) NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    KEY ix_credit_transactions_user (user_id),
    KEY ix_credit_transactions_credit_account_id (credit_account_id),
    KEY ix_credit_transactions_transaction_date (transaction_date),
    CONSTRAINT fk_credit_transactions_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_credit_transactions_account FOREIGN KEY (credit_account_id) REFERENCES credit_accounts(id) ON DELETE CASCADE,
    CONSTRAINT fk_credit_transactions_category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT,
    CONSTRAINT fk_credit_transactions_subcategory FOREIGN KEY (subcategory_id) REFERENCES categories(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- credit_payment_schedule
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS credit_payment_schedule (
    id CHAR(36) NOT NULL,
    credit_transaction_id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    scheduled_year INT NOT NULL,
    scheduled_month INT NOT NULL,
    payment_amount DECIMAL(12,2) NOT NULL,
    is_paid TINYINT(1) NOT NULL DEFAULT 0,
    paid_date DATE NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    KEY ix_credit_payment_schedule_transaction (credit_transaction_id),
    KEY ix_credit_payment_schedule_user (user_id),
    KEY ix_credit_payment_schedule_year_month (scheduled_year, scheduled_month),
    CONSTRAINT fk_credit_payment_schedule_transaction FOREIGN KEY (credit_transaction_id) REFERENCES credit_transactions(id) ON DELETE CASCADE,
    CONSTRAINT fk_credit_payment_schedule_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- expenses
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS expenses (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    account_id CHAR(36) NULL,
    category_id CHAR(36) NOT NULL,
    subcategory_id CHAR(36) NULL,
    expense_date DATE NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    description VARCHAR(400) NULL,
    planned_budget_id CHAR(36) NULL,
    credit_payment_schedule_id CHAR(36) NULL,
    credit_account_id CHAR(36) NULL,
    is_planned TINYINT(1) NOT NULL DEFAULT 0,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    KEY ix_expenses_expense_date (expense_date),
    KEY ix_expenses_user (user_id),
    KEY ix_expenses_user_date (user_id, expense_date),
    KEY ix_expenses_subcategory (subcategory_id),
    KEY ix_expenses_account_id (account_id),
    KEY ix_expenses_is_planned_expense_date (is_planned, expense_date),
    KEY ix_expenses_credit_account_id (credit_account_id),
    KEY ix_expenses_credit_payment_schedule_id (credit_payment_schedule_id),
    CONSTRAINT fk_expenses_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_expenses_account FOREIGN KEY (account_id) REFERENCES accounts(id) ON DELETE SET NULL,
    CONSTRAINT fk_expenses_category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT,
    CONSTRAINT fk_expenses_subcategory FOREIGN KEY (subcategory_id) REFERENCES categories(id) ON DELETE SET NULL,
    CONSTRAINT fk_expenses_planned_budget FOREIGN KEY (planned_budget_id) REFERENCES planned_budgets(id) ON DELETE SET NULL,
    CONSTRAINT fk_expenses_credit_payment_schedule FOREIGN KEY (credit_payment_schedule_id) REFERENCES credit_payment_schedule(id) ON DELETE SET NULL,
    CONSTRAINT fk_expenses_credit_account FOREIGN KEY (credit_account_id) REFERENCES credit_accounts(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- recurring_expenses
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS recurring_expenses (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    category_id CHAR(36) NOT NULL,
    subcategory_id CHAR(36) NULL,
    title VARCHAR(150) NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NULL,
    frequency VARCHAR(20) NOT NULL DEFAULT 'Monthly',
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    is_planned TINYINT(1) NOT NULL DEFAULT 0,
    notes VARCHAR(500) NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    KEY ix_recurring_expenses_user (user_id),
    KEY ix_recurring_expenses_category (category_id),
    KEY ix_recurring_expenses_start_date (start_date),
    KEY ix_recurring_expenses_is_planned (is_planned),
    CONSTRAINT fk_recurring_expenses_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_recurring_expenses_category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT,
    CONSTRAINT fk_recurring_expenses_subcategory FOREIGN KEY (subcategory_id) REFERENCES categories(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- investments
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS investments (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    title VARCHAR(150) NOT NULL,
    investment_type VARCHAR(20) NOT NULL DEFAULT 'Stock',
    amount DECIMAL(12,2) NOT NULL,
    purchase_date DATE NOT NULL,
    current_value DECIMAL(12,2) NULL,
    notes VARCHAR(500) NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    KEY ix_investments_user (user_id),
    KEY ix_investments_purchase_date (purchase_date),
    CONSTRAINT fk_investments_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- income_records
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS income_records (
    id CHAR(36) NOT NULL,
    user_id CHAR(36) NOT NULL,
    income_cycle_id CHAR(36) NULL,
    title VARCHAR(150) NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    received_date DATE NOT NULL,
    is_from_credit TINYINT(1) NOT NULL DEFAULT 0,
    credit_transaction_id CHAR(36) NULL,
    is_planned TINYINT(1) NOT NULL DEFAULT 0,
    first_confirmed_date DATE NULL,
    notes VARCHAR(500) NULL,
    created_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    KEY ix_income_records_user (user_id),
    KEY ix_income_records_received_date (received_date),
    KEY ix_income_records_income_cycle_id (income_cycle_id),
    KEY ix_income_records_is_planned_received_date (is_planned, received_date),
    CONSTRAINT fk_income_records_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_income_records_cycle FOREIGN KEY (income_cycle_id) REFERENCES income_cycles(id) ON DELETE SET NULL,
    CONSTRAINT fk_income_records_credit FOREIGN KEY (credit_transaction_id) REFERENCES credit_transactions(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- Связь monthly_plans <-> monthly_snapshots (один к одному): plan_id на snapshot
-- EF: MonthlyPlan.Snapshot, FK на MonthlySnapshot стороне — уже есть plan_id.
-- Доп. FK с plan.snapshot_id в EF? Проверка: ConfigureMonthlyPlans HasOne Snapshot WithForeignKey MonthlySnapshot PlanId
-- Значит только plan_id в snapshot, без обратной колонки. Ок.
-- ---------------------------------------------------------------------------

-- ---------------------------------------------------------------------------
-- Сид: пользователь для стартовых категорий
-- Пароль входа в API: SeedUser_1 (SHA256+Base64 как в AuthService)
-- ---------------------------------------------------------------------------
INSERT INTO users (
    id, username, email, password_hash, full_name, is_active, created_at, updated_at
) VALUES (
    'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11',
    'seed_owner',
    'seed_owner@financialplanner.local',
    '/fBqYBClLuvGpTFGXQy+n6T2bobrjSn4tKZ11vEfTYE=',
    'Seed owner',
    1,
    CURRENT_TIMESTAMP(6),
    CURRENT_TIMESTAMP(6)
) ON DUPLICATE KEY UPDATE updated_at = CURRENT_TIMESTAMP(6);

-- ---------------------------------------------------------------------------
-- Сид: категории (те же имена часто используются в коде сервисов)
-- ---------------------------------------------------------------------------
INSERT INTO categories (id, user_id, parent_id, name, hex_color, icon, is_active, created_at, updated_at) VALUES
('b1eebc99-9c0b-4ef8-bb6d-6bb9bd380a01', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', NULL, 'Продукты', '#22C55E', '🛒', 1, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)),
('b1eebc99-9c0b-4ef8-bb6d-6bb9bd380a02', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', NULL, 'Транспорт', '#3B82F6', '🚗', 1, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)),
('b1eebc99-9c0b-4ef8-bb6d-6bb9bd380a03', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', NULL, 'Жильё', '#F59E0B', '🏠', 1, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)),
('b1eebc99-9c0b-4ef8-bb6d-6bb9bd380a04', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', NULL, 'Здоровье', '#EF4444', '💊', 1, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)),
('b1eebc99-9c0b-4ef8-bb6d-6bb9bd380a05', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', NULL, 'Развлечения', '#8B5CF6', '🎬', 1, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)),
('b1eebc99-9c0b-4ef8-bb6d-6bb9bd380a06', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', NULL, 'Кредиты', '#FF6B6B', '💳', 1, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)),
('b1eebc99-9c0b-4ef8-bb6d-6bb9bd380a07', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', NULL, 'Кредитная карта', '#6366F1', '💳', 1, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)),
('b1eebc99-9c0b-4ef8-bb6d-6bb9bd380a08', 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', NULL, 'Прочее', '#64748B', '📌', 1, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE
    hex_color = VALUES(hex_color),
    icon = VALUES(icon),
    is_active = VALUES(is_active),
    updated_at = CURRENT_TIMESTAMP(6);
