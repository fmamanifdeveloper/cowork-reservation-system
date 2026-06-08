# CoWork Spaces Web

Aplicación frontend desarrollada en Angular para el sistema de reservas de espacios de coworking.

## Funcionalidades

- Portal público para visualizar espacios disponibles.
- Creación de reservas públicas.
- Cálculo de precio estimado antes de reservar.
- Login con JWT.
- Panel interno para Admin y Staff.
- Gestión de espacios.
- Gestión de clientes.
- Gestión de reservas.
- Dashboard administrativo.
- Reportes.
- Auditoría.
- Navegación protegida por roles.

## Roles soportados

| Rol | Acceso |
|---|---|
| Admin | Acceso completo |
| Staff | Gestión operativa |
| Customer | Solo sus reservas |

## Rutas principales

```text
/public/spaces
/public/reservation
/auth/login
/admin/dashboard
/admin/spaces
/admin/customers
/admin/reservations
/admin/reports
/admin/audit-logs
/forbidden