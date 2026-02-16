-- version -> expression migration
ALTER TABLE consent.consent_version_status RENAME TO consent_expression_status;


ALTER TABLE consent.consent_version RENAME TO consent_expression;
ALTER INDEX consent.consent_version_pkey RENAME TO consent_expression_pkey;
ALTER INDEX consent.consent_version_name_consent_id_key RENAME TO consent_expression_name_consent_id_key;
ALTER TABLE consent.consent_expression RENAME CONSTRAINT consent_version_consent_id_fkey TO consent_expression_consent_id_fkey;
ALTER TABLE consent.consent_expression RENAME CONSTRAINT consent_version_status_id_fkey TO consent_expression_status_id_fkey;


ALTER TABLE consent.consent_channel RENAME COLUMN consent_version_id TO consent_expression_id;
ALTER TABLE consent.consent_channel RENAME CONSTRAINT consent_channel_consent_version_id_fkey TO consent_channel_consent_expression_id_fkey;


ALTER TABLE consent.consent_data_category RENAME COLUMN consent_version_id TO consent_expression_id;
ALTER TABLE consent.consent_data_category RENAME CONSTRAINT consent_data_category_consent_version_id_fkey TO consent_data_category_consent_expression_id_fkey;


ALTER TABLE consent.consent_version_text RENAME TO consent_expression_text;
ALTER TABLE consent.consent_expression_text RENAME COLUMN consent_version_id TO consent_expression_id;
ALTER TABLE consent.consent_expression_text RENAME CONSTRAINT consent_text_consent_version_id_fkey TO consent_text_consent_expression_id_fkey;


ALTER TABLE consent.consent_version_tag RENAME TO consent_expression_tag;
ALTER INDEX consent.consent_version_tag_pkey RENAME TO consent_expression_tag_pkey;


ALTER TABLE consent.consent_version_tag_list RENAME TO consent_expression_tag_list;
ALTER INDEX consent.consent_version_tag_list_pkey RENAME TO consent_expression_tag_list_pkey;
ALTER TABLE consent.consent_expression_tag_list RENAME COLUMN consent_version_id TO consent_expression_id;
ALTER TABLE consent.consent_expression_tag_list RENAME COLUMN consent_version_tag_id TO consent_expression_tag_id;
ALTER TABLE consent.consent_expression_tag_list RENAME CONSTRAINT consent_version_tag_list_consent_version_id_fkey TO consent_expression_tag_list_consent_expression_id_fkey;
ALTER TABLE consent.consent_expression_tag_list RENAME CONSTRAINT consent_version_tag_list_consent_version_tag_id_fkey TO consent_expression_tag_list_consent_expression_tag_id_fkey;


ALTER TABLE consent.user_consent_audit_trail RENAME COLUMN consent_version_id TO consent_expression_id;
ALTER TABLE consent.user_consent_audit_trail RENAME CONSTRAINT user_consent_audit_trail_consent_version_id_fkey TO user_consent_audit_trail_consent_expression_id_fkey;


ALTER TABLE consent.user_consent RENAME COLUMN consent_version_id TO consent_expression_id;
ALTER INDEX consent.user_consent_consent_version_id RENAME TO user_consent_consent_expression_id;
ALTER TABLE consent.user_consent RENAME CONSTRAINT user_consent_consent_version_id_fkey TO user_consent_consent_expression_id_fkey;

-- sequences rename
ALTER SEQUENCE consent.consent_version_id_seq RENAME TO consent_expression_id_seq;
ALTER SEQUENCE consent.consent_version_tag_id_seq RENAME TO consent_expression_tag_id_seq;

-- triggers rename
ALTER TRIGGER update_consent_version_modtime ON consent.consent_expression RENAME TO update_consent_expression_modtime;
ALTER TRIGGER user_consent_check_version_consistency ON consent.user_consent RENAME TO user_consent_check_expression_consistency;

-- trigger function update
CREATE OR REPLACE FUNCTION consent.user_consent_check_version_consent()
RETURNS trigger
LANGUAGE 'plpgsql'
COST 100
VOLATILE NOT LEAKPROOF
AS $BODY$

BEGIN
IF
(SELECT v.consent_id FROM consent.consent_expression v WHERE v.id = NEW.consent_expression_id) <> NEW.consent_id
THEN
RAISE EXCEPTION 'An inconsistency found. The expression_id references to another consent';
END IF;
RETURN NEW;
END;

$BODY$;

ALTER FUNCTION consent.user_consent_check_version_consent() RENAME TO user_consent_check_expression_consent;
