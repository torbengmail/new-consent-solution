CREATE SEQUENCE consent.id_type_id_seq START 105;
--;;
ALTER TABLE consent.id_type ALTER COLUMN id SET DEFAULT nextval('consent.id_type_id_seq');