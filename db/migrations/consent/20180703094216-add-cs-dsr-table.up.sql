CREATE TYPE status_dsr_processing AS ENUM ('open','error','processing', 'done');
CREATE TYPE type_dsr_processing AS ENUM ('export', 'deletion');

CREATE TABLE consent.dsr_tracking (
  ticket_id TEXT PRIMARY KEY,
  user_id TEXT NOT NULL,
  id_type_id INTEGER REFERENCES consent.id_type (id),
  created_date TIMESTAMP NOT NULL DEFAULT now(),
  updated_date TIMESTAMP,
  type type_dsr_processing NOT NULL,
  status status_dsr_processing NOT NULL DEFAULT 'open'
);
