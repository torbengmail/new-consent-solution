INSERT INTO data_inventory.product (name,product_group_id,description,owner_id,is_visible,product_rank)
SELECT 'Connect ID',1,'Authentiction solution. Allows developers to add email and phone number based authentication to their products.',(SELECT id FROM data_inventory.owner WHERE name='Telenor Digital'),true,0
WHERE NOT EXISTS (
        SELECT 1 FROM data_inventory.product WHERE name='Connect ID'
    );