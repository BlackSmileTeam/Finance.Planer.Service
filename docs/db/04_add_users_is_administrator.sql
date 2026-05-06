-- Adds administrator flag for users (category management and future admin-only features).
-- Run after existing schema is applied.

USE financial_planner;

ALTER TABLE users
ADD COLUMN is_administrator TINYINT(1) NOT NULL DEFAULT 0
AFTER is_active;

-- Grant administrator role to the user with username 'sekisov' (case-insensitive match)
UPDATE users
SET is_administrator = 1
WHERE LOWER(username) = 'sekisov';

-- After changing is_administrator, the user must log in again so the JWT includes the Admin role.
