-- Consents for the public namespace. Should be gone once Connect ID stops sending data to the non versioned endpoint.
INSERT INTO entity (short_name,description, entity_group) VALUES ('capture','Capture', 'Telenor Digital');
INSERT INTO entity (short_name,description, entity_group) VALUES ('mycontacts','MyContacts', 'Telenor Digital');
INSERT INTO consent (short_name,entity_id,version,version_changes,title,short_description,long_description,implications,category,created)
VALUES ('geo-td',1,1,null,'{"en": "Geolocation"}','{"en": "Geolocation based offers"}','{"en": "Geolocation based offers long description"}',null,'GEOLOCATION','2017-02-23');
INSERT INTO consent (short_name,entity_id,version,version_changes,title,short_description,long_description,implications,category,created)
VALUES ('marketing-td',1,1,null,'{"en": "Marketing"}','{"en": "Marketing title"}','{"en": "Marketing long description"}',null,'MARKETING','2017-02-23');
INSERT INTO consent (short_name,entity_id,version,version_changes,title,short_description,long_description,implications,category,created)
VALUES ('share-third-party',1,1,null,'{"en": "Share data"}','{"en": "Share data title"}','{"en": "Share data long description"}',null,'SHARE','2017-02-23 12:00');
INSERT INTO consent (short_name,entity_id,version,version_changes,title,short_description,long_description,implications,category,created)
VALUES ('geo-td',1,2,null,'{"en": "Geolocation V2"}','{"en": "Geolocation based offers"}','{"en": "Geolocation based offers long description"}',null,'GEOLOCATION','2017-02-25');
INSERT INTO user_consent (consent_id,user_id,last_asked,agreed,source_of_agreement) VALUES (1,'111','2017-02-22 12:01:00',true,'CONNECT ID T&C');
INSERT INTO user_consent (consent_id,user_id,last_asked,agreed,source_of_agreement) VALUES (2,'111','2017-02-22 12:01:00',true,'CONNECT ID T&C');
INSERT INTO user_consent (consent_id,user_id,last_asked,agreed,source_of_agreement) VALUES (3,'111','2017-02-22 12:02:00',false,'CONNECT ID T&C');
INSERT INTO user_consent (consent_id,user_id,last_asked,agreed,source_of_agreement) VALUES (4,'111','2017-02-22 12:02:00',true,'CONNECT ID T&C');
INSERT INTO user_consent (consent_id,user_id,last_asked,agreed,source_of_agreement) VALUES (1,'9065392725727911111','2017-02-23 12:01:00',true,'Privacy Dashboard');
INSERT INTO user_consent (consent_id,user_id,last_asked,agreed,source_of_agreement) VALUES (2,'9065392725727911111','2017-02-23 12:02:00',false,'Privacy Dashboard');
INSERT INTO user_consent (consent_id,user_id,last_asked,agreed,source_of_agreement) VALUES (3,'9065392725727911111','2017-02-23 12:03:00',true,'Privacy Dashboard');

-- Consents for the consent namespace
INSERT INTO consent.consent_expression_tag (id, name, owner_id) VALUES (1, 'privacy-dashboard', 1);
INSERT INTO consent.consent_expression_tag (id, name, owner_id) VALUES (2, 'capture', 1);
INSERT INTO consent.consent_expression_tag (id, name, owner_id) VALUES (69, 'delete-me', 1);

