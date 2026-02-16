-- -----------------------------------------------------
-- Table consent.language
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.language (
  name TEXT PRIMARY KEY
);

-- -----------------------------------------------------
-- Table consent.communication_channel
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.communication_channel (
  id SERIAL PRIMARY KEY,
  name TEXT UNIQUE NOT NULL
);

-- -----------------------------------------------------
-- Table consent.user_consent_source_channel_type
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.user_consent_source_channel_type (
  id SERIAL PRIMARY KEY,
  name TEXT UNIQUE NOT NULL
);

-- -----------------------------------------------------
-- Table consent.user_consent_source
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.user_consent_source (
  id SERIAL PRIMARY KEY,
  name TEXT UNIQUE NOT NULL,
  description TEXT NOT NULL,
  source_channel_type_id INT NOT NULL REFERENCES consent.user_consent_source_channel_type (id)
);

-- -----------------------------------------------------
-- Table consent.consent_status
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.consent_status (
  id SERIAL PRIMARY KEY,
  name TEXT UNIQUE NOT NULL
);

-- -----------------------------------------------------
-- Table consent.consent_type
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.consent_type (
  id SERIAL PRIMARY KEY,
  name TEXT UNIQUE NOT NULL,
  default_opt_in BOOLEAN NOT NULL DEFAULT FALSE
);

-- -----------------------------------------------------
-- Table consent.consent
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.consent (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  description TEXT NOT NULL,
  owner_id INT NOT NULL REFERENCES data_inventory.owner (id),
  purpose_id INT NOT NULL REFERENCES data_inventory.purpose_category (id),
  consent_type_id INT NOT NULL REFERENCES consent.consent_type (id),
  created_date TIMESTAMPTZ NOT NULL DEFAULT now(),
  modified_date TIMESTAMPTZ NOT NULL DEFAULT now(),
  special_data_category_id INT REFERENCES data_inventory.data_category (id),
  data_source_id INT REFERENCES data_inventory.data_source (id),
  processing_type_id INT REFERENCES data_inventory.processing_type (id),
  UNIQUE (owner_id, name),
  -- Allow only one of the following fields to be non null
  CONSTRAINT consent_extension_only_one CHECK (
    (CASE WHEN special_data_category_id IS NULL THEN 0 ELSE 1 END) +
    (CASE WHEN data_source_id IS NULL THEN 0 ELSE 1 END) +
    (CASE WHEN processing_type_id IS NULL THEN 0 ELSE 1 END) <= 1)
);

-- -----------------------------------------------------
-- Table consent.consent_version
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.consent_version (
  id SERIAL PRIMARY KEY,
  name TEXT NOT NULL,
  description TEXT,
  consent_id INT NOT NULL REFERENCES consent.consent (id),
  created_date TIMESTAMPTZ NOT NULL DEFAULT now(),
  modified_date TIMESTAMPTZ NOT NULL DEFAULT now(),
  status_id INT NOT NULL REFERENCES consent.consent_status (id),
  is_default BOOLEAN NOT NULL DEFAULT FALSE,
  UNIQUE (name, consent_id)
);

-- -----------------------------------------------------
-- Table consent.consent_text
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.consent_text (
  consent_version_id INT NOT NULL REFERENCES consent.consent_version (id),
  language TEXT NOT NULL REFERENCES consent.language (name),
  short_text TEXT NOT NULL,
  long_text TEXT NOT NULL,
  PRIMARY KEY (consent_version_id, language)
);

-- -----------------------------------------------------
-- Table consent.consent_channel
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.consent_channel (
  consent_version_id INT NOT NULL REFERENCES consent.consent_version (id),
  communication_channel_id INT NOT NULL REFERENCES consent.communication_channel (id),
  PRIMARY KEY (consent_version_id, communication_channel_id)
);

-- -----------------------------------------------------
-- Table consent.consent_data_category
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.consent_data_category (
  consent_version_id INT NOT NULL REFERENCES consent.consent_version (id),
  data_category_id INT NOT NULL REFERENCES data_inventory.data_category (id)
);

