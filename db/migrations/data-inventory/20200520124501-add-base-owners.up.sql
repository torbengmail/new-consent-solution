INSERT INTO data_inventory.owner (name,has_bcr,registration_country_id,owner_type_id,owner_rank,default_language)
SELECT 'Telenor Digital',false,155,1,0,'en'
WHERE NOT EXISTS (
        SELECT 1 FROM data_inventory.owner WHERE name='Telenor Digital'
    );

INSERT INTO data_inventory.owner (name,has_bcr,registration_country_id,owner_type_id,owner_rank,default_language)
SELECT 'Unknown',false,155,1,0,'en'
WHERE NOT EXISTS (
        SELECT 1 FROM data_inventory.owner WHERE name='Unknown'
    );


