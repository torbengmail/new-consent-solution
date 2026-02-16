-- do not use sequences for the register tables
ALTER TABLE consent.consent_type ALTER COLUMN id DROP DEFAULT;
ALTER TABLE consent.communication_channel ALTER COLUMN id DROP DEFAULT;
ALTER TABLE consent.consent_status ALTER COLUMN id DROP DEFAULT;
ALTER TABLE consent.user_consent_source_channel_type ALTER COLUMN id DROP DEFAULT;
DROP SEQUENCE consent.consent_type_id_seq;
DROP SEQUENCE consent.communication_channel_id_seq;
DROP SEQUENCE consent.consent_status_id_seq;
DROP SEQUENCE consent.user_consent_source_channel_type_id_seq;

-- fix naming after the versions introduction
ALTER TABLE consent.consent_status RENAME TO consent_version_status;
ALTER TABLE consent.consent_text RENAME TO consent_version_text;
ALTER TABLE consent.user_consent_source_channel_type RENAME TO user_consent_source_type;
ALTER TABLE consent.user_consent_source RENAME source_channel_type_id TO user_consent_source_type_id;
ALTER TABLE consent.user_consent RENAME source_channel_id TO user_consent_source_id;

-- add product_id so that one could request consents related to the specific product only
ALTER TABLE consent.consent ADD COLUMN product_id INTEGER REFERENCES data_inventory.product(id);

-- requiring descriptions is too strict
ALTER TABLE consent.consent ALTER COLUMN description DROP NOT NULL;

-- Additional information related to consent decision change request 
-- (e.g. call center agent name who changed consent on subscriber's behalf)
-- Requirement came from Telenor Denmark initially
ALTER TABLE consent.user_consent_audit_trail ADD COLUMN change_context JSONB;
ALTER TABLE consent.user_consent ADD COLUMN change_context JSONB;

-- Some consents should not be displayed in the list unless there is a user decision for it
-- e.g. consents used for prospects only
ALTER TABLE consent.consent ADD COLUMN hide_by_default BOOLEAN NOT NULL DEFAULT false;
