-- Financial Planner MySQL schema
CREATE DATABASE IF NOT EXISTS financial_planner CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE financial_planner;

CREATE TABLE IF NOT EXISTS categories (
    id CHAR(36) NOT NULL,
    name VARCHAR(120) NOT NULL,
    hex_color CHAR(7) NOT NULL DEFAULT '#3B82F6',
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY ux_categories_name (name)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS income_cycles (
    id CHAR(36) NOT NULL,
    title VARCHAR(150) NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    received_date DATE NOT NULL,
    frequency ENUM('Weekly','BiWeekly','Monthly','Quarterly','Yearly') NOT NULL DEFAULT 'BiWeekly',
    notes VARCHAR(500) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    KEY ix_income_cycles_received_date (received_date)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS monthly_plans (
    id CHAR(36) NOT NULL,
    plan_year INT NOT NULL,
    plan_month INT NOT NULL,
    planned_income DECIMAL(12,2) NOT NULL DEFAULT 0,
    planned_expense DECIMAL(12,2) NOT NULL DEFAULT 0,
    carry_over DECIMAL(12,2) NOT NULL DEFAULT 0,
    expected_pay_cycles INT NOT NULL DEFAULT 2,
    notes VARCHAR(500) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY ux_monthly_plan_year_month (plan_year, plan_month)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS planned_budgets (
    id CHAR(36) NOT NULL,
    plan_id CHAR(36) NOT NULL,
    category_id CHAR(36) NOT NULL,
    planned_amount DECIMAL(12,2) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    CONSTRAINT fk_planned_budgets_plan FOREIGN KEY (plan_id) REFERENCES monthly_plans(id) ON DELETE CASCADE,
    CONSTRAINT fk_planned_budgets_category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT,
    UNIQUE KEY ux_planned_budget_plan_category (plan_id, category_id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS expenses (
    id CHAR(36) NOT NULL,
    category_id CHAR(36) NOT NULL,
    expense_date DATE NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    description VARCHAR(400) NULL,
    planned_budget_id CHAR(36) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    CONSTRAINT fk_expenses_category FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE RESTRICT,
    CONSTRAINT fk_expenses_planned_budget FOREIGN KEY (planned_budget_id) REFERENCES planned_budgets(id) ON DELETE SET NULL,
    KEY ix_expenses_expense_date (expense_date)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS monthly_snapshots (
    id CHAR(36) NOT NULL,
    plan_id CHAR(36) NOT NULL,
    actual_income DECIMAL(12,2) NOT NULL DEFAULT 0,
    actual_expense DECIMAL(12,2) NOT NULL DEFAULT 0,
    closing_balance DECIMAL(12,2) NOT NULL DEFAULT 0,
    generated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    CONSTRAINT fk_snapshots_plan FOREIGN KEY (plan_id) REFERENCES monthly_plans(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE VIEW vw_monthly_overview AS
SELECT
    mp.plan_year,
    mp.plan_month,
    mp.planned_income,
    mp.planned_expense,
    mp.carry_over,
    mp.expected_pay_cycles,
    IFNULL(ms.actual_income, 0) AS actual_income,
    IFNULL(ms.actual_expense, 0) AS actual_expense,
    IFNULL(ms.closing_balance, mp.carry_over + mp.planned_income - mp.planned_expense) AS closing_balance
FROM monthly_plans mp
LEFT JOIN monthly_snapshots ms ON ms.plan_id = mp.id;
