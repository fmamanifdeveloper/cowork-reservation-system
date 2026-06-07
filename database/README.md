# Base de datos

Scripts de inicialización PostgreSQL para el sistema **CoWork Spaces Reservation System**.

Esta carpeta contiene la estructura de base de datos usada por la aplicación: catálogos, tablas principales, relaciones, índices, restricciones, datos iniciales y comentarios técnicos.

---

## Estructura

```text
database/
├── 000_extensions.sql
├── 001_catalogs.sql
├── 002_schema.sql
├── 003_indexes_constraints.sql
├── 004_seed.sql
├── 005_comments.sql
└── README.md
```

---

## Orden de ejecución

Los scripts están pensados para ejecutarse en orden alfabético.

| Script | Descripción |
|---|---|
| `000_extensions.sql` | Habilita extensiones de PostgreSQL y funciones compartidas. |
| `001_catalogs.sql` | Crea catálogos para roles y estados. |
| `002_schema.sql` | Crea las tablas principales del modelo de negocio. |
| `003_indexes_constraints.sql` | Crea índices, triggers y la restricción anti-overbooking. |
| `004_seed.sql` | Inserta datos iniciales para ejecutar y probar la aplicación. |
| `005_comments.sql` | Agrega comentarios a tablas y columnas relevantes. |

---

## Extensiones utilizadas

| Extensión | Uso |
|---|---|
| `pgcrypto` | Generación de UUIDs y hashes demo mediante `crypt`. |
| `btree_gist` | Soporte para la `exclusion constraint` usada contra solapamientos de reservas. |

---

## Tablas principales

| Tabla | Descripción |
|---|---|
| `app_users` | Cuentas autenticadas del sistema: administradores, personal interno y clientes. |
| `customers` | Perfiles comerciales de clientes que realizan o reciben reservas. |
| `spaces` | Salas o espacios disponibles para reserva. |
| `reservations` | Reservas con cliente, espacio, horario, estado, precio y trazabilidad. |
| `audit_logs` | Registro auditable de eventos importantes del negocio. |

---

## Tablas de catálogo

| Tabla | Descripción |
|---|---|
| `app_user_roles` | Roles de usuario. |
| `app_user_statuses` | Estados de cuenta de usuario. |
| `space_statuses` | Estados operativos de los espacios. |
| `reservation_statuses` | Estados del ciclo de vida de una reserva. |

---

## Roles de usuario

| ID | Code | Texto visible |
|---:|---|---|
| 1 | `Admin` | Administrador |
| 2 | `Staff` | Personal interno |
| 3 | `Customer` | Cliente |

---

## Estados de usuario

| ID | Code | Texto visible |
|---:|---|---|
| 1 | `Active` | Activo |
| 2 | `Inactive` | Inactivo |
| 3 | `Locked` | Bloqueado |

---

## Estados de espacios

| ID | Code | Texto visible |
|---:|---|---|
| 1 | `Active` | Activo |
| 2 | `Maintenance` | Mantenimiento |
| 3 | `Inactive` | Inactivo |

Solo los espacios activos deben estar disponibles para nuevas reservas.

---

## Estados de reservas

| ID | Code | Texto visible |
|---:|---|---|
| 1 | `Pending` | Pendiente |
| 2 | `Confirmed` | Confirmada |
| 3 | `Cancelled` | Cancelada |
| 4 | `Completed` | Completada |

Las reservas canceladas no bloquean la disponibilidad futura.

---

## Modelo de usuarios y clientes

El modelo separa la cuenta de acceso del perfil comercial del cliente.

```text
app_users = cuenta de acceso / login
customers = perfil comercial del cliente
```

La relación se maneja mediante:

```text
app_users.customer_id
```

Reglas principales:

- Los usuarios `Admin` y `Staff` no deben tener `customer_id`.
- Los usuarios `Customer` sí deben tener `customer_id`.
- Un cliente puede tener como máximo una cuenta de usuario.
- Una reserva siempre pertenece comercialmente a un cliente.
- Una reserva puede ser creada por el propio cliente o por un usuario interno.

---

## Escenarios soportados

### Cliente crea su propia reserva

```text
reservations.customer_id = cliente dueño de la reserva
reservations.created_by_user_id = usuario autenticado del cliente
```

Ejemplo:

```text
Juan Pérez inicia sesión y reserva una sala.
La reserva pertenece comercialmente a Juan Pérez.
El registro fue creado por la cuenta de Juan Pérez.
```

