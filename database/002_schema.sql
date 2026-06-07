-- ============================================================
-- CoWork Spaces — Main schema
-- ============================================================

-- ============================================================
-- CUSTOMERS
-- Created before app_users because app_users.customer_id references it.
-- ============================================================
CREATE TABLE IF NOT EXISTS customers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    full_name VARCHAR(160) NOT NULL,
    email VARCHAR(160) NULL,
    phone VARCHAR(40) NULL,
    document_number VARCHAR(40) NULL,

    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL,

    created_by_user_id UUID NULL,
    updated_by_user_id UUID NULL,
    deleted_by_user_id UUID NULL,

    version INTEGER NOT NULL DEFAULT 1,

    CONSTRAINT chk_customers_full_name
        CHECK (length(trim(full_name)) > 0)
);

-- ============================================================
-- APP_USERS
-- Authenticated accounts: Admin, Staff and Customer.
-- If role_id = 3, customer_id is required.
-- If role_id <> 3, customer_id must be null.
-- ============================================================
CREATE TABLE IF NOT EXISTS app_users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    customer_id UUID NULL,

    username VARCHAR(120) NOT NULL,
    password_hash VARCHAR(500) NOT NULL,
    display_name VARCHAR(160) NOT NULL,

    role_id SMALLINT NOT NULL,
    status_id SMALLINT NOT NULL DEFAULT 1,

    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    last_login_at TIMESTAMPTZ NULL,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL,

    created_by_user_id UUID NULL,
    updated_by_user_id UUID NULL,
    deleted_by_user_id UUID NULL,

    version INTEGER NOT NULL DEFAULT 1,

    CONSTRAINT uq_app_users_username UNIQUE (username),

    -- One customer can have at most one login account.
    -- PostgreSQL allows multiple NULLs, so admin/staff users are fine.
    CONSTRAINT uq_app_users_customer_id UNIQUE (customer_id),

    CONSTRAINT chk_app_users_customer_role CHECK (
        (role_id = 3 AND customer_id IS NOT NULL) OR
        (role_id <> 3 AND customer_id IS NULL)
    ),

    CONSTRAINT chk_app_users_username
        CHECK (length(trim(username)) > 0),

    CONSTRAINT chk_app_users_display_name
        CHECK (length(trim(display_name)) > 0),

    CONSTRAINT fk_app_users_customer
        FOREIGN KEY (customer_id)
        REFERENCES customers(id),

    CONSTRAINT fk_app_users_role
        FOREIGN KEY (role_id)
        REFERENCES app_user_roles(id),

    CONSTRAINT fk_app_users_status
        FOREIGN KEY (status_id)
        REFERENCES app_user_statuses(id),

    CONSTRAINT fk_app_users_created_by
        FOREIGN KEY (created_by_user_id)
        REFERENCES app_users(id),

    CONSTRAINT fk_app_users_updated_by
        FOREIGN KEY (updated_by_user_id)
        REFERENCES app_users(id),

    CONSTRAINT fk_app_users_deleted_by
        FOREIGN KEY (deleted_by_user_id)
        REFERENCES app_users(id)
);

-- ============================================================
-- Deferred FKs: customers -> app_users
-- Wrapped in DO blocks because ADD CONSTRAINT has no IF NOT EXISTS.
-- ============================================================
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_customers_created_by'
    ) THEN
        ALTER TABLE customers
        ADD CONSTRAINT fk_customers_created_by
            FOREIGN KEY (created_by_user_id)
            REFERENCES app_users(id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_customers_updated_by'
    ) THEN
        ALTER TABLE customers
        ADD CONSTRAINT fk_customers_updated_by
            FOREIGN KEY (updated_by_user_id)
            REFERENCES app_users(id);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_customers_deleted_by'
    ) THEN
        ALTER TABLE customers
        ADD CONSTRAINT fk_customers_deleted_by
            FOREIGN KEY (deleted_by_user_id)
            REFERENCES app_users(id);
    END IF;
END $$;

-- ============================================================
-- SPACES
-- ============================================================
CREATE TABLE IF NOT EXISTS spaces (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    name VARCHAR(120) NOT NULL,
    capacity INTEGER NOT NULL,
    base_hourly_rate NUMERIC(10,2) NOT NULL,

    opening_time TIME NOT NULL,
    closing_time TIME NOT NULL,
    time_zone_id VARCHAR(80) NOT NULL DEFAULT 'America/Lima',

    status_id SMALLINT NOT NULL DEFAULT 1,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL,

    created_by_user_id UUID NULL,
    updated_by_user_id UUID NULL,
    deleted_by_user_id UUID NULL,

    version INTEGER NOT NULL DEFAULT 1,

    CONSTRAINT chk_spaces_name_not_empty
        CHECK (length(trim(name)) > 0),

    CONSTRAINT chk_spaces_capacity
        CHECK (capacity > 0),

    CONSTRAINT chk_spaces_base_hourly_rate
        CHECK (base_hourly_rate > 0),

    CONSTRAINT chk_spaces_schedule
        CHECK (opening_time < closing_time),

    CONSTRAINT chk_spaces_time_zone_id
        CHECK (length(trim(time_zone_id)) > 0),

    CONSTRAINT fk_spaces_status
        FOREIGN KEY (status_id)
        REFERENCES space_statuses(id),

    CONSTRAINT fk_spaces_created_by
        FOREIGN KEY (created_by_user_id)
        REFERENCES app_users(id),

    CONSTRAINT fk_spaces_updated_by
        FOREIGN KEY (updated_by_user_id)
        REFERENCES app_users(id),

    CONSTRAINT fk_spaces_deleted_by
        FOREIGN KEY (deleted_by_user_id)
        REFERENCES app_users(id)
);