INSERT INTO consent.consent (id, owner_id, product_id, consent_type_id, purpose_id, name, description, parent_consent_id, is_group)
VALUES
  (99,  1, null, 1, 7, 'objection', 'objection description', null, false),
  (101, 1, 8, 3, 7, 'direct communication', 'direct communication description', null, true),
  (201, 1, 8, 3, 7, 'direct marketing by sms', 'direct marketing by sms description', 101, false),
  (202, 1, 8, 3, 7, 'direct marketing by email', 'direct marketing by email description', 101, false),
  (203, 1, 8, 3, 7, 'direct marketing by push', 'direct marketing by push description', 101, false),
  (204, 1, 8, 2, 5, 'personalised marketing', 'personalised marketing description', null, false),
  (111, 6, 8, 3, 7, 'direct communication Denmark-Capture', 'direct communication Denmark-Capture description', null, true),
  (112, 6, 18, 3, 7, 'direct communication Denmark-My-Telenor', 'direct communication Denmark-My-Telenor description', null, true),
  (211, 6, 8, 3, 7, 'direct marketing by sms', 'direct marketing by sms description', 111, false),
  (212, 6, 8, 3, 7, 'direct marketing by email', 'direct marketing by email description', 111, false),
  (213, 6, 8, 3, 7, 'direct marketing by push', 'direct marketing by push description', 111, false),
  (214, 6, 8, 2, 5, 'personalised marketing', 'personalised marketing description', null, false),
  (215, 6, 18, 3, 7, 'news through email', 'news through email description', 112, false),
  (216, 6, 18, 3, 7, 'news through online platforms', 'news through online platforms description', 112, false),
  (217, 6, 18, 3, 7, 'news through message/call', 'news through message/call description', 112, false),
  (218, 6, 18, 2, 5, 'location data', 'location data description', null, false);

-- Expired consents
INSERT INTO consent.consent (id, owner_id, product_id, consent_type_id, purpose_id, name, description, parent_consent_id, is_group, expiration_date)
VALUES
  (289, 1, 8, 3, 7, 'Expired Consent #289', 'Expired Consent #289', null, false, '2018-03-30 00:00:01.5000+02'),
  (290, 1, 8, 3, 7, 'Expired Consent #290', 'Expired Consent #290', null, false, '2018-03-30 00:00:01.5000+02');


-- Consent for testing deletion
INSERT INTO consent.consent (id, owner_id, product_id, consent_type_id, purpose_id, name, description, parent_consent_id, is_group)
VALUES
(1598,  1, null, 1, 7, 'will be deleted in a test', 'objection description', null, false);

-- Soft deleted consents
INSERT INTO consent.consent (id, owner_id, product_id, consent_type_id, purpose_id, name, description, parent_consent_id, is_group, delete_at)
VALUES
(1600,  1, null, 1, 7, 'deleted consent - a', 'objection description', null, false, '2016-04-30 00:00:01.5000+02'),
(1601, 1, 8, 3, 7, 'deleted consent - b', 'direct communication description', null, true, '2017-07-30 00:00:01.5000+02'),
(1602, 1, 8, 3, 7, 'deleted consent - c', 'direct marketing by sms description', 101, false, '2019-03-30 00:00:01.5000+02'),
(1603, 1, 8, 3, 7, 'deleted consent - d', 'direct marketing by email description', 101, false, '2020-03-30 00:00:01.5000+02'),
(1604, 1, 8, 3, 7, 'deleted consent - e', 'direct marketing by push description', 101, false, '2020-04-30 00:00:01.5000+02'),
(1605, 6, 8, 3, 7, 'deleted consent - f', 'direct communication Denmark-Capture description', null, true, '2020-05-30 00:00:01.5000+02'),
(1606, 6, 18, 3, 7, 'deleted consent - g', 'direct communication Denmark-My-Telenor description', null, true, '2020-06-30 00:00:01.5000+02'),
(1607, 6, 18, 3, 7, 'deleted consent - h', 'news through message/call description', 112, false, '2020-07-30 00:00:01.5000+02'),
(1608, 6, 18, 2, 5, 'deleted consent - i', 'location data description', null, false, '2020-08-30 00:00:01.5000+02');

INSERT INTO consent.consent_expression (id, name, consent_id, status_id, is_default)
VALUES
  (101, 'direct communication default expression', 101, 2, true),
  (301, 'direct marketing by sms default expression', 201, 2, true),
  (302, 'direct marketing by email default expression', 202, 2, true),
  (303, 'direct marketing by push default expression', 203, 2, true),
  (304, 'personalised marketing default expression', 204, 2, true),
  (111, 'direct communication Denmark-Capture default expression', 111, 2, true),
  (112, 'direct communication Denmark-My-Telenor default expression', 112, 2, true),
  (311, 'direct marketing by sms default expression', 211, 2, true),
  (312, 'direct marketing by email default expression', 212, 2, true),
  (313, 'direct marketing by push default expression', 213, 2, true),
  (314, 'personalised marketing default expression', 214, 2, true),
  (315, 'news through email default expression', 215, 2, true),
  (316, 'news through online platforms expression', 216, 2, true),
  (317, 'news through message/call', 217, 2, true),
  (318, 'location data default expression 1', 218, 2, true);

