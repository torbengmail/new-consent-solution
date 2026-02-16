ALTER TABLE consent.dsr_tracking DROP CONSTRAINT dsr_tracking_pkey;
ALTER TABLE consent.dsr_tracking ADD PRIMARY KEY (ticket_id, user_id, id_type_id, type);