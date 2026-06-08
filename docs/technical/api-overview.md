# API Overview

Este documento resume los endpoints principales de la API del sistema **CoWork Spaces Reservation System**.

La API está construida con ASP.NET Core Web API y expone endpoints públicos para consulta/reserva de espacios, además de endpoints protegidos para administración interna.

## Base local

```text
https://localhost:7011/api
```

Documentación interactiva:

```text
https://localhost:7011/scalar/v1
```

OpenAPI JSON:

```text
https://localhost:7011/openapi/v1.json
```

---

## Autenticación

### Login

```http
POST /api/auth/login
```

Ejemplo:

```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

### Usuario actual

```http
GET /api/auth/me
Authorization: Bearer {token}
```

---

## Endpoints públicos

No requieren autenticación.

### Listar espacios públicos

```http
GET /api/public/spaces
```

Devuelve espacios disponibles para reserva pública.

### Consultar disponibilidad por espacio y fecha

```http
GET /api/public/spaces/{spaceId}/availability?date=2026-06-08
```

Devuelve bloques de 30 minutos indicando si cada bloque está disponible u ocupado.

### Preview de precio

```http
POST /api/public/pricing/preview
```

Ejemplo:

```json
{
  "spaceId": "00000000-0000-0000-0000-000000000001",
  "startTime": "2026-06-08T09:00:00-05:00",
  "endTime": "2026-06-08T10:30:00-05:00"
}
```

### Crear reserva pública

```http
POST /api/public/reservations
```

Ejemplo:

```json
{
  "spaceId": "00000000-0000-0000-0000-000000000001",
  "customerFullName": "Juan Pérez",
  "customerEmail": "juan.perez@example.com",
  "customerPhone": "999999999",
  "customerDocumentNumber": "12345678",
  "startTime": "2026-06-08T09:00:00-05:00",
  "endTime": "2026-06-08T10:30:00-05:00"
}
```

---

## Endpoints protegidos

Requieren JWT.

```http
Authorization: Bearer {token}
```

### Espacios

```http
GET    /api/spaces
GET    /api/spaces/{id}
POST   /api/spaces
PUT    /api/spaces/{id}
DELETE /api/spaces/{id}
```

### Clientes

```http
GET    /api/customers
GET    /api/customers/{id}
POST   /api/customers
PUT    /api/customers/{id}
DELETE /api/customers/{id}
```

### Reservas

```http
GET  /api/reservations
GET  /api/reservations/{id}
POST /api/reservations
POST /api/reservations/{id}/cancel
POST /api/reservations/{id}/complete
```

### Reportes

```http
GET /api/reports
GET /api/reports?from=2026-06-01T00:00:00Z&to=2026-06-30T23:59:59Z
```

Devuelve total de reservas, ingresos, ocupación por espacio, hora más demandada y demanda por hora.

### Auditoría

```http
GET /api/audit-logs
```

Devuelve eventos funcionales del sistema.

---

## Códigos HTTP principales

| Código | Uso |
|---:|---|
| 200 | Operación exitosa |
| 201 | Recurso creado |
| 400 | Datos inválidos o regla de negocio incumplida |
| 401 | No autenticado |
| 403 | Sin permisos |
| 404 | Recurso no encontrado |
| 409 | Conflicto de reserva u overbooking |
| 500 | Error inesperado |

---

## Colección manual

Las pruebas manuales están en:

```text
docs/requests/requests.http
```
