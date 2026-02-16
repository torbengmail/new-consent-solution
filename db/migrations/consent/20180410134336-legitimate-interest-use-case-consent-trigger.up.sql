--------------------------------------------------------------------------------------
-- TICS-4453 Trigger to enforce 1 only consent id for legitimate interest use cases	--
--------------------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION consent.legitimate_interest_use_case_consent()
RETURNS TRIGGER AS $$
DECLARE
    use_case_count INTEGER;
BEGIN

    SELECT COUNT(ucc.use_case_id) INTO use_case_count
    FROM consent.use_case_consent AS ucc
    JOIN consent.consent AS c ON ucc.consent_id = c.id
    WHERE ucc.use_case_id = NEW.use_case_id AND c.consent_type_id = 1;

    IF (use_case_count > 1) THEN
        RAISE EXCEPTION 'Only 1 use case consent is allowed for legitimate interest (consent type = 1)';
    END IF;

    RETURN NEW;

END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER legitimate_interest_use_case_consent
AFTER INSERT OR UPDATE ON consent.use_case_consent
FOR EACH ROW EXECUTE PROCEDURE consent.legitimate_interest_use_case_consent();
