ALTER TABLE consent.user_consent ADD COLUMN owner_id INTEGER;

UPDATE consent.user_consent AS uc
SET owner_id = c.owner_id
FROM consent.consent c
WHERE c.id = uc.consent_id AND uc.owner_id IS NULL;

ALTER TABLE consent.user_consent ALTER COLUMN owner_id SET NOT NULL;
ALTER TABLE consent.user_consent ADD CONSTRAINT fk_user_consent_owner_id FOREIGN KEY (owner_id) REFERENCES data_inventory.owner (id);

CREATE INDEX user_consent_owner_id_idx ON consent.user_consent USING btree (owner_id);