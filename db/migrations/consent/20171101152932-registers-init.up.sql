INSERT INTO consent.consent_type (id, name, default_opt_in) VALUES
(1, 'Objection', true),
(2, 'General GDPR Consent', false),
(3, 'GDPR Marketing Consent', true),
(4, 'E-Privacy Consent', false),
(5, 'GDPR Special Data Category Consent', false),
(6, 'GDPR Consent for Profiling with Legal Implications', false);

INSERT INTO consent.consent_version_status (id, name) VALUES 
(1, 'Draft'), 
(2, 'Published');

INSERT INTO consent.communication_channel (id, name) VALUES
(1, 'E-mail'),
(2, 'SMS'),
(3, 'Push'),
(4, 'Phone');

INSERT INTO consent.user_consent_source_type (id, name) VALUES
(1, 'Mobile Application'),
(2, 'Website'),
(3, 'SMS portal'),
(4, 'Phone Call');