INSERT INTO consent.consent_expression (id, name, consent_id, status_id, is_default, created_date)
VALUES
  (319, 'location data default expression 2', 218, 2, true,'2001-01-01 00:00:01.5000+02');

INSERT INTO consent.consent_expression_text (consent_expression_id, language, title, short_text, long_text)
VALUES
  (101, 'en', 'Stay updated', 'I agree to Capture can send me updates, news, and information via', 'LONG LEGAL TEXT'),
  (101, 'no', 'Hold deg oppdatert', 'Jeg godkjenner at MinSky kan sende meg nyheter og relevant informasjon via:','LONG LEGAL TEXT'),
  (301, 'en', 'SMS', 'SHORT TEXT', 'LONG LEGAL TEXT'),
  (302, 'en', 'Email', 'SHORT TEXT', 'LONG LEGAL TEXT'),
  (303, 'en', 'Push notifications', 'SHORT TEXT', 'LONG LEGAL TEXT'),
  (304, 'en', 'Get more out of Telenor',
   'Share my contact information and Capture data (e.g., how frequently I use the app) with Telenor, so that I can get personalised deals.',
   'LONG LEGAL TEXT'),
  (111, 'en', 'Stay up to date', 'I agree that Capture can send me news and relevant information via:','LONG LEGAL TEXT'),
  (111, 'no', 'Hold deg oppdatert', 'I agree that Capture can send me news and relevant information via:','LONG LEGAL TEXT'),
  (112, 'en', 'Get relevant news', 'I agree that Telenor A/S can contact me with relevant information and discounts through','LONG LEGAL TEXT'),
  (311, 'en', 'SMS', 'SHORT TEXT', 'LONG LEGAL TEXT'),
  (312, 'en', 'email', 'SHORT TEXT', 'LONG LEGAL TEXT'),
  (313, 'en', 'push notifications', 'SHORT TEXT', 'LONG LEGAL TEXT'),
  (314, 'en', 'Get more out of Telenor',
   'Share my contact information and Capture data (e.g., how frequently I use the app) with Telenor, so that I can get personalised deals.',
    'LONG LEGAL TEXT'),
  (315, 'en', 'email', 'SHORT TEXT', 'LONG LEGAL TEXT'),
  (316, 'en', 'online platforms and services I use', 'SHORT TEXT', 'LONG LEGAL TEXT'),
  (317, 'en', 'my phone (message/call)', 'SHORT TEXT', 'LONG LEGAL TEXT'),
  (318, 'en', 'Share your (geo) location with Telenor',
    'I agree that Telenor A/S can gather and process my location data from the communication devices I use on the Telenor network, in relation to my Telenor subscription(s), to provide me with relevant information and discounts.',
     'LONG LEGAL TEXT');

INSERT INTO consent.consent_expression_tag_list (consent_expression_id, consent_expression_tag_id)
VALUES
  (101, 1),
  (101, 2),
  (301, 1),
  (302, 1),
  (303, 1),
  (304, 1),
  (111, 1),
  (112, 1),
  (311, 1),
  (312, 1),
  (313, 1),
  (314, 1),
  (315, 1),
  (316, 1),
  (317, 1),
  (318, 1);

INSERT INTO consent.user_consent_source(id, name, description, user_consent_source_type_id)
VALUES (3, 'Capture', ' ', 1);

INSERT INTO consent.user_consent_source(id, name, description, user_consent_source_type_id, owner_id, product_id)
VALUES
    (5, 'Test name 5', 'Test description 5', 1, 1, 1),
    (7, 'Test name 7', 'Test description 7', 1, 1, 1),
    (9, 'Test name 9', 'Test description 9', 1, 1, 2);

