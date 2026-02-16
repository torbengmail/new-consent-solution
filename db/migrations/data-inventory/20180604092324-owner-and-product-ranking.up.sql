-----------------------------------------------------------
-- TICS-4744 Add support for consent ranking and sorting --
-----------------------------------------------------------
ALTER TABLE data_inventory.product ADD product_rank INT DEFAULT 0 NOT NULL;
ALTER TABLE data_inventory.owner ADD owner_rank INT DEFAULT 0 NOT NULL;