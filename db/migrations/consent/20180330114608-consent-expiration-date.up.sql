---------------------------------------
-- TICS-4507 Consent expiration date --
---------------------------------------
ALTER TABLE consent.consent ADD COLUMN expiration_date TIMESTAMPTZ NOT NULL DEFAULT 'infinity';
