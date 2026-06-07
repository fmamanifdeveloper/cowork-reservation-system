-- ============================================================
-- CoWork Spaces — Seed data
-- Demo password for all users: Admin123!
-- Hash generated with pgcrypto crypt('Admin123!', gen_salt('bf', 12)).
-- ============================================================

-- ============================================================
-- USERS: Admin and Staff
-- ============================================================
INSERT INTO app_users (
    id,
    customer_id,
    username,
    password_hash,
    display_name,
    role_id,
    status_id,
    is_active,
    is_deleted,
    created_at
)
VALUES
(
    '00000000-0000-0000-0000-000000000100',
    NULL,
    'admin',
    crypt('Admin123!', gen_salt('bf', 12)),
    'Administrador General',
    1,
    1,
    TRUE,
    FALSE,
    NOW()
),
(
    '00000000-0000-0000-0000-000000000101',
    NULL,
    'staff01',
    crypt('Admin123!', gen_salt('bf', 12)),
    'Recepcionista',
    2,
    1,
    TRUE,
    FALSE,
    NOW()
)
ON CONFLICT (username) DO UPDATE
SET password_hash = EXCLUDED.password_hash,
    display_name = EXCLUDED.display_name,
    role_id = EXCLUDED.role_id,
    status_id = EXCLUDED.status_id,
    is_active = EXCLUDED.is_active,
    is_deleted = EXCLUDED.is_deleted;

-- ============================================================
-- CUSTOMERS
-- ============================================================
INSERT INTO customers (
    id,
    full_name,
    email,
    phone,
    document_number,
    is_deleted,
    created_by_user_id,
    created_at
)
VALUES
(
    '00000000-0000-0000-0000-000000000201',
    'Juan Pérez',
    'juan.perez@example.com',
    '999888777',
    '70000001',
    FALSE,
    '00000000-0000-0000-0000-000000000100',
    NOW()
),
(
    '00000000-0000-0000-0000-000000000202',
    'María Rodríguez',
    'maria.rodriguez@example.com',
    '999888778',
    '70000002',
    FALSE,
    '00000000-0000-0000-0000-000000000100',
    NOW()
)
ON CONFLICT (id) DO UPDATE
SET full_name = EXCLUDED.full_name,
    email = EXCLUDED.email,
    phone = EXCLUDED.phone,
    document_number = EXCLUDED.document_number,
    is_deleted = EXCLUDED.is_deleted,
    updated_at = NOW(),
    updated_by_user_id = '00000000-0000-0000-0000-000000000100';

-- ============================================================
-- CUSTOMER LOGIN ACCOUNT
-- ============================================================
INSERT INTO app_users (
    id,
    customer_id,
    username,
    password_hash,
    display_name,
    role_id,
    status_id,
    is_active,
    is_deleted,
    created_by_user_id,
    created_at
)
VALUES
(
    '00000000-0000-0000-0000-000000000301',
    '00000000-0000-0000-0000-000000000201',
    'juan.perez',
    crypt('Admin123!', gen_salt('bf', 12)),
    'Juan Pérez',
    3,
    1,
    TRUE,
    FALSE,
    '00000000-0000-0000-0000-000000000100',
    NOW()
)
ON CONFLICT (username) DO UPDATE
SET password_hash = EXCLUDED.password_hash,
    display_name = EXCLUDED.display_name,
    role_id = EXCLUDED.role_id,
    status_id = EXCLUDED.status_id,
    is_active = EXCLUDED.is_active,
    is_deleted = EXCLUDED.is_deleted,
    customer_id = EXCLUDED.customer_id,
    updated_at = NOW(),
    updated_by_user_id = '00000000-0000-0000-0000-000000000100';

