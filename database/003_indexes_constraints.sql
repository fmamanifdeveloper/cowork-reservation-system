-- ============================================================
-- CoWork Spaces — Indexes, constraints and triggers
-- ============================================================

-- ============================================================
-- APP_USERS
-- ============================================================
CREATE INDEX IF NOT EXISTS idx_app_users_customer_id
ON app_users(customer_id);

CREATE INDEX IF NOT EXISTS idx_app_users_role_status
ON app_users(role_id, status_id);

CREATE INDEX IF NOT EXISTS idx_app_users_is_deleted
ON app_users(is_deleted);

-- ============================================================
-- CUSTOMERS
-- ============================================================
CREATE UNIQUE INDEX IF NOT EXISTS uq_customers_email_not_deleted
ON customers(lower(email))
WHERE email IS NOT NULL AND is_deleted = FALSE;

CREATE INDEX IF NOT EXISTS idx_customers_document_number
ON customers(document_number);

CREATE INDEX IF NOT EXISTS idx_customers_is_deleted
ON customers(is_deleted);

-- ============================================================
-- SPACES
-- ============================================================
CREATE UNIQUE INDEX IF NOT EXISTS uq_spaces_name_not_deleted
ON spaces(lower(name))
WHERE is_deleted = FALSE;

CREATE INDEX IF NOT EXISTS idx_spaces_status
ON spaces(status_id);

CREATE INDEX IF NOT EXISTS idx_spaces_is_deleted
ON spaces(is_deleted);

-- ============================================================
-- RESERVATIONS
-- ============================================================
CREATE INDEX IF NOT EXISTS idx_reservations_space_time
ON reservations(space_id, start_time, end_time);

CREATE INDEX IF NOT EXISTS idx_reservations_customer
ON reservations(customer_id);

CREATE INDEX IF NOT EXISTS idx_reservations_status
ON reservations(status_id);

CREATE INDEX IF NOT EXISTS idx_reservations_created_by_user
ON reservations(created_by_user_id);

CREATE INDEX IF NOT EXISTS idx_reservations_updated_by_user
ON reservations(updated_by_user_id);

CREATE INDEX IF NOT EXISTS idx_reservations_cancelled_by_user
ON reservations(cancelled_by_user_id);

CREATE INDEX IF NOT EXISTS idx_reservations_completed_by_user
ON reservations(completed_by_user_id);

CREATE INDEX IF NOT EXISTS idx_reservations_created_at
ON reservations(created_at);

CREATE INDEX IF NOT EXISTS idx_reservations_start_time
ON reservations(start_time);

-- ============================================================
-- AUDIT_LOGS
-- ============================================================
CREATE INDEX IF NOT EXISTS idx_audit_logs_entity
ON audit_logs(entity_type, entity_id);

CREATE INDEX IF NOT EXISTS idx_audit_logs_actor_user
ON audit_logs(actor_user_id);

CREATE INDEX IF NOT EXISTS idx_audit_logs_actor_customer
ON audit_logs(actor_customer_id);

CREATE INDEX IF NOT EXISTS idx_audit_logs_created_at
ON audit_logs(created_at);

-- ============================================================
-- EXCLUSION CONSTRAINT — anti-overbooking
--
-- Guarantees that active reservations cannot overlap for the same space.
-- '[)' allows 10:00-11:00 and 11:00-12:00 without conflict.
-- Cancelled reservations do not block future reservations.
-- ============================================================
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'excl_reservations_no_overlap'
    ) THEN
        ALTER TABLE reservations
        ADD CONSTRAINT excl_reservations_no_overlap
        EXCLUDE USING gist (
            space_id WITH =,
            tstzrange(start_time, end_time, '[)') WITH &&
        )
        WHERE (status_id IN (1, 2, 4));
    END IF;
END $$;

-- ============================================================
-- TRIGGERS — updated_at + version
-- ============================================================
DROP TRIGGER IF EXISTS trg_app_users_updated_at_version ON app_users;
CREATE TRIGGER trg_app_users_updated_at_version
BEFORE UPDATE ON app_users
FOR EACH ROW
EXECUTE FUNCTION set_updated_at_and_version();

DROP TRIGGER IF EXISTS trg_customers_updated_at_version ON customers;
CREATE TRIGGER trg_customers_updated_at_version
BEFORE UPDATE ON customers
FOR EACH ROW
EXECUTE FUNCTION set_updated_at_and_version();

DROP TRIGGER IF EXISTS trg_spaces_updated_at_version ON spaces;
CREATE TRIGGER trg_spaces_updated_at_version
BEFORE UPDATE ON spaces
FOR EACH ROW
EXECUTE FUNCTION set_updated_at_and_version();

DROP TRIGGER IF EXISTS trg_reservations_updated_at_version ON reservations;
CREATE TRIGGER trg_reservations_updated_at_version
BEFORE UPDATE ON reservations
FOR EACH ROW
EXECUTE FUNCTION set_updated_at_and_version();