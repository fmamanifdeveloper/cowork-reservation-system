# CoWork Spaces Reservation System

Sistema de reservas para espacios de coworking desarrollado como solución full stack. Permite gestionar espacios, clientes, reservas, precios dinámicos, autenticación con JWT, auditoría de negocio, reportes administrativos y prevención de overbooking a nivel de base de datos.

## Funcionalidades principales

- Autenticación JWT con roles `Admin`, `Staff` y `Customer`.
- Gestión administrativa de espacios.
- Gestión administrativa de clientes.
- Creación, cancelación y finalización de reservas.
- Portal público para consulta de espacios y creación de reservas.
- Motor de tarifas dinámicas.
- Política de reembolsos por cancelación.
- Auditoría de eventos de negocio.
- Reportes administrativos.
- Prevención de reservas solapadas mediante PostgreSQL exclusion constraint.
- Pruebas unitarias e integración.

## Tecnologías

### Backend

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- JWT Bearer Authentication
- PostgreSQL
- xUnit

### Frontend

- Angular
- TypeScript
- SCSS

### Infraestructura local

- Docker
- Docker Compose
- PostgreSQL 16

## Estructura del repositorio

```text
cowork-reservation-system/
├── backend/
├── frontend/
├── database/
├── docs/
├── docker-compose.yml
└── README.md