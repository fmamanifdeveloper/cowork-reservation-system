-- ============================================================
-- CoWork Spaces — Catalog tables
-- ============================================================

CREATE TABLE IF NOT EXISTS app_user_roles (
    id SMALLINT PRIMARY KEY,
    code VARCHAR(40) NOT NULL UNIQUE,
    label_es VARCHAR(80) NOT NULL
);

CREATE TABLE IF NOT EXISTS app_user_statuses (
    id SMALLINT PRIMARY KEY,
    code VARCHAR(40) NOT NULL UNIQUE,
    label_es VARCHAR(80) NOT NULL
);

CREATE TABLE IF NOT EXISTS space_statuses (
    id SMALLINT PRIMARY KEY,
    code VARCHAR(40) NOT NULL UNIQUE,
    label_es VARCHAR(80) NOT NULL
);

CREATE TABLE IF NOT EXISTS reservation_statuses (
    id SMALLINT PRIMARY KEY,
    code VARCHAR(40) NOT NULL UNIQUE,
    label_es VARCHAR(80) NOT NULL
);

INSERT INTO app_user_roles (id, code, label_es)
VALUES
    (1, 'Admin', 'Administrador'),
    (2, 'Staff', 'Personal interno'),
    (3, 'Customer', 'Cliente')
ON CONFLICT (id) DO UPDATE
SET code = EXCLUDED.code,
    label_es = EXCLUDED.label_es;

INSERT INTO app_user_statuses (id, code, label_es)
VALUES
    (1, 'Active', 'Activo'),
    (2, 'Inactive', 'Inactivo'),
    (3, 'Locked', 'Bloqueado')
ON CONFLICT (id) DO UPDATE
SET code = EXCLUDED.code,
    label_es = EXCLUDED.label_es;

INSERT INTO space_statuses (id, code, label_es)
VALUES
    (1, 'Active', 'Activo'),
    (2, 'Maintenance', 'Mantenimiento'),
    (3, 'Inactive', 'Inactivo')
ON CONFLICT (id) DO UPDATE
SET code = EXCLUDED.code,
    label_es = EXCLUDED.label_es;

INSERT INTO reservation_statuses (id, code, label_es)
VALUES
    (1, 'Pending', 'Pendiente'),
    (2, 'Confirmed', 'Confirmada'),
    (3, 'Cancelled', 'Cancelada'),
    (4, 'Completed', 'Completada')
ON CONFLICT (id) DO UPDATE
SET code = EXCLUDED.code,
    label_es = EXCLUDED.label_es;