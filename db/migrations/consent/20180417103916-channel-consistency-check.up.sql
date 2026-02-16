CREATE OR REPLACE FUNCTION consent.check_channels_consistency()
  RETURNS TRIGGER AS $$
DECLARE
  parent consent.consent%ROWTYPE;
BEGIN
  IF (new.parent_consent_id IS NOT NULL)
  THEN
    SELECT * INTO parent FROM consent.consent WHERE id=new.parent_consent_id;
    IF parent.parent_consent_id IS NOT NULL
    THEN
      RAISE EXCEPTION '3-level hierarchy is forbidden. Can not use consent % as a parent.', new.parent_consent_id;
    END IF;
    IF new.is_group IS TRUE
    THEN
      RAISE EXCEPTION '3-level hierarchy is forbidden. Flag is_group can not set true when consent has a parent.';
    END IF;
    IF parent.consent_type_id <> 3
    THEN
      RAISE EXCEPTION 'Only GDPR Marketing consents can have channel consents';
    END IF;
    IF parent.is_group IS NOT TRUE
    THEN
      RAISE EXCEPTION 'Parent consent should be a group';
    END IF;
    IF parent.owner_id <> new.owner_id OR parent.purpose_id <> new.purpose_id OR parent.product_id <> new.product_id OR
       parent.consent_type_id <> new.consent_type_id
    THEN
      RAISE EXCEPTION 'Channel consent should inherit (owner_id, purpose_id, consent_type_id, product_id) fields from the parent.';
    END IF;
  END IF;
  RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER consent_check_channels_consistency
  BEFORE UPDATE OR INSERT ON consent.consent
  FOR EACH ROW
EXECUTE PROCEDURE consent.check_channels_consistency();