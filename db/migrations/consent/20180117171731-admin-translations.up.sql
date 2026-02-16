-----------------------------
--TICS-4064 Admin Translations--
-----------------------------
CREATE TABLE IF NOT EXISTS consent.admin_translation (
  "lang_code" VARCHAR(10) PRIMARY KEY,
  "translations" JSONB,
  "augmented_translations" JSONB
);
