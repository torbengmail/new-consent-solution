-----------------------------------------
-- TICS-4800 Role-based access control --
-----------------------------------------

-- Permission
CREATE TABLE IF NOT EXISTS consent.permission (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE
);

-- Role
CREATE TABLE IF NOT EXISTS consent.role (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL UNIQUE
);

-- m2m for Role & Permission
CREATE TABLE IF NOT EXISTS consent.role_permission (
    role_id INT NOT NULL REFERENCES consent.role (id),
    permission_id INT NOT NULL REFERENCES consent.permission (id)
);

-- User
-- Max length of email value is 320 = {64}@{255}
-- But, according to the RFC 2821 (https://www.ietf.org/rfc/rfc2821.txt):
-- The maximum total length of a reverse-path or forward-path is 256
-- characters (including the punctuation and element separators).
-- VARCHAR(255) has been chosen (255 + 1 byte of metadata - string length)
CREATE TABLE IF NOT EXISTS consent.user (
    id SERIAL PRIMARY KEY,
    username VARCHAR(64) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    name VARCHAR,
    email VARCHAR(255)
);

-- m2m for User & Role
CREATE TABLE IF NOT EXISTS consent.user_role (
    user_id INT NOT NULL REFERENCES consent.user (id),
    role_id INT NOT NULL REFERENCES consent.role (id)
);

-- m2m for User & Owner
CREATE TABLE IF NOT EXISTS consent.user_owner (
    user_id INT NOT NULL REFERENCES consent.user (id),
    owner_id INT NOT NULL REFERENCES data_inventory.owner (id)
);
