CREATE TABLE consent.id_map (
      id_type_id int4 NOT NULL,
      name TEXT NOT NULL,
      CONSTRAINT consent_id_map_pkey PRIMARY KEY (id_type_id)
);
--;;
-- Constraints
ALTER TABLE consent.id_map ADD CONSTRAINT consent_id_map_consent_id_type_fkey
    FOREIGN KEY (id_type_id) REFERENCES consent.id_type(id);
--;;
-- Permissions
ALTER TABLE consent.id_map OWNER TO privacy;
GRANT ALL ON TABLE consent.id_map TO privacy;
-- Insert
--;;
INSERT INTO consent.id_type(id, name) VALUES (5 , 'Sweden IAM Party ID')
    ON CONFLICT ON CONSTRAINT id_type_pkey DO NOTHING;
--;;
INSERT INTO consent.id_map (id_type_id, name) VALUES
    (1, 'CID'),
    (2, 'TNDK_NEM'),
    (3, 'TNSE_IAM'),
    (4, 'TNN_KURT'),
    (5, 'TNSE_PARTY');