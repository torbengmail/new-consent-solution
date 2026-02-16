CREATE TABLE consent.master_id (
  id UUID,
  user_id TEXT,
  id_type_id INT,
  is_device_id BOOLEAN DEFAULT FALSE,
  modified_date TIMESTAMPTZ NOT NULL DEFAULT now(),
  PRIMARY KEY (user_id, id_type_id)
);
CREATE INDEX master_id_id ON consent.master_id (id);

-- SET NOT NULL after migration
ALTER TABLE consent.user_consent ADD COLUMN master_id UUID NULL;
ALTER TABLE consent.request_attempt ADD COLUMN master_id UUID NULL;

ALTER TABLE consent.user_consent_audit_trail ADD COLUMN user_id TEXT NULL;
ALTER TABLE consent.request_attempt_audit_trail ADD COLUMN user_id TEXT NULL;

ALTER TABLE consent.user_consent_audit_trail ADD COLUMN id_type_id INT NULL;
ALTER TABLE consent.request_attempt_audit_trail ADD COLUMN id_type_id INT NULL;

-- All commented lines are part of manual migration process

----- comment when deploy on prod start
ALTER TABLE consent.user_consent DROP CONSTRAINT user_consent_user_id_consent_id_id_type_id_key;
ALTER TABLE consent.request_attempt DROP CONSTRAINT request_attempt_consent_id_user_id_id_type_id_key;

ALTER TABLE consent.user_consent ADD UNIQUE (master_id, consent_id);
ALTER TABLE consent.request_attempt ADD UNIQUE (master_id, consent_id);

UPDATE consent.user_consent_audit_trail AS ucat
SET user_id = uc.user_id, id_type_id = uc.id_type_id
FROM consent.user_consent AS uc
WHERE ucat.decision_id = uc.id;

UPDATE consent.request_attempt_audit_trail AS raat
SET user_id = ra.user_id, id_type_id = ra.id_type_id
FROM consent.request_attempt AS ra
WHERE raat.attempt_id = ra.id;

ALTER TABLE consent.user_consent_audit_trail ALTER COLUMN user_id SET NOT NULL;
ALTER TABLE consent.user_consent_audit_trail ALTER COLUMN id_type_id SET NOT NULL;
ALTER TABLE consent.user_consent_audit_trail ADD CONSTRAINT user_consent_audit_trail_id_type_fkey FOREIGN KEY (id_type_id) REFERENCES consent.id_type(id);

ALTER TABLE consent.request_attempt_audit_trail ALTER COLUMN user_id SET NOT NULL;
ALTER TABLE consent.request_attempt_audit_trail ALTER COLUMN id_type_id SET NOT NULL;
ALTER TABLE consent.request_attempt_audit_trail ADD CONSTRAINT request_attempt_audit_trail_id_type_fkey FOREIGN KEY (id_type_id) REFERENCES consent.id_type(id);
----- comment when deploy on prod end