-- ============================================================
-- RESERVATIONS
-- customer_id = commercial owner of the reservation.
-- created_by_user_id = authenticated user who created the record.
-- ============================================================
CREATE TABLE IF NOT EXISTS reservations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    reservation_code VARCHAR(40) NOT NULL,

    space_id UUID NOT NULL,
    customer_id UUID NOT NULL,

    created_by_user_id UUID NULL,
    updated_by_user_id UUID NULL,
    cancelled_by_user_id UUID NULL,
    completed_by_user_id UUID NULL,

    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NOT NULL,

    status_id SMALLINT NOT NULL DEFAULT 1,

    base_amount NUMERIC(10,2) NOT NULL,
    final_amount NUMERIC(10,2) NOT NULL,
    refund_amount NUMERIC(10,2) NULL,

    pricing_breakdown JSONB NOT NULL DEFAULT '{}'::jsonb,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NULL,
    cancelled_at TIMESTAMPTZ NULL,
    completed_at TIMESTAMPTZ NULL,

    version INTEGER NOT NULL DEFAULT 1,

    CONSTRAINT uq_reservations_reservation_code
        UNIQUE (reservation_code),

    CONSTRAINT chk_reservations_time_range
        CHECK (start_time < end_time),

    CONSTRAINT chk_reservations_min_duration
        CHECK ((end_time - start_time) >= INTERVAL '30 minutes'),

    CONSTRAINT chk_reservations_max_duration
        CHECK ((end_time - start_time) <= INTERVAL '8 hours'),

    CONSTRAINT chk_reservations_amounts
        CHECK (
            base_amount >= 0
            AND final_amount >= 0
            AND (refund_amount IS NULL OR refund_amount >= 0)
        ),

    CONSTRAINT chk_reservations_pricing_breakdown_is_object
        CHECK (jsonb_typeof(pricing_breakdown) = 'object'),

    CONSTRAINT fk_reservations_space
        FOREIGN KEY (space_id)
        REFERENCES spaces(id),

    CONSTRAINT fk_reservations_customer
        FOREIGN KEY (customer_id)
        REFERENCES customers(id),

    CONSTRAINT fk_reservations_status
        FOREIGN KEY (status_id)
        REFERENCES reservation_statuses(id),

    CONSTRAINT fk_reservations_created_by
        FOREIGN KEY (created_by_user_id)
        REFERENCES app_users(id),

    CONSTRAINT fk_reservations_updated_by
        FOREIGN KEY (updated_by_user_id)
        REFERENCES app_users(id),

    CONSTRAINT fk_reservations_cancelled_by
        FOREIGN KEY (cancelled_by_user_id)
        REFERENCES app_users(id),

    CONSTRAINT fk_reservations_completed_by
        FOREIGN KEY (completed_by_user_id)
        REFERENCES app_users(id)
);

-- ============================================================
-- AUDIT_LOGS
-- Business-level audit trail.
-- Technical logs should be handled separately with application logging.
-- ============================================================
CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    event_type VARCHAR(80) NOT NULL,
    entity_type VARCHAR(80) NOT NULL,
    entity_id UUID NULL,

    actor_user_id UUID NULL,
    actor_customer_id UUID NULL,

    action VARCHAR(80) NOT NULL,
    description VARCHAR(300) NOT NULL,

    old_values JSONB NULL,
    new_values JSONB NULL,
    metadata JSONB NULL,

    ip_address VARCHAR(80) NULL,
    user_agent VARCHAR(300) NULL,

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_audit_logs_event_type
        CHECK (length(trim(event_type)) > 0),

    CONSTRAINT chk_audit_logs_entity_type
        CHECK (length(trim(entity_type)) > 0),

    CONSTRAINT chk_audit_logs_action
        CHECK (length(trim(action)) > 0),

    CONSTRAINT chk_audit_logs_description
        CHECK (length(trim(description)) > 0),

    CONSTRAINT fk_audit_logs_actor_user
        FOREIGN KEY (actor_user_id)
        REFERENCES app_users(id),

    CONSTRAINT fk_audit_logs_actor_customer
        FOREIGN KEY (actor_customer_id)
        REFERENCES customers(id)
);