### Administrador o personal interno crea una reserva para un cliente

```text
reservations.customer_id = cliente seleccionado
reservations.created_by_user_id = usuario administrador o personal interno
```

Ejemplo:

```text
Recepción crea una reserva para Juan Pérez.
La reserva pertenece comercialmente a Juan Pérez.
El registro fue creado por el usuario de recepción.
```

Esto permite saber quién es el dueño comercial de la reserva y quién registró la operación.

---

## Estrategia de fechas y zonas horarias

Las reservas usan:

```text
TIMESTAMPTZ
```

Los espacios usan:

```text
opening_time TIME
closing_time TIME
time_zone_id VARCHAR(80)
```

Decisión técnica:

- `reservations.start_time` y `reservations.end_time` representan instantes reales.
- `spaces.opening_time` y `spaces.closing_time` representan horarios locales del negocio.
- `spaces.time_zone_id` define la zona horaria local del espacio.
- Por defecto se usa `America/Lima`.

Ejemplo:

```text
opening_time = 08:00
closing_time = 20:00
time_zone_id = America/Lima
```

El backend debe convertir las fechas de reserva a la zona horaria del espacio antes de validar:

- Horario de apertura.
- Horario de cierre.
- Hora pico.
- Fin de semana.
- Reportes agrupados por hora.

---

## Duración de reservas

La base de datos valida la duración mínima y máxima de una reserva.

| Regla | Valor |
|---|---:|
| Duración mínima | 30 minutos |
| Duración máxima | 8 horas |

Restricciones aplicadas:

```sql
CHECK ((end_time - start_time) >= INTERVAL '30 minutes')
CHECK ((end_time - start_time) <= INTERVAL '8 hours')
```

---

## Prevención de overbooking

La prevención de reservas duplicadas o solapadas se garantiza a nivel de base de datos usando una `exclusion constraint` de PostgreSQL.

```sql
EXCLUDE USING gist (
    space_id WITH =,
    tstzrange(start_time, end_time, '[)') WITH &&
)
WHERE (status_id IN (1, 2, 4));
```

Esta restricción impide que existan dos reservas activas para el mismo espacio en horarios cruzados.

El modo de rango `[)` permite reservas consecutivas sin conflicto:

```text
10:00 - 11:00
11:00 - 12:00
```

Estados que bloquean disponibilidad:

```text
Pending
Confirmed
Completed
```

Estado que no bloquea disponibilidad:

```text
Cancelled
```

---

## Estrategia de concurrencia

La estrategia principal para evitar overbooking es:

```text
PostgreSQL Exclusion Constraint
```

Esta garantía vive en la base de datos, por lo que funciona incluso si dos solicitudes llegan al mismo tiempo intentando reservar el mismo espacio y horario.

Además, las tablas principales incluyen:

```text
version INTEGER
```

La columna `version` permite aplicar control optimista de concurrencia en actualizaciones administrativas como:

- Editar un espacio.
- Actualizar datos de cliente.
- Cancelar una reserva.
- Completar una reserva.
- Actualizar datos de usuario.

---

## Motor de tarifas dinámicas

El cálculo de tarifa se almacena de forma auditable en la reserva mediante:

```text
reservations.base_amount
reservations.final_amount
reservations.pricing_breakdown
```

Orden de aplicación de reglas:

| Orden | Regla | Condición | Ajuste |
|---:|---|---|---:|
| 1 | Base | `base_hourly_rate × duración en horas` | — |
| 2 | Hora pico | Reserva entre `09:00–11:00` o `17:00–19:00` | +25% |
| 3 | Fin de semana | Sábado o domingo | +15% |
| 4 | Reserva larga | Duración mayor o igual a 4 horas | -10% |
| 5 | Anticipación | Reserva creada con 7 o más días de anticipación | -5% |

Ejemplo de cálculo:

```text
Base: S/ 100.00

Hora pico +25%
= S/ 125.00

Fin de semana +15%
= S/ 143.75

Reserva larga -10%
= S/ 129.38

Anticipación -5%
= S/ 122.91
```

El detalle del cálculo se guarda en:

```text
reservations.pricing_breakdown JSONB
```

Ejemplo:

```json
{
  "baseAmount": 100.00,
  "finalAmount": 125.00,
  "rules": [
    {
      "rule": "PeakHour",
      "percentage": 0.25,
      "amountBefore": 100.00,
      "amountAfter": 125.00
    }
  ]
}
```

