# Data Dictionary

Este documento resume las entidades principales del sistema.

---

## app_users

Representa usuarios que pueden autenticarse en el sistema.

| Campo | Descripción |
|---|---|
| `id` | Identificador único del usuario |
| `username` | Nombre de usuario para login |
| `password_hash` | Hash de contraseña |
| `role` | Rol del usuario: Admin, Staff o Customer |
| `status` | Estado del usuario |
| `customer_id` | Cliente vinculado, si aplica |
| `created_at` | Fecha de creación |
| `updated_at` | Fecha de actualización |

---

## customers

Representa clientes registrados o creados desde reservas públicas.

| Campo | Descripción |
|---|---|
| `id` | Identificador único del cliente |
| `full_name` | Nombre completo |
| `email` | Correo |
| `phone` | Teléfono |
| `document_number` | Documento |
| `is_deleted` | Marca de eliminación lógica |
| `created_at` | Fecha de creación |
| `updated_at` | Fecha de actualización |

---

## spaces

Representa espacios reservables.

| Campo | Descripción |
|---|---|
| `id` | Identificador único del espacio |
| `name` | Nombre del espacio |
| `capacity` | Capacidad de personas |
| `base_hourly_rate` | Tarifa por hora |
| `opening_time` | Hora de apertura |
| `closing_time` | Hora de cierre |
| `time_zone_id` | Zona horaria |
| `status` | Active, Maintenance o Inactive |
| `is_deleted` | Marca de eliminación lógica |
| `created_at` | Fecha de creación |
| `updated_at` | Fecha de actualización |

---

## reservations

Representa reservas de espacios.

| Campo | Descripción |
|---|---|
| `id` | Identificador único |
| `reservation_code` | Código visible para cliente y personal |
| `space_id` | Espacio reservado |
| `customer_id` | Cliente asociado |
| `created_by_user_id` | Usuario que creó la reserva, si aplica |
| `start_time` | Inicio de la reserva |
| `end_time` | Fin de la reserva |
| `status` | Pending, Confirmed, Cancelled o Completed |
| `base_amount` | Subtotal antes de ajustes |
| `final_amount` | Total final |
| `refund_amount` | Monto de reembolso si se cancela |
| `pricing_breakdown` | Detalle JSON del cálculo |
| `cancelled_at` | Fecha de cancelación |
| `completed_at` | Fecha de finalización |
| `created_at` | Fecha de creación |
| `updated_at` | Fecha de actualización |

---

## audit_logs

Representa eventos funcionales relevantes.

| Campo | Descripción |
|---|---|
| `id` | Identificador único |
| `event_name` | Nombre del evento |
| `entity_name` | Entidad afectada |
| `entity_id` | ID de la entidad afectada |
| `user_id` | Usuario que realizó la acción |
| `customer_id` | Cliente relacionado, si aplica |
| `action` | Acción ejecutada |
| `message` | Mensaje descriptivo |
| `old_values` | Valores anteriores |
| `new_values` | Valores nuevos |
| `created_at` | Fecha del evento |

---

## Catálogos

| Catálogo | Valores |
|---|---|
| Roles | Admin, Staff, Customer |
| Estado de usuario | Active, Inactive |
| Estado de espacio | Active, Maintenance, Inactive |
| Estado de reserva | Pending, Confirmed, Cancelled, Completed |

---

## Relaciones principales

```text
app_users 1 --- 0..1 customers
customers 1 --- N reservations
spaces    1 --- N reservations
app_users 1 --- N audit_logs
```

---

## Fechas

Las reservas se manejan con `DateTimeOffset`.

Para PostgreSQL `timestamp with time zone`, el backend convierte a UTC antes de consultar o guardar.

---

## Pricing breakdown

El campo `pricing_breakdown` almacena el detalle del cálculo aplicado.

Ejemplo:

```json
{
  "baseAmount": 75,
  "finalAmount": 93.75,
  "adjustments": [
    {
      "rule": "PeakHour",
      "percentage": 0.25,
      "amountBefore": 75,
      "amountAfter": 93.75
    }
  ]
}
```
