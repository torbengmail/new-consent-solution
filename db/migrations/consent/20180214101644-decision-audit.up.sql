ALTER TABLE consent.user_consent_audit_trail ADD COLUMN parent_consent_expression_id INT REFERENCES consent.consent_version (id);
ALTER TABLE consent.user_consent_audit_trail ADD COLUMN presented_language TEXT NOT NULL REFERENCES consent.language (name) DEFAULT 'en';
ALTER TABLE consent.user_consent_audit_trail RENAME user_consent_id TO decision_id;
ALTER TABLE consent.user_consent_audit_trail RENAME source_channel_id TO user_consent_source_id;
ALTER TABLE consent.user_consent_audit_trail ALTER COLUMN date SET DEFAULT now();