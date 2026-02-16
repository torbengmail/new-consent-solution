ALTER TABLE consent.user ALTER COLUMN password DROP NOT NULL;

CREATE OR REPLACE FUNCTION consent.user_password_check()
  RETURNS TRIGGER AS $$
BEGIN
  IF (new.password IS NULL OR LENGTH(new.password) = 0) AND new.is_connect_id IS NOT TRUE
  THEN
     RAISE EXCEPTION 'Password cannot be empty or null.';
  END IF;
  RETURN NEW;
END;
$$ language 'plpgsql';

CREATE TRIGGER user_password_check
  BEFORE UPDATE OR INSERT ON consent.user
  FOR EACH ROW
EXECUTE PROCEDURE consent.user_password_check();