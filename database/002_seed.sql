INSERT INTO spaces (
    id,
    name,
    capacity,
    base_hourly_rate,
    opening_time,
    closing_time,
    status
)
VALUES
(
    '00000000-0000-0000-0000-000000000001',
    'Meeting Room A',
    8,
    100.00,
    '08:00',
    '20:00',
    1
),
(
    '00000000-0000-0000-0000-000000000002',
    'Meeting Room B',
    12,
    150.00,
    '08:00',
    '20:00',
    1
),
(
    '00000000-0000-0000-0000-000000000003',
    'Focus Room',
    4,
    70.00,
    '09:00',
    '18:00',
    1
),
(
    '00000000-0000-0000-0000-000000000004',
    'Maintenance Room',
    6,
    90.00,
    '08:00',
    '20:00',
    2
)
ON CONFLICT (id) DO NOTHING;