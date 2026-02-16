ALTER TABLE consent.consent_expression_tag_list
    DROP CONSTRAINT consent_expression_tag_list_consent_expression_tag_id_fkey,
    ADD CONSTRAINT consent_expression_tag_list_consent_expression_tag_id_fkey
        FOREIGN KEY (consent_expression_tag_id)
            REFERENCES consent.consent_expression_tag(id)
            ON DELETE CASCADE
            ON UPDATE CASCADE;
