INSERT INTO data_inventory.owner (id, name, registration_country_id, owner_type_id)
SELECT 1, 'Telenor Digital', 155, 1
WHERE NOT EXISTS(
        SELECT 1 FROM data_inventory.owner WHERE name = 'Telenor Digital'
    );

INSERT INTO data_inventory.owner (id, name, registration_country_id, owner_type_id)
SELECT 2, 'Unknown', 155, 1
WHERE NOT EXISTS(
        SELECT 1 FROM data_inventory.owner WHERE name = 'Unknown'
    );

INSERT INTO data_inventory.owner (id, name, registration_country_id, owner_type_id)
VALUES (6, 'Telenor Denmark', 55, 1);

INSERT INTO data_inventory.product (id, name, product_group_id, owner_id)
SELECT 1, 'Connect ID', 1, 1
WHERE NOT EXISTS(
        SELECT 1 FROM data_inventory.product WHERE name = 'Connect ID'
    );

INSERT INTO data_inventory.product (id, name, product_group_id, owner_id)
VALUES (2, 'Mobile Connect', 1, 1),
    (8, 'Capture', 1, 1),
    (18, 'My Telenor Common Core', 1, 1);

INSERT INTO data_inventory.purpose_category (id, name, legal_basis_id)
VALUES (1, 'Entering Contract (Data Processing as Service to User)', 1),
    (2, 'Performance of Contract (Data Processing as Service to User)', 1),
    (3, 'Product Usability Improvement', 1),
    (5, 'Direct Marketing of Own Products (Similar)', 1),
    (6, 'Direct Marketing of Own Products (Non-Similar)', 1),
    (7, 'Direct Marketing of Third Party Products', 1);

INSERT INTO data_inventory.use_case (id, name, product_id, description, purpose_category_id, owner_role_id,
                                     use_case_state_id)
VALUES (1001, 'Use Case 1', 1, 'Use case 1 for Product 1', 1, 1, 1),
    (1002, 'Use Case 2', 1, 'Use case 2 for Product 1', 1, 1, 2),
    (1003, 'Use Case 1', 2, 'Use case 1 for Product 2', 1, 1, 2);
