-----------------------------------------------------------
-- TICS-4744 Add support for consent ranking and sorting --
-----------------------------------------------------------
ALTER TABLE consent.consent ADD consent_rank INT DEFAULT 0 NOT NULL;