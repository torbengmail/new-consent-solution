----------------------------------------------
--TICS-3873 Add consent version tags support--
----------------------------------------------
CREATE TABLE IF NOT EXISTS consent.consent_version_tag (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS consent.consent_version_tag_list (
  consent_version_id INT NOT NULL REFERENCES consent.consent_version (id),
  consent_version_tag_id INT NOT NULL REFERENCES consent.consent_version_tag (id),
  PRIMARY KEY (consent_version_id, consent_version_tag_id)
);
----------------------------------------------------------------------
--TICS-3872 Refine Data Model to reference consent from user_consent--
----------------------------------------------------------------------
ALTER TABLE consent.user_consent ADD COLUMN consent_id INT NOT NULL REFERENCES consent.consent (id);
ALTER TABLE consent.user_consent DROP CONSTRAINT user_consent_consent_version_id_user_id_key;
ALTER TABLE consent.user_consent ADD UNIQUE (user_id, consent_id);

ALTER TABLE consent.user_consent_audit_trail ADD COLUMN consent_version_id INT NOT NULL REFERENCES consent.consent_version (id);

----------------------------------------
--TICS-3869 Add consent groups support--
----------------------------------------
CREATE TABLE IF NOT EXISTS consent.consent_group (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  owner_id INT NOT NULL REFERENCES data_inventory.owner (id)
);

ALTER TABLE consent.consent ADD COLUMN consent_group_id INT REFERENCES consent.consent_group (id);

CREATE TABLE IF NOT EXISTS consent.consent_group_text (
  consent_group_id INT NOT NULL REFERENCES consent.consent_group (id),
  language TEXT NOT NULL REFERENCES consent.language (name),
  title TEXT NOT NULL,
  short_text TEXT NOT NULL,
  long_text TEXT NOT NULL,
  PRIMARY KEY (consent_group_id, language)
);

ALTER TABLE consent.consent_version_text ADD COLUMN title TEXT NOT NULL DEFAULT '';