ALTER TABLE data_inventory.legal_basis ADD COLUMN is_legitimate_interest BOOLEAN NOT NULL DEFAULT FALSE;

UPDATE data_inventory.legal_basis SET is_consent = FALSE WHERE id = 40;
UPDATE data_inventory.legal_basis SET is_legitimate_interest = TRUE WHERE id IN (5, 40);