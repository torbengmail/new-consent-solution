CREATE TABLE IF NOT EXISTS consent.request_attempt (
  id SERIAL PRIMARY KEY,
  presented_language TEXT NOT NULL REFERENCES consent.language (name) DEFAULT 'en',
  consent_expression_id INT NOT NULL REFERENCES consent.consent_version (id),
  consent_id INT NOT NULL REFERENCES consent.consent (id),
  user_id TEXT NOT NULL,
  id_type_id INT NOT NULL REFERENCES consent.id_type (id) DEFAULT 1,
  last_asked_date TIMESTAMPTZ NOT NULL DEFAULT now(),
  attempts_count INT NOT NULL DEFAULT 1,
  UNIQUE (consent_id, user_id, id_type_id)
);
