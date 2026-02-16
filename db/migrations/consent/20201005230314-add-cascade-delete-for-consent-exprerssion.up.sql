ALTER TABLE consent.consent_expression_tag_list
    DROP constraint consent_expression_tag_list_consent_expression_id_fkey,
    ADD CONSTRAINT consent_expression_tag_list_consent_expression_id_fkey
        FOREIGN KEY (consent_expression_id)
            REFERENCES consent.consent_expression(id)
            ON DELETE CASCADE
            ON UPDATE CASCADE;
--;;
ALTER TABLE consent.consent_expression_text
    DROP constraint consent_text_consent_expression_id_fkey,
    ADD CONSTRAINT consent_text_consent_expression_id_fkey
        FOREIGN KEY (consent_expression_id)
            REFERENCES consent.consent_expression(id)
            ON DELETE CASCADE
            ON UPDATE CASCADE;