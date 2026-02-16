-- Old columns (we don't use them)
ALTER TABLE data_inventory.data_use_service DROP COLUMN IF EXISTS data_retention_time;
ALTER TABLE data_inventory.data_use_cluster DROP COLUMN IF EXISTS data_retention_time;

CREATE TABLE data_inventory.retention_policy (
  id INT PRIMARY KEY,
  name TEXT UNIQUE NOT NULL
);

ALTER TABLE data_inventory.data_use ADD COLUMN retention_policy_id INT;
ALTER TABLE data_inventory.data_use ADD COLUMN retention_reason TEXT DEFAULT NULL;
ALTER TABLE data_inventory.data_use ADD COLUMN is_exemption BOOLEAN DEFAULT FALSE NOT NULL;

ALTER TABLE data_inventory.data_use ADD CONSTRAINT data_use_retention_policy_id_fkey
  FOREIGN KEY (retention_policy_id) REFERENCES data_inventory.retention_policy (id)
  ON DELETE SET NULL;

INSERT INTO data_inventory.retention_policy (id, name) VALUES
  (1, '0 ... 1 day'),
  (2, '1 day ... 1 month'),
  (3, '1 month ... 2 years'),
  (4, '2 years ... 10 years'),
  (5, '10 years ... ∞'),
  (6, 'Dynamic (Event-triggered)'),
  (7, 'Unknown');