INSERT INTO consent.use_case_consent(use_case_id,consent_id)
VALUES
(1001, 201),
(1001, 203),
(1001, 213);

ALTER SEQUENCE consent.user_consent_source_id_seq RESTART WITH 1033;

INSERT INTO consent.admin_translation(owner_id, product_id, lang_code, translations, augmented_translations)
VALUES
  (1, 1, 'en',
   '{"home": {"home_title": "HomeEN", "home_summary": "HomeSummaryEN"}, "review": {"review_title": "ReviewsEN", "review_summary": "ReviewsSummaryEN"}}',
   '{"home": {"home_title": "HomeEN", "home_summary": "HomeSummaryEN"}, "review": {"review_title": "ReviewsEN", "review_summary": "ReviewsSummaryEN"}}'),
  (1, 1, 'ru',
   '{"home": {"home_title": "HomeRU"}, "review": {"review_title": "ReviewsRU"}}',
   '{"home": {"home_title": "HomeRU", "home_summary": "HomeSummaryEN"}, "review": {"review_title": "ReviewsRU", "review_summary": "ReviewsSummaryEN"}}'),
  (1, 1, 'it',
   '{"home": {"home_title": "HomeIT"}, "review": {"review_title": "ReviewsIT"}}',
   '{"home": {"home_title": "HomeIT", "home_summary": "HomeSummaryEN"}, "review": {"review_title": "ReviewsIT", "review_summary": "ReviewsSummaryEN"}}'),
  (1, 2, 'en',
   '{"home": {"home_title": "HomeEN", "home_summary": "HomeSummaryEN"}, "review": {"review_title": "ReviewsEN", "review_summary": "ReviewsSummaryEN"}}',
   '{"home": {"home_title": "HomeEN", "home_summary": "HomeSummaryEN"}, "review": {"review_title": "ReviewsEN", "review_summary": "ReviewsSummaryEN"}}'),
  (1, 2, 'bg',
   '{"home": {"home_title": "HomeBG"}, "review": {"review_title": "ReviewsBG"}}',
   '{"home": {"home_title": "HomeBG", "home_summary": "HomeSummaryEN"}, "review": {"review_title": "ReviewsBG", "review_summary": "ReviewsSummaryEN"}}'),
  (1, NULL, 'en',
   '{"home": {"home_title": "HomeEN", "home_summary": "HomeSummaryEN"}, "review": {"review_title": "ReviewsEN", "review_summary": "ReviewsSummaryEN"}}',
   '{"home": {"home_title": "HomeEN", "home_summary": "HomeSummaryEN"}, "review": {"review_title": "ReviewsEN", "review_summary": "ReviewsSummaryEN"}}'),
  (1, NULL, 'lt',
   '{"home": {"home_title": "HomeLT", "home_summary": "HomeSummaryLT"},
              "review": {"review_title": "ReviewsLT", "review_summary": "ReviewsSummaryL [PRODUCT_NAME]"},
              "legal": {"legal_texts_title": "LegalTitleLT", "legal_tab{0}_content": "LegalContentLT"}}',
   '{"home": {"home_title": "HomeLT", "home_summary": "HomeSummaryLT"},
              "review": {"review_title": "ReviewsLT", "review_summary": "ReviewsSummaryLT [PRODUCT_NAME] URL: [PRODUCT_MANAGE_URL]"},
              "legal": {"legal_texts_title": "LegalTitleLT", "legal_tab{0}_content": "LegalContentLT"}}'),
  (1, NULL, 'bg',
   '{"home": {"home_title": "HomeBG_product_null"}, "review": {"review_title": "ReviewsBG_product_null"}}',
   '{"home": {"home_title": "HomeBG_product_null", "home_summary": "HomeSummaryEN"}, "review": {"review_title": "ReviewsBG_product_null", "review_summary": "ReviewsSummaryEN"}}'),
  (6, NULL, 'en',
   '{"home": {"home_title": "HomeEN", "home_summary": "HomeSummaryEN", "home_review": "ReviewHomeEN"}, "review": {"review_title": "ReviewsEN", "review_summary": "ReviewsSummaryEN"}}',
   '{"home": {"home_title": "HomeEN", "home_summary": "HomeSummaryEN", "home_review": "ReviewHomeEN"}, "review": {"review_title": "ReviewsEN", "review_summary": "ReviewsSummaryEN"}}'),
  (6, NULL, 'bg',
   '{"home": {"home_title": "HomeBG_product_null", "home_review": "ReviewHomeBG"}, "review": {"review_title": "ReviewsBG_product_null"}}',
   '{"home": {"home_title": "HomeBG_product_null", "home_summary": "HomeSummaryEN", "home_review": "ReviewHomeBG"}, "review": {"review_title": "ReviewsBG_product_null", "review_summary": "ReviewsSummaryEN"}}');

