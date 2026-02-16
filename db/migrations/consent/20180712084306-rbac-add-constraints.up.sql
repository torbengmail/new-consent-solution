ALTER TABLE consent.role_permission
  DROP CONSTRAINT IF EXISTS role_permission_role_id_fkey,
  ADD CONSTRAINT role_permission_role_id_fkey
FOREIGN KEY (role_id) REFERENCES consent.role (id) ON DELETE CASCADE,
  DROP CONSTRAINT IF EXISTS role_permission_permission_id_fkey,
  ADD CONSTRAINT role_permission_permission_id_fkey
FOREIGN KEY (permission_id) REFERENCES consent.permission (id) ON DELETE CASCADE,
  DROP CONSTRAINT IF EXISTS role_permission_unique,
  ADD CONSTRAINT role_permission_unique UNIQUE (role_id, permission_id);

ALTER TABLE consent.user_role
  DROP CONSTRAINT IF EXISTS user_role_user_id_fkey,
  ADD CONSTRAINT user_role_user_id_fkey
FOREIGN KEY (user_id) REFERENCES consent."user" (id) ON DELETE CASCADE,
  DROP CONSTRAINT IF EXISTS user_role_role_id_fkey,
  ADD CONSTRAINT user_role_role_id_fkey
FOREIGN KEY (role_id) REFERENCES consent.role (id) ON DELETE CASCADE,
  DROP CONSTRAINT IF EXISTS user_role_unique,
  ADD CONSTRAINT user_role_unique UNIQUE (user_id, role_id);

ALTER TABLE consent.user_owner
  DROP CONSTRAINT IF EXISTS user_owner_user_id_fkey,
  ADD CONSTRAINT user_owner_user_id_fkey
FOREIGN KEY (user_id) REFERENCES consent."user" (id) ON DELETE CASCADE,
  DROP CONSTRAINT IF EXISTS user_owner_owner_id_fkey,
  ADD CONSTRAINT user_owner_owner_id_fkey
FOREIGN KEY (owner_id) REFERENCES data_inventory.owner (id) ON DELETE CASCADE,
  DROP CONSTRAINT IF EXISTS user_owner_unique,
  ADD CONSTRAINT user_owner_unique UNIQUE (user_id, owner_id);