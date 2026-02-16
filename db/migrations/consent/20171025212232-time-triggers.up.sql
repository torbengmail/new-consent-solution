CREATE OR REPLACE FUNCTION consent.update_modified_column()
  RETURNS TRIGGER AS $$
BEGIN
  NEW.modified_date = now();
  RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER update_consent_modtime
BEFORE UPDATE
  ON consent.consent
FOR EACH ROW
EXECUTE PROCEDURE consent.update_modified_column();

CREATE TRIGGER update_consent_version_modtime
BEFORE UPDATE
  ON consent.consent_version
FOR EACH ROW
EXECUTE PROCEDURE consent.update_modified_column();
