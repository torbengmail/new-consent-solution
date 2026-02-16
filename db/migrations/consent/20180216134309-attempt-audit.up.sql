CREATE TABLE IF NOT EXISTS consent.request_attempt_audit_trail (
  id SERIAL PRIMARY KEY,
  attempt_id INT NOT NULL REFERENCES consent.request_attempt (id),
  date TIMESTAMPTZ NOT NULL DEFAULT now(),
  presented_language TEXT NOT NULL REFERENCES consent.language (name) DEFAULT 'en',
  consent_expression_id INT NOT NULL REFERENCES consent.consent_version (id)
);
