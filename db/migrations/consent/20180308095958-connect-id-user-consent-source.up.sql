--------------------------------------------------------
--TICS-4406 Default user consent source for CONNECT ID--
--------------------------------------------------------
ALTER TABLE consent.user_consent_source ADD COLUMN owner_id INTEGER NOT NULL REFERENCES data_inventory.owner (id) DEFAULT 1;
ALTER TABLE consent.user_consent_source ADD COLUMN product_id INTEGER REFERENCES data_inventory.product(id);

INSERT INTO consent.user_consent_source (id, name, description, owner_id, product_id, user_consent_source_type_id) VALUES (0, 'CONNECT ID', '', 1, 1, 2);
