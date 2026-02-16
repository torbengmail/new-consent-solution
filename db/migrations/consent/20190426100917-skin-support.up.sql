CREATE TABLE consent.skin (
  id SERIAL PRIMARY KEY,
  owner_id INT NOT NULL REFERENCES data_inventory.owner (id),
  product_id INT NULL REFERENCES data_inventory.product (id),
  name TEXT NOT NULL,
  hide_sections TEXT[] DEFAULT ARRAY[]::TEXT[],
  UNIQUE (owner_id, product_id)
);