INSERT INTO consent.user_consent (id, consent_expression_id, user_id, master_id,is_agreed, user_consent_source_id, change_context, consent_id, owner_id) VALUES
  (7000,111,'222','ecdea009-b706-3365-b882-a13e8386d090', true,3,'{}', 111, 6),
  (7001,111,'333','20156a51-f2c5-39dc-9e31-e9f7cc518e7d', true,3,'{}', 111, 6);

INSERT INTO consent.user_consent (id, consent_id, user_id, master_id, id_type_id, consent_expression_id, is_agreed, user_consent_source_id, change_context, last_decision_date, last_seen_date, owner_id)
VALUES
    (8000, 111, '2222', 'df6fb61d-2fdf-3656-88d3-44f34132a97c', 1, 111, true, 3, '{}', '2018-04-01 00:00:00', '2018-04-01 00:00:01', 6),
    (8001, 111, '3333', 'fd6df754-8a47-3212-beeb-c64af7bdedc1', 1, 111, true, 3, '{}', '2018-04-02 00:00:00', '2018-04-02 00:00:01', 6),
    (8002, 112, '2222', 'df6fb61d-2fdf-3656-88d3-44f34132a97c', 1, 112, false, 3, '{}', '2018-04-03 00:00:00', '2018-04-03 00:00:01', 6),
    (8003, 112, '3333', 'fd6df754-8a47-3212-beeb-c64af7bdedc1', 1, 112, false, 3, '{}', '2018-04-04 00:00:00', '2018-04-04 00:00:01', 6);

INSERT INTO consent.test_user_group(id, name, owner_id) VALUES (1, 'capture', 1);
INSERT INTO consent.test_user(user_id, id_type_id, test_user_group_id)
VALUES ('222', 1, 1);

INSERT INTO consent.request_attempt (id, presented_language, consent_expression_id, consent_id, user_id, id_type_id, master_id, last_asked_date, attempts_count)
VALUES
  (7000, 'en', 111, 111, '222', 1, 'ecdea009-b706-3365-b882-a13e8386d090','2018-02-22 12:02:00', 42),
  (7001, 'en', 111, 111, '333', 1, '20156a51-f2c5-39dc-9e31-e9f7cc518e7d', '2018-02-22 12:02:00', 1);

INSERT INTO consent.request_attempt_audit_trail(id, attempt_id, date, presented_language, consent_expression_id, user_id, id_type_id)
VALUES
  (70000, 7000, '2018-02-22 12:02:00', 'en', 111, '222', 1),
  (70001, 7000, '2018-02-22 12:02:00', 'en', 111, '222', 1),
  (70002, 7001, '2018-02-22 12:02:00', 'en', 111, '333', 1);

INSERT INTO consent.user_consent_audit_trail(id, decision_id, is_agreed, user_consent_source_id, change_context, consent_expression_id, user_id, id_type_id)
VALUES
  (9001, 7000, false, 3, '{}', 111, '222', 1),
  (9002, 7000, true, 3, '{}', 111, '222', 1),
  (9003, 7001, false, 3, '{}', 111, '333', 1);

UPDATE consent.language SET description='English', flag_key='gb' WHERE name='en';

INSERT INTO consent.product_connect_id (product_id, connect_id_name) VALUES
(8, 'capture');

--
-- Role-based access control
--

