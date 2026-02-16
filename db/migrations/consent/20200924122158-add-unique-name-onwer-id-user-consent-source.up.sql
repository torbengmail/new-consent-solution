ALTER TABLE consent.user_consent_source DROP CONSTRAINT user_consent_source_name_key;
--;;
ALTER TABLE consent.user_consent_source ADD CONSTRAINT user_consent_source_unique UNIQUE (owner_id, name);
