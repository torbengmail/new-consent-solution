CREATE UNLOGGED TABLE consent.user_data_cache (
  user_id TEXT,
  id_type_id INT,
  data_key TEXT,
  data_value TEXT,
  modified_date TIMESTAMPTZ NOT NULL DEFAULT now(),
  PRIMARY KEY(user_id, id_type_id, data_key)
);