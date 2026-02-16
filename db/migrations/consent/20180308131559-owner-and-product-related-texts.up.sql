--------------------------------------------------------------------------------
--TICS-4169 Texts(former translations) are related to the owner and/or product--
--------------------------------------------------------------------------------
ALTER TABLE consent.admin_translation ADD COLUMN owner_id INTEGER NOT NULL REFERENCES data_inventory.owner (id) DEFAULT 1;
ALTER TABLE consent.admin_translation ADD COLUMN product_id INTEGER REFERENCES data_inventory.product(id);

ALTER TABLE consent.admin_translation DROP CONSTRAINT admin_translation_pkey;
ALTER TABLE consent.admin_translation ADD COLUMN id SERIAL;
ALTER TABLE consent.admin_translation ADD PRIMARY KEY (id);

CREATE UNIQUE INDEX unique_product_id_not_null ON consent.admin_translation (lang_code, owner_id, product_id) WHERE product_id IS NOT NULL;
CREATE UNIQUE INDEX unique_product_id_null ON consent.admin_translation (lang_code, owner_id) WHERE product_id IS NULL;
