----------------------------------------------
-- TICS-4849 Add new dump exchange approach --
----------------------------------------------
CREATE TABLE IF NOT EXISTS consent.user_consent_decisions_batch (
    "id" SERIAL PRIMARY KEY,
    "hash" CHAR(64) NOT NULL,
    "params" JSONB,
    "owner_id" INT NOT NULL REFERENCES data_inventory.owner (id),
    "download_url" TEXT,
    "status" VARCHAR(32),
    "created_at" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS user_consent_decisions_batch_idx ON consent.user_consent_decisions_batch USING btree (owner_id, hash, created_at);