-- Role
INSERT INTO consent.role (id, name)
VALUES
(1, 'GUEST'),
(2, 'USER'),
(3, 'ADMIN'),
(4, 'PUBLISHER'),
(5, 'SUPERADMIN'),
(6, 'TEST_ROLE'),
(7, 'CUSTOMER_CARE_AGENT');

-- m2m for Role & Permission
INSERT INTO consent.role_permission (role_id, permission_id)
VALUES
(2, 12),
(2, 16),
(2, 21),
(2, 26),
(2, 30),
(2, 34),
(2, 38),
(2, 42),
(3, 12),
(3, 13),
(3, 14),
(3, 15),
(3, 16),
(3, 17),
(3, 19),
(3, 20),
(3, 21),
(3, 22),
(3, 24),
(3, 25),
(3, 26),
(3, 27),
(3, 28),
(3, 29),
(3, 30),
(3, 31),
(3, 32),
(3, 33),
(3, 34),
(3, 35),
(3, 36),
(3, 37),
(3, 38),
(3, 39),
(3, 40),
(3, 41),
(3, 42),
(3, 43),
(3, 44),
(4, 18),
(4, 23),
(4, 12),
(4, 13),
(4, 14),
(4, 15),
(4, 16),
(4, 17),
(4, 19),
(4, 21),
(4, 22),
(4, 24),
(4, 25),
(4, 26),
(4, 27),
(4, 28),
(4, 29),
(4, 30),
(4, 31),
(4, 32),
(4, 33),
(4, 34),
(4, 35),
(4, 36),
(4, 37),
(4, 38),
(4, 39),
(4, 40),
(4, 41),
(4, 42),
(4, 43),
(4, 44),
(5, 12),
(5, 13),
(5, 14),
(5, 15),
(5, 16),
(5, 17),
(5, 18),
(5, 19),
(5, 20),
(5, 21),
(5, 22),
(5, 23),
(5, 24),
(5, 25),
(5, 26),
(5, 27),
(5, 28),
(5, 29),
(5, 30),
(5, 31),
(5, 32),
(5, 33),
(5, 34),
(5, 35),
(5, 36),
(5, 37),
(5, 38),
(5, 39),
(5, 40),
(5, 41),
(5, 42),
(5, 43),
(5, 44),
(6, 45),
(6, 46),
(3, 1),
(7, 47),
(3, 48),
(3, 49),
(5, 50),
(5, 51),
(5, 52),
(5, 53),
(5, 54),
-- Temporal role-permission relations (are going to be removed after owner/product-based filtering is implemented)
(3, 101),
(3, 102),
(4, 101),
(4, 102);
-- Temporal role-permission relations END

-- User
INSERT INTO consent.user (id, username, password, name, email, is_connect_id)
VALUES
(1, 'admin', '$2a$04$aoyxpZjEcFgndDXzFr4/Keg8jUOR6HebjDRVl6u8wUIZM8nlX7D/K', 'admin user (password is "test")', 'payment@telenordigital.com', false),
(2, 'publisher', '$2a$04$KAMdxc/Ydn6i6.PCiSawJOlYLQ2cyJndqUVUg2Qc5dRLWFCCouT5K', 'Translations Publisher (password is "secret")', 'publisher@telenordigital.com', false),
(3, 'test', '$2a$11$XddBZfRgYpJl9pITteAV4esYmE6xDBqHNTV08IL1g8OsSTyi30KpG', 'Test user (password is "pwd")', 'test@telenordigital.com', false),
(4, '9065392725727911111', null, '', '', true),
(5, '9065392725727911112', null, '', '', true);

-- m2m for User & Role
INSERT INTO consent.user_role (user_id, role_id)
VALUES
(1, 3),
(2, 4),
(3, 6),
(4, 3),
(4, 5),
(5, 4),
(1, 7),
(2, 7);

-- m2m for User & Owner
INSERT INTO consent.user_owner (user_id, owner_id)
VALUES
(1, 1),
(1, 6),
(2, 1),
(2, 6),
(3, 1),
(4, 1),
(4, 6),
(5, 1),
(5, 6);

--
-- End: Role-based access control
--

SELECT setval('consent.user_id_seq', (SELECT MAX(id) FROM consent.user)) INTO ignored;

