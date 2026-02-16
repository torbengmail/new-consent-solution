ALTER TABLE consent.user_consent RENAME last_asked_date TO last_decision_date;

ALTER TABLE consent.user_consent DROP COLUMN IF EXISTS request_attempts;