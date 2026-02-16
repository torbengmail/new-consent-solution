-------------------------------------
-- TICS-4872 Admin API Audit Trail --
-------------------------------------
CREATE TABLE IF NOT EXISTS consent.admin_api_audit_trail (
    id SERIAL PRIMARY KEY,
    username VARCHAR(64) NOT NULL,
    "data" JSONB,
    "resource" VARCHAR(255) NOT NULL,
    action VARCHAR(255) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT now()
);