INSERT INTO consent.user_consent_decisions_batch (id, hash, params, owner_id, download_url, status, created_at)
VALUES
  (1, '52009F18A8F88C5E14D5B73A734DE2C06426E81FCF2397A70EA7B060A809BF3C', '{}', 6, '', 'IN_PROGRESS', NOW()),
  (2, '7A928667A2324F791F3C9B23E30D586F072CAD07BE3FAEBC25DD69DAC884CABF', '{}', 6, '', 'IN_PROGRESS', NOW()),
  (3, '7F495A9AEDA0FE77BCC6F9FF5586632331E5D6052EA620E291C1A5AE11B73AA9', '{}', 6, 'url3', 'DONE', NOW()),
  (4, '8fe6480c9ae132068a1c52f970c49542ec8a12a961a2407b958523279e38c550', '{"owner_id": 1, "product_id": 1, "use_case_id": 1, "consent_id": 1, "positive_only": true, "negative_only": false}', 1, 'url4', 'DONE', NOW()),
  (5, '5b76c34e40552abfc37a380ef19e9a1e756ea8d889b18a1ab4ce347a6e55395b', '{"owner_id": 1, "product_id": 2, "use_case_id": 22, "consent_id": 222, "positive_only": true, "negative_only": false}', 1, 'url5', 'DONE', NOW()),
  (6, '5024750f6682cd1ea0bab1bfd29422b532132912cc615b43bc68ad9098cbe026', '{"owner_id": 1, "product_id": 3, "use_case_id": 33, "consent_id": 333, "positive_only": false, "negative_only": true}', 1, 'url6', 'DONE', NOW()),
  (7, '5024750f6682cd1ea0bab1bfd29422b532132912cc615b43bc68ad9098cbe026', '{"owner_id": 1, "product_id": 3, "use_case_id": 33, "consent_id": 333, "positive_only": false, "negative_only": true}', 1, 'url6_1', 'DONE', NOW()),
  (8, '5024750f6682cd1ea0bab1bfd29422b532132912cc615b43bc68ad9098cbe026', '{"owner_id": 1, "product_id": 3, "use_case_id": 33, "consent_id": 333, "positive_only": false, "negative_only": true}', 1, 'url6_2', 'DONE', NOW()),
  (9, '5024750f6682cd1ea0bab1bfd29422b532132912cc615b43bc68ad9098cbe026', '{"owner_id": 1, "product_id": 3, "use_case_id": 33, "consent_id": 333, "positive_only": false, "negative_only": true}', 1, 'url6_3_expired', 'DONE', NOW() - INTERVAL '99 DAYS'),
  (10, '5024750f6682cd1ea0bab1bfd29422b532132912cc615b43bc68ad9098cbe026', '{"owner_id": 1, "product_id": 3, "use_case_id": 33, "consent_id": 333, "positive_only": false, "negative_only": true}', 1, 'url6_4_expired', 'DONE', NOW() - INTERVAL '99 DAYS');

-- Reset 'user_consent_decisions_batch_id_seq' sequence object's counter value.
SELECT setval('consent.user_consent_decisions_batch_id_seq', (SELECT MAX(id) FROM consent.user_consent_decisions_batch)) INTO ucdb_ignored;

INSERT INTO consent.master_id(id, user_id, id_type_id, is_device_id)
VALUES
 ('ecdea009-b706-3365-b882-a13e8386d090', '222', 1, false),
 ('20156a51-f2c5-39dc-9e31-e9f7cc518e7d', '333', 1, false),
 ('df6fb61d-2fdf-3656-88d3-44f34132a97c', '2222', 1, false),
 ('fd6df754-8a47-3212-beeb-c64af7bdedc1', '3333', 1, false),
 ('b4aa64b5-f660-36a2-a876-6c2b80554bf8', '6667043688931143680', 1, false);

insert into consent.skin (id, owner_id, product_id, name, hide_sections) VALUES
(1, 1, 1, 'td1', ARRAY['tour', 'consent', 'review']),
(2, 1, null, 'td2', ARRAY['tour', 'consent']),
(3, 6, 8, 'capture', ARRAY['tour']);
