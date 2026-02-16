CREATE TABLE consent.test_user_group (
  id INT PRIMARY KEY,
  name TEXT NOT NULL UNIQUE,
  owner_id INT NOT NULL REFERENCES data_inventory.owner (id) ON DELETE CASCADE
);

CREATE TABLE consent.test_user (
  user_id TEXT,
  id_type_id INT REFERENCES consent.id_type(id),
  test_user_group_id INT REFERENCES consent.test_user_group (id) ON DELETE CASCADE,
  UNIQUE (user_id, id_type_id, test_user_group_id)
);

ALTER TABLE consent.request_attempt_audit_trail
  DROP CONSTRAINT request_attempt_audit_trail_attempt_id_fkey;

ALTER TABLE consent.request_attempt_audit_trail
  ADD CONSTRAINT request_attempt_audit_trail_attempt_id_fkey
FOREIGN KEY (attempt_id) REFERENCES consent.request_attempt (id) ON DELETE CASCADE;


ALTER TABLE consent.user_consent_last_usage
    DROP CONSTRAINT user_consent_last_usage_user_consent_id_fkey;

ALTER TABLE consent.user_consent_last_usage
  ADD CONSTRAINT user_consent_last_usage_user_consent_id_fkey
FOREIGN KEY (user_consent_id) REFERENCES consent.user_consent (id) ON DELETE CASCADE;


ALTER TABLE consent.user_consent_audit_trail
  DROP CONSTRAINT IF EXISTS user_consent_audit_trail_user_consent_id_fkey;

-- Renaming constraint _user_consent_id_ -> _decision_id_
ALTER TABLE consent.user_consent_audit_trail
  DROP CONSTRAINT IF EXISTS user_consent_audit_trail_decision_id_fkey;

ALTER TABLE consent.user_consent_audit_trail
  ADD CONSTRAINT user_consent_audit_trail_decision_id_fkey
FOREIGN KEY (decision_id) REFERENCES consent.user_consent (id) ON DELETE CASCADE;