Esto permite auditar por qué una reserva llegó a un determinado precio final.

---

## Cancelaciones y reembolsos

Cuando una reserva se cancela, se actualizan campos como:

```text
status_id
refund_amount
cancelled_at
cancelled_by_user_id
updated_at
version
```

La política de reembolso se calcula en backend según la anticipación de la cancelación.

| Anticipación | Reembolso |
|---|---:|
| Más de 48 horas | 100% |
| Entre 24 y 48 horas | 50% |
| Menos de 24 horas | 0% |

---

## Eliminación lógica

Las entidades administrativas principales usan eliminación lógica.

Campos utilizados:

```text
is_deleted
deleted_at
deleted_by_user_id
```

Aplica principalmente a:

- `app_users`
- `customers`
- `spaces`

Las reservas no deben eliminarse físicamente. Su ciclo de vida se representa mediante estados como `Cancelled` o `Completed`.

---

## Auditoría de negocio

La tabla `audit_logs` registra eventos importantes del negocio.

Ejemplos:

- `ReservationCreated`
- `ReservationCancelled`
- `SpaceCreated`
- `SpaceUpdated`
- `SpaceDeleted`
- `UserLoginSucceeded`
- `UserLoginFailed`

Campos principales:

| Columna | Descripción |
|---|---|
| `event_type` | Nombre del evento de negocio. |
| `entity_type` | Tipo de entidad afectada. |
| `entity_id` | Identificador de la entidad afectada. |
| `actor_user_id` | Usuario autenticado que ejecutó la acción. |
| `actor_customer_id` | Cliente asociado al evento, si aplica. |
| `old_values` | Valores anteriores en formato JSONB. |
| `new_values` | Valores nuevos en formato JSONB. |
| `metadata` | Información adicional del evento. |

---

## Logs técnicos y auditoría

La auditoría de negocio no reemplaza los logs técnicos de aplicación.

Separación recomendada:

```text
Serilog     = logs técnicos, excepciones, requests y diagnósticos.
audit_logs  = eventos de negocio y trazabilidad funcional.
```

---

## Índices importantes

| Índice | Propósito |
|---|---|
| `idx_reservations_space_time` | Consultas de disponibilidad y rangos de tiempo. |
| `idx_reservations_customer` | Historial de reservas por cliente. |
| `idx_reservations_status` | Filtros por estado de reserva. |
| `idx_reservations_created_by_user` | Trazabilidad por usuario creador. |
| `idx_reservations_start_time` | Reportes por rango de fechas. |
| `uq_spaces_name_not_deleted` | Evita nombres duplicados en espacios activos. |
| `uq_customers_email_not_deleted` | Evita correos duplicados en clientes activos. |
| `idx_audit_logs_entity` | Búsqueda de eventos por entidad. |
| `idx_audit_logs_created_at` | Búsqueda de eventos por fecha. |

---

## Datos iniciales

El seed incluye datos mínimos para ejecutar y validar la aplicación localmente:

- Usuario administrador.
- Usuario de recepción.
- Usuario cliente.
- Clientes de ejemplo.
- Espacios de ejemplo.
- Reserva de ejemplo.
- Evento de auditoría de ejemplo.

Usuarios demo:

| Usuario | Rol |
|---|---|
| `admin` | Administrador |
| `staff01` | Personal interno |
| `juan.perez` | Cliente |

Contraseña demo:

```text
Admin123!
```

> Nota: las credenciales demo son solo para entorno de desarrollo local. No deben utilizarse en producción.

---

## Validación local

Listar tablas:

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "\dt"
```

Validar usuarios:

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "SELECT username, role_id, status_id FROM app_users;"
```

Validar espacios:

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "SELECT name, capacity, base_hourly_rate, time_zone_id FROM spaces;"
```

Validar reserva demo:

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "SELECT reservation_code, start_time, end_time, final_amount FROM reservations;"
```

---

## Integridad del modelo

El modelo aplica restricciones de negocio directamente en PostgreSQL para proteger la consistencia de la información. Entre las reglas principales se incluyen duración mínima y máxima de reservas, prevención de solapamientos por espacio, unicidad de cuentas de usuario, unicidad de clientes activos por correo, unicidad de espacios activos por nombre y control del ciclo de vida mediante estados.