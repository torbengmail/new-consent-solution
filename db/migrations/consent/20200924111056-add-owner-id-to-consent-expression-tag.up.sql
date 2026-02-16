ALTER TABLE consent.consent_expression_tag ADD COLUMN owner_id INTEGER NOT NULL default 2;
--;;
ALTER TABLE consent.consent_expression_tag ADD CONSTRAINT consent_expression_tag_unique UNIQUE (owner_id, name);
--;;
ALTER TABLE consent.consent_expression_tag ADD CONSTRAINT consent_expression_tag_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES data_inventory.owner(id);