------------------------------------------------------------------
-- TICS-4572 Allow defining decision date when saving decisions	--
------------------------------------------------------------------
ALTER TABLE consent.user_consent ADD COLUMN ext_date boolean NOT NULL DEFAULT false;
