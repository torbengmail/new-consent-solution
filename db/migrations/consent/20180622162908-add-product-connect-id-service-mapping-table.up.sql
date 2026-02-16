--------------------------------------------------------------------------------------------------
-- TICS-4767 add support for multiple CONNECT ID services for the same product to the data model
--------------------------------------------------------------------------------------------------
CREATE TABLE consent.product_connect_id
(
  product_id integer NOT NULL,
  connect_id_name text NOT NULL UNIQUE,
  CONSTRAINT product_connect_id_pkey PRIMARY KEY (product_id, connect_id_name),
  CONSTRAINT product_connect_id_product_id_fkey FOREIGN KEY (product_id) REFERENCES data_inventory.product (id)
);

-----------------------------------------------------------------
-- Run manually after migration to copy existing data if needed
-----------------------------------------------------------------
--INSERT INTO consent.product_connect_id (product_id, connect_id_name)
--  SELECT id, connect_id_name
--  FROM data_inventory.product
--  WHERE connect_id_name IS NOT NULL;
