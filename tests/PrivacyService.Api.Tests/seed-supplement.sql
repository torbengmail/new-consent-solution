-- Create admin_api_audit_trail table (not in Clojure migrations)
CREATE TABLE IF NOT EXISTS consent.admin_api_audit_trail (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    action VARCHAR(50) NOT NULL,
    entity_type VARCHAR(50),
    entity_id VARCHAR(100),
    details JSONB,
    date TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Additional permissions for authorization checks (C1-C3)
INSERT INTO consent.permission (id, name) VALUES
  (55, 'READ_DATA_DUMP'),
  (56, 'DELETE_TEST_USER'),
  (57, 'CREATE_ID_TYPE'),
  (58, 'CREATE_ID_MAPPING')
ON CONFLICT (id) DO NOTHING;

-- Grant new permissions to ADMIN role (role_id=3)
INSERT INTO consent.role_permission (role_id, permission_id) VALUES
  (3, 55),
  (3, 56),
  (3, 57),
  (3, 58)
ON CONFLICT DO NOTHING;

-- Grant to SUPERADMIN role (role_id=5) as well
INSERT INTO consent.role_permission (role_id, permission_id) VALUES
  (5, 55),
  (5, 56),
  (5, 57),
  (5, 58)
ON CONFLICT DO NOTHING;
