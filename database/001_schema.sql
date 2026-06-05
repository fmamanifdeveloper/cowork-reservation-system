CREATE EXTENSION IF NOT EXISTS btree_gist;

CREATE TABLE IF NOT EXISTS spaces (
    id UUID PRIMARY KEY,
    name VARCHAR(120) NOT NULL,
    capacity INTEGER NOT NULL,
    base_hourly_rate NUMERIC(10,2) NOT NULL,
    opening_time TIME NOT NULL,
    closing_time TIME NOT NULL,
    status SMALLINT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_spaces_name_not_empty CHECK (length(trim(name)) > 0),
    CONSTRAINT chk_spaces_capacity CHECK (capacity > 0),
    CONSTRAINT chk_spaces_base_hourly_rate CHECK (base_hourly_rate > 0),
    CONSTRAINT chk_spaces_schedule CHECK (opening_time < closing_time),
    CONSTRAINT chk_spaces_status CHECK (status IN (1, 2))
);

CREATE TABLE IF NOT EXISTS reservations (
    id UUID PRIMARY KEY,
    space_id UUID NOT NULL,
    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NOT NULL,
    status SMALLINT NOT NULL,
    base_amount NUMERIC(10,2) NOT NULL,
    final_amount NUMERIC(10,2) NOT NULL,
    refund_amount NUMERIC(10,2) NULL,
    created_at TIMESTAMPTZ NOT NULL,
    cancelled_at TIMESTAMPTZ NULL,
    completed_at TIMESTAMPTZ NULL,

    CONSTRAINT fk_reservations_spaces
        FOREIGN KEY (space_id)
        REFERENCES spaces(id),

    CONSTRAINT chk_reservations_time_range
        CHECK (start_time < end_time),

    CONSTRAINT chk_reservations_min_duration
        CHECK ((end_time - start_time) >= INTERVAL '30 minutes'),

    CONSTRAINT chk_reservations_max_duration
        CHECK ((end_time - start_time) <= INTERVAL '8 hours'),

    CONSTRAINT chk_reservations_status
        CHECK (status IN (1, 2, 3, 4)),

    CONSTRAINT chk_reservations_amounts
        CHECK (base_amount >= 0 AND final_amount >= 0 AND (refund_amount IS NULL OR refund_amount >= 0))
);

CREATE INDEX IF NOT EXISTS idx_spaces_status
ON spaces(status);

CREATE INDEX IF NOT EXISTS idx_reservations_space_time
ON reservations(space_id, start_time, end_time);

CREATE INDEX IF NOT EXISTS idx_reservations_status
ON reservations(status);

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
        WHERE (status IN (1, 2));
    END IF;
END $$;