-- -----------------------------------------------------
-- Table consent.use_case_consent
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.use_case_consent (
  use_case_id INT NOT NULL REFERENCES data_inventory.use_case (id),
  consent_id INT NOT NULL REFERENCES consent.consent (id),
  PRIMARY KEY (use_case_id, consent_id)
);

-- -----------------------------------------------------
-- Table consent.user_consent
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.user_consent (
  id SERIAL PRIMARY KEY,
  consent_version_id INT NOT NULL REFERENCES consent.consent_version (id),
  user_id TEXT NOT NULL,
  last_asked_date TIMESTAMPTZ DEFAULT now(),
  request_attempts INT NOT NULL DEFAULT 1,
  is_agreed BOOLEAN NOT NULL,
  source_channel_id INT NOT NULL REFERENCES consent.user_consent_source (id),
  presented_language TEXT NOT NULL REFERENCES consent.language (name) DEFAULT 'en',
  UNIQUE (consent_version_id, user_id)
);

CREATE INDEX user_consent_consent_version_id ON consent.user_consent USING BTREE (consent_version_id);
CREATE INDEX user_consent_user_id ON consent.user_consent USING BTREE (user_id);

-- Create sequence for prospect ID generation
CREATE SEQUENCE consent.prospect_id_seq;

-- -----------------------------------------------------
-- Table consent.prospect
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.prospect (
  -- Prospect IDs should be autogenerated, never try to insert it manually
  id TEXT PRIMARY KEY DEFAULT 'P-'||nextval('consent.prospect_id_seq')::TEXT CHECK(id ILIKE 'P-%'),
  owner_id INT NOT NULL REFERENCES data_inventory.owner (id),
  email TEXT,
  msisdn INT,
  internal_system_reference TEXT,
  UNIQUE (owner_id, email),
  UNIQUE (owner_id, msisdn),
  CONSTRAINT prospect_at_least_email_or_msisdn CHECK (
    (CASE WHEN email IS NULL THEN 0 ELSE 1 END) +
    (CASE WHEN msisdn IS NULL THEN 0 ELSE 1 END) >= 1)
);

CREATE INDEX prospect_owner_id ON consent.prospect USING BTREE (owner_id);

-- -----------------------------------------------------
-- Table consent.user_consent_audit_trail
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.user_consent_audit_trail (
  user_consent_id INT NOT NULL REFERENCES consent.user_consent (id),
  is_agreed BOOLEAN NOT NULL,
  date TIMESTAMPTZ NOT NULL,
  source_channel_id INT NOT NULL REFERENCES consent.user_consent_source (id)
);

CREATE INDEX user_consent_audit_trail_user_consent_id ON consent.user_consent_audit_trail USING BTREE (user_consent_id);

-- -----------------------------------------------------
-- Table consent.user_consent_last_usage
-- -----------------------------------------------------

CREATE TABLE IF NOT EXISTS consent.user_consent_last_usage (
  user_consent_id INT NOT NULL REFERENCES consent.user_consent (id),
  system_name TEXT NOT NULL,
  last_used_date TIMESTAMPTZ NOT NULL,
  PRIMARY KEY (user_consent_id, system_name)
);

CREATE INDEX user_consent_last_usage_user_consent_id ON consent.user_consent_last_usage USING BTREE (user_consent_id);

-- Create function and trigger for only allowing newer timestamps
CREATE OR REPLACE FUNCTION consent.last_used_date_newer()
  RETURNS TRIGGER AS
$BODY$
BEGIN
  IF NEW.last_used_date >= OLD.last_used_date
  THEN
    RETURN NEW;
  ELSE RETURN NULL;
  END IF;
END;
$BODY$ LANGUAGE plpgsql;

CREATE TRIGGER user_consent_last_used_date_newer
BEFORE UPDATE
  ON consent.user_consent_last_usage
FOR EACH ROW
EXECUTE PROCEDURE consent.last_used_date_newer();
