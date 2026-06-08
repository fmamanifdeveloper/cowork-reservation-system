# Database Design

Este documento describe el diseño general de la base de datos del sistema.

---

## Motor

El sistema usa PostgreSQL.

La base de datos se inicializa mediante scripts SQL versionados en:

```text
database/
```

Archivos:

```text
000_extensions.sql
001_catalogs.sql
002_schema.sql
003_indexes_constraints.sql
004_seed.sql
005_comments.sql
```

---

## Objetivo del diseño

El diseño busca cubrir:

- Autenticación.
- Gestión de clientes.
- Gestión de espacios.
- Gestión de reservas.
- Reglas de disponibilidad.
- Prevención de overbooking.
- Auditoría.
- Reportes administrativos.

---

## Entidades principales

```text
app_users
customers
spaces
reservations
audit_logs
```

---

## app_users

Tabla de usuarios autenticables.

Permite login, roles, estado de usuario y vinculación opcional a cliente.

Roles principales:

```text
Admin
Staff
Customer
```

---

## customers

Tabla de clientes.

Puede contener clientes creados desde:

- Panel interno.
- Reserva pública tipo invitado.

Un cliente no necesariamente tiene usuario para login.

---

## spaces

Tabla de espacios reservables.

Cada espacio tiene:

- Nombre.
- Capacidad.
- Tarifa por hora.
- Horario de apertura.
- Horario de cierre.
- Zona horaria.
- Estado.

Estados:

```text
Active
Maintenance
Inactive
```

Solo espacios activos pueden reservarse.

---

## reservations

Tabla central del sistema.

Contiene:

- Espacio reservado.
- Cliente asociado.
- Inicio.
- Fin.
- Estado.
- Precio base.
- Precio final.
- Reembolso.
- Breakdown de pricing.
- Fechas de cancelación/finalización.

Estados:

```text
Pending
Confirmed
Cancelled
Completed
```

---

## audit_logs

Tabla de auditoría funcional.

Registra acciones como:

- Creación de reserva.
- Cancelación.
- Finalización.
- Creación/actualización/eliminación de espacio.
- Login exitoso/fallido.

---

## Disponibilidad

La disponibilidad se calcula consultando reservas por:

```text
space_id
start_time
end_time
status
```

Condición:

```text
start_time < requested_end
AND end_time > requested_start
AND status != Cancelled
```

---

## Prevención de reservas solapadas

La base de datos incluye una restricción para impedir que existan dos reservas activas del mismo espacio con rangos horarios solapados.

La base de datos es la última línea de defensa frente a concurrencia.

---

## Índices

Los índices principales están orientados a:

- Buscar reservas por espacio.
- Buscar reservas por rango temporal.
- Buscar reservas por cliente.
- Consultar disponibilidad.
- Generar reportes.
- Consultar auditoría.

Campos importantes:

```text
space_id
customer_id
start_time
end_time
status
created_at
```

---

## Seeds

El archivo `004_seed.sql` carga datos iniciales:

- Usuarios demo.
- Clientes demo.
- Espacios demo.
- Catálogos básicos.

Usuarios demo:

```text
admin / Admin123!
staff01 / Admin123!
juan.perez / Admin123!
```

---

## Reinicialización

Para reconstruir la base desde cero:

```bash
docker compose down --volumes --remove-orphans
docker compose up -d
```

---

## Decisiones

- Scripts SQL versionados en lugar de migraciones.
- Soft delete para no perder trazabilidad.
- Auditoría funcional en `audit_logs`.
- Restricción en base de datos para evitar overbooking.
