ALTER TABLE consent.consent ADD COLUMN parent_consent_id INT REFERENCES consent.consent (id);
ALTER TABLE consent.consent ADD COLUMN is_group BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE consent.user_consent ADD COLUMN parent_consent_expression_id INT REFERENCES consent.consent_version (id);

ALTER TABLE consent.consent DROP COLUMN consent_group_id;

