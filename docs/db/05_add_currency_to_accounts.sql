-- Add currency column to accounts table
-- This script adds the currency field to support multiple currencies per account

USE financial_planner;

ALTER TABLE accounts 
ADD COLUMN currency VARCHAR(10) NULL DEFAULT 'RUB' AFTER color;

