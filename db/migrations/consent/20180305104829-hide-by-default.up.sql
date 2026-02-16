ALTER TABLE consent.consent_type ADD COLUMN hide_by_default boolean NOT NULL DEFAULT false;

UPDATE consent.consent_type
SET hide_by_default=TRUE
WHERE name='Objection';