-- ============================================================
-- SPACES
-- ============================================================
INSERT INTO spaces (
    id,
    name,
    capacity,
    base_hourly_rate,
    opening_time,
    closing_time,
    time_zone_id,
    status_id,
    is_deleted,
    created_by_user_id,
    created_at
)
VALUES
(
    '00000000-0000-0000-0000-000000000001',
    'Sala Amazonia',
    10,
    50.00,
    '08:00',
    '22:00',
    'America/Lima',
    1,
    FALSE,
    '00000000-0000-0000-0000-000000000100',
    NOW()
),
(
    '00000000-0000-0000-0000-000000000002',
    'Sala Pacífico',
    6,
    35.00,
    '08:00',
    '20:00',
    'America/Lima',
    1,
    FALSE,
    '00000000-0000-0000-0000-000000000100',
    NOW()
),
(
    '00000000-0000-0000-0000-000000000003',
    'Sala Titicaca',
    20,
    80.00,
    '09:00',
    '22:00',
    'America/Lima',
    1,
    FALSE,
    '00000000-0000-0000-0000-000000000100',
    NOW()
),
(
    '00000000-0000-0000-0000-000000000004',
    'Oficina Privada',
    4,
    25.00,
    '08:00',
    '18:00',
    'America/Lima',
    1,
    FALSE,
    '00000000-0000-0000-0000-000000000100',
    NOW()
),
(
    '00000000-0000-0000-0000-000000000005',
    'Sala Conferencias',
    30,
    120.00,
    '07:00',
    '23:00',
    'America/Lima',
    1,
    FALSE,
    '00000000-0000-0000-0000-000000000100',
    NOW()
),
(
    '00000000-0000-0000-0000-000000000006',
    'Sala en Mantenimiento',
    8,
    60.00,
    '08:00',
    '20:00',
    'America/Lima',
    2,
    FALSE,
    '00000000-0000-0000-0000-000000000100',
    NOW()
)
ON CONFLICT (id) DO UPDATE
SET name = EXCLUDED.name,
    capacity = EXCLUDED.capacity,
    base_hourly_rate = EXCLUDED.base_hourly_rate,
    opening_time = EXCLUDED.opening_time,
    closing_time = EXCLUDED.closing_time,
    time_zone_id = EXCLUDED.time_zone_id,
    status_id = EXCLUDED.status_id,
    is_deleted = EXCLUDED.is_deleted,
    updated_at = NOW(),
    updated_by_user_id = '00000000-0000-0000-0000-000000000100';

-- ============================================================
-- SAMPLE RESERVATION
-- Uses local Peru time offset -05:00.
-- ============================================================
INSERT INTO reservations (
    id,
    reservation_code,
    space_id,
    customer_id,
    created_by_user_id,
    start_time,
    end_time,
    status_id,
    base_amount,
    final_amount,
    refund_amount,
    pricing_breakdown,
    created_at
)
VALUES
(
    '00000000-0000-0000-0000-000000000401',
    'RSV-202606-0001',
    '00000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000201',
    '00000000-0000-0000-0000-000000000301',
    '2026-06-12 14:00:00-05',
    '2026-06-12 16:00:00-05',
    2,
    100.00,
    100.00,
    NULL,
    '{
        "baseAmount": 100.00,
        "finalAmount": 100.00,
        "rules": []
    }'::jsonb,
    NOW()
)
ON CONFLICT (reservation_code) DO NOTHING;

-- ============================================================
-- SAMPLE AUDIT LOGS
-- ============================================================
INSERT INTO audit_logs (
    event_type,
    entity_type,
    entity_id,
    actor_user_id,
    actor_customer_id,
    action,
    description,
    old_values,
    new_values,
    metadata,
    created_at
)
VALUES
(
    'ReservationCreated',
    'Reservation',
    '00000000-0000-0000-0000-000000000401',
    '00000000-0000-0000-0000-000000000301',
    '00000000-0000-0000-0000-000000000201',
    'Create',
    'Reserva de ejemplo creada por el cliente.',
    NULL,
    '{
        "reservationCode": "RSV-202606-0001",
        "status": "Confirmada",
        "finalAmount": 100.00
    }'::jsonb,
    '{
        "source": "seed"
    }'::jsonb,
    NOW()
);