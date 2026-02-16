CREATE OR REPLACE FUNCTION consent.user_consent_check_version_consent()
  RETURNS TRIGGER AS $$
BEGIN
   IF
   (SELECT v.consent_id FROM consent.consent_version v WHERE v.id = NEW.consent_version_id) <> NEW.consent_id
   THEN
        RAISE EXCEPTION 'An inconsistency found. The version_id references to another consent';
   END IF;
  RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER user_consent_check_version_consistency
    BEFORE UPDATE OR INSERT ON consent.user_consent
    FOR EACH ROW
    EXECUTE PROCEDURE consent.user_consent_check_version_consent();
