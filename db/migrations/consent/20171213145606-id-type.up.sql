-----------------------------
--TICS-4006 ID Type support--
-----------------------------
CREATE TABLE IF NOT EXISTS consent.id_type (
  id integer PRIMARY KEY,
  name TEXT NOT NULL
);

INSERT INTO consent.id_type (id, name) VALUES
(1, 'CONNECT ID'),
(2, 'Denmark Local ID'),
(3, 'Sweden Local ID');

ALTER TABLE consent.user_consent ADD COLUMN id_type_id INT NOT NULL REFERENCES consent.id_type (id) DEFAULT 1;

ALTER TABLE consent.user_consent DROP CONSTRAINT user_consent_user_id_consent_id_key;
ALTER TABLE consent.user_consent ADD UNIQUE (user_id, consent_id, id_type_id);
