-- Entity
-- A single app, service or organization that creates and own consents

CREATE TABLE entity (
  id SERIAL PRIMARY KEY,
  short_name VARCHAR(64) NOT NULL UNIQUE,
  description TEXT NOT NULL,
  entity_group VARCHAR(128)
);

-- Consent
-- The definition of a single agreement (versioned)

CREATE TABLE consent (
  id BIGSERIAL UNIQUE,
  short_name VARCHAR(64) NOT NULL, -- Immutable across consent versions, identifies the consent. Eg. title+entity
  entity_id INTEGER NOT NULL,
  version INTEGER NOT NULL DEFAULT 1,
  version_changes JSONB,
  title JSONB NOT NULL,
  short_description JSONB NOT NULL,
  long_description JSONB NOT NULL,
  implications TEXT,
  category TEXT,
  created TIMESTAMP WITHOUT TIME ZONE DEFAULT now(),
  PRIMARY KEY (short_name, version, entity_id),
  FOREIGN KEY (entity_id) REFERENCES entity (id) ON DELETE CASCADE
);

CREATE INDEX consent_short_name ON consent USING BTREE (short_name);

-- User Consent
-- A single agreement from the user

CREATE TABLE user_consent (
  consent_id BIGINT NOT NULL,
  user_id VARCHAR(64) NOT NULL,
  last_asked TIMESTAMP WITHOUT TIME ZONE,
  agreed BOOLEAN,
  source_of_agreement TEXT NOT NULL,
  presented_locale VARCHAR(64) NOT NULL DEFAULT 'en',
  PRIMARY KEY (consent_id, user_id),
  FOREIGN KEY (consent_id) REFERENCES consent (id) ON DELETE RESTRICT
);

CREATE INDEX user_consent_consent_id ON user_consent USING BTREE (consent_id);
CREATE INDEX user_consent_user_id ON user_consent USING BTREE (